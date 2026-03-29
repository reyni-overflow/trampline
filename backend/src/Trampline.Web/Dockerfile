FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

RUN apt-get update && apt-get install -y --no-install-recommends libkrb5-3 curl && rm -rf /var/lib/apt/lists/* \
    && mkdir -p /app/data-protection-keys /app/files \
    && chown $APP_UID /app/data-protection-keys /app/files

USER $APP_UID
WORKDIR /app

EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/Trampline.Core/Trampline.Core.csproj", "src/Trampline.Core/"]
COPY ["src/Trampline.Application/Trampline.Application.csproj", "src/Trampline.Application/"]
COPY ["src/Trampline.Infrastructure.Redis/Trampline.Infrastructure.Redis.csproj", "src/Trampline.Infrastructure.Redis/"]
COPY ["src/Trampline.Infrastructure.Postgres/Trampline.Infrastructure.Postgres.csproj", "src/Trampline.Infrastructure.Postgres/"]
COPY ["src/Trampline.Shared/Trampline.Shared.csproj", "src/Trampline.Shared/"]
COPY ["src/Trampline.Contracts/Trampline.Contracts.csproj", "src/Trampline.Contracts/"]
COPY ["src/Trampline.Web/Trampline.Web.csproj", "src/Trampline.Web/"]

RUN dotnet restore "src/Trampline.Web/Trampline.Web.csproj"

COPY . .

WORKDIR "/src/src/Trampline.Web"
RUN dotnet build "./Trampline.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish

ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Trampline.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .
HEALTHCHECK --interval=30s --timeout=3s --start-period=15s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Trampline.Web.dll"]
