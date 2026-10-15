# The EzyHouse API. Built and run by ../docker-compose.yml; see README.md.

# Build: restore first from the project files alone, so editing code doesn't re-download packages.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/EzyHouse.Domain/EzyHouse.Domain.csproj src/EzyHouse.Domain/
COPY src/EzyHouse.Infrastructure/EzyHouse.Infrastructure.csproj src/EzyHouse.Infrastructure/
COPY src/EzyHouse.Api/EzyHouse.Api.csproj src/EzyHouse.Api/
RUN dotnet restore src/EzyHouse.Api/EzyHouse.Api.csproj
COPY src/ src/
RUN dotnet publish src/EzyHouse.Api/EzyHouse.Api.csproj -c Release -o /app --no-restore

# Run: the ASP.NET runtime only, as the image's non-root user, listening on port 8080.
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "EzyHouse.Api.dll"]
