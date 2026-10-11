var builder = WebApplication.CreateBuilder(args);

// Database: pin the server version so design-time tools don't need a live connection
builder.Services.AddDbContext<EzyHouseDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("EzyHouse"),
                     new MySqlServerVersion(new Version(8, 0, 36))));

// JSON: camelCase (the default) and lowercase camelCase enum strings, as API.md requires
builder.Services.AddControllers().AddJsonOptions(o =>
    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));

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
builder.Services.AddAuthorization();

// CORS for the React dev server
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()!)
    .AllowAnyHeader().AllowAnyMethod()));

// Application services (see Section 5)
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddSingleton<ISplitStrategy, EqualSplitStrategy>();
builder.Services.AddSingleton<ISplitStrategy, ExactSplitStrategy>();
builder.Services.AddSingleton<SplitCalculator>();
builder.Services.AddSingleton<BalanceCalculator>();
builder.Services.AddScoped<ICurrentUserContext, CurrentUserContext>();
builder.Services.AddScoped<IHouseholdService, HouseholdService>();
// ... one AddScoped line per service interface

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();   // plus a bearer security definition, so Swagger UI can send tokens

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<EzyHouseDbContext>();
    db.Database.Migrate();          // create or upgrade the schema
    DemoSeeder.Seed(db);            // insert demo data if the database is empty
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();