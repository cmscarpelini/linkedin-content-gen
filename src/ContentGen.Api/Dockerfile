# syntax=docker/dockerfile:1
# Build context is the repository root: docker build -f src/ContentGen.Api/Dockerfile .

# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

# Restore first (cached while the .csproj files don't change)
COPY src/ContentGen.Api/ContentGen.Api.csproj src/ContentGen.Api/
COPY src/ContentGen.Application/ContentGen.Application.csproj src/ContentGen.Application/
COPY src/ContentGen.Domain/ContentGen.Domain.csproj src/ContentGen.Domain/
COPY src/ContentGen.Infrastructure/ContentGen.Infrastructure.csproj src/ContentGen.Infrastructure/
RUN dotnet restore src/ContentGen.Api/ContentGen.Api.csproj

# Publish
COPY src/ ./src/
RUN dotnet publish src/ContentGen.Api/ContentGen.Api.csproj -c Release -o /app --no-restore

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# curl is used by the container HEALTHCHECK below
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app ./

# Keep the SQLite database in a dedicated directory so it can be mounted as a volume
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/contentgen.db"
RUN mkdir -p /app/data

EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=3s --start-period=15s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "ContentGen.Api.dll"]
