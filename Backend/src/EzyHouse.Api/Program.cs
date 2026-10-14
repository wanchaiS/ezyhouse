using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using EzyHouse.Api.Core;
using EzyHouse.Api.Features.Auth;
using EzyHouse.Domain.Entities;
using EzyHouse.Domain.Balances;
using EzyHouse.Domain.Interfaces;
using EzyHouse.Domain.Splitting;
using EzyHouse.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Database: pin the server version so design-time tools don't need a live connection
builder.Services.AddDbContext<EzyHouseDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("EzyHouse"),
                     new MySqlServerVersion(new Version(8, 0, 36))));

// JSON: camelCase (the default) and lowercase camelCase enum strings, as API.md requires
builder.Services.AddControllers().AddJsonOptions(o =>
    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false)));

// Errors: AppException subclasses become ProblemDetails responses
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<AppExceptionHandler>();

// Authentication: JWT bearer tokens
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;   // keep "sub" as "sub"
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
        };
    });
// Every endpoint needs a signed-in user unless it opts out with [AllowAnonymous]
builder.Services.AddAuthorization(o => o.FallbackPolicy = new AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser()
    .Build());

// CORS for the React dev server
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()!)
    .AllowAnyHeader().AllowAnyMethod()));

// Application services (see Section 5)
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddSingleton<ISplitStrategy, EqualSplitStrategy>();
builder.Services.AddSingleton<ISplitStrategy, ExactSplitStrategy>();
builder.Services.AddSingleton<SplitCalculator>();
builder.Services.AddSingleton<BalanceCalculator>();
builder.Services.AddScoped<ICurrentUserContext, CurrentUserContext>();

// Feature slices: each registers its own services
builder.Services.AddAuthFeature(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
// Bearer security definition, so Swagger UI's Authorize button can send tokens
builder.Services.AddSwaggerGen(o =>
{
    o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste the JWT returned when signing in.",
    });
    o.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = [],
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<EzyHouseDbContext>();
    db.Database.Migrate();          // create or upgrade the schema
    await DemoSeeder.SeedAsync(     // demo users and data, only when the database is empty
        db,
        scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>(),
        scope.ServiceProvider.GetRequiredService<IClock>().Today);
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
