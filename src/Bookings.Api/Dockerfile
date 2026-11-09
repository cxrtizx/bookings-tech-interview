# Build context: repository root (see docker-compose.yml).
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore first, copying only project files, so the restore layer is cached until a .csproj changes.
COPY global.json ./
COPY src/Bookings.Domain/Bookings.Domain.csproj src/Bookings.Domain/
COPY src/Bookings.Contracts/Bookings.Contracts.csproj src/Bookings.Contracts/
COPY src/Bookings.Application/Bookings.Application.csproj src/Bookings.Application/
COPY src/Bookings.Infrastructure/Bookings.Infrastructure.csproj src/Bookings.Infrastructure/
COPY src/Bookings.Api/Bookings.Api.csproj src/Bookings.Api/
RUN dotnet restore src/Bookings.Api/Bookings.Api.csproj

COPY src/ src/
RUN dotnet publish src/Bookings.Api/Bookings.Api.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
# The .NET 8 images listen on port 8080 and run as a non-root user by default.
EXPOSE 8080
ENTRYPOINT ["dotnet", "Bookings.Api.dll"]
