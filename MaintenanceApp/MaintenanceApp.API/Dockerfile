# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["MaintenanceApp.API/Maintenance.API.csproj", "MaintenanceApp.API/"]
COPY ["Maintenance.Domain/Maintenance.Domain.csproj", "Maintenance.Domain/"]
COPY ["Maintenance.Infrastructure/Maintenance.Infrastructure.csproj", "Maintenance.Infrastructure/"]
COPY ["Maintenance.Persistence/Maintenance.Persistence.csproj", "Maintenance.Persistence/"]
COPY ["Maintenance.Repository/Maintenance.Repository.csproj", "Maintenance.Repository/"]

RUN dotnet restore "MaintenanceApp.API/Maintenance.API.csproj"

COPY . .
RUN dotnet publish "MaintenanceApp.API/Maintenance.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Maintenance.API.dll"]
