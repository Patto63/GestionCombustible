# Etapa base
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Etapa de compilacion
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar archivos de proyecto
COPY ["src/VehicleService.API/VehicleService.API.csproj", "src/VehicleService.API/"]
COPY ["src/VehicleService.Application/VehicleService.Application.csproj", "src/VehicleService.Application/"]
COPY ["src/VehicleService.Domain/VehicleService.Domain.csproj", "src/VehicleService.Domain/"]
COPY ["src/VehicleService.Infrastructure/VehicleService.Infrastructure.csproj", "src/VehicleService.Infrastructure/"]
COPY ["src/VehicleService.Persistence/VehicleService.Persistence.csproj", "src/VehicleService.Persistence/"]
COPY ["shared/Shared.Security/Shared.Security.csproj", "shared/Shared.Security/"]

# Copiar todo el codigo fuente
COPY . .

# Cambiar al directorio del proyecto API
WORKDIR "/src/src/VehicleService.API"

# Restaurar dependencias
RUN dotnet restore

# Compilar
RUN dotnet build -c Release -o /app/build

# Publicar
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Etapa final
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "VehicleService.API.dll"]