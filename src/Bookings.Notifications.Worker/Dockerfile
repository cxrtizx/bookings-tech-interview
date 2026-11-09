# Build context: repository root (see docker-compose.yml).
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# The worker only depends on Contracts, never on the bookings domain.
COPY global.json ./
COPY src/Bookings.Contracts/Bookings.Contracts.csproj src/Bookings.Contracts/
COPY src/Bookings.Notifications.Worker/Bookings.Notifications.Worker.csproj src/Bookings.Notifications.Worker/
RUN dotnet restore src/Bookings.Notifications.Worker/Bookings.Notifications.Worker.csproj

COPY src/Bookings.Contracts/ src/Bookings.Contracts/
COPY src/Bookings.Notifications.Worker/ src/Bookings.Notifications.Worker/
RUN dotnet publish src/Bookings.Notifications.Worker/Bookings.Notifications.Worker.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Bookings.Notifications.Worker.dll"]
