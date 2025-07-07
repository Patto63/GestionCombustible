FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/ApiGateway.API/ApiGateway.API.csproj", "src/ApiGateway.API/"]
RUN dotnet restore "src/ApiGateway.API/ApiGateway.API.csproj"
COPY . .
WORKDIR "/src/src/ApiGateway.API"
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ApiGateway.API.dll"]
