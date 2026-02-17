# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Comanda.Api/Comanda.Api.csproj", "Comanda.Api/"]
COPY ["Comanda.Core/Comanda.Core.csproj", "Comanda.Core/"]
COPY ["Comanda.Infrastructure/Comanda.Infrastructure.csproj", "Comanda.Infrastructure/"]
COPY Comanda.sln .

RUN dotnet restore "Comanda.Api/Comanda.Api.csproj"
COPY . .
RUN dotnet publish "Comanda.Api/Comanda.Api.csproj" -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Render inyecta PORT en runtime; el shell expande la variable al iniciar
EXPOSE 5000
ENTRYPOINT ["sh", "-c", "dotnet Comanda.Api.dll --urls http://0.0.0.0:${PORT:-5000}"]
