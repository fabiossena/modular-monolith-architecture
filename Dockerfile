# syntax=docker/dockerfile:1

# ---------------------------------------------------------------------------
# Modular Monolith Store — one image, one process, one database.
#
#   build : docker build -t modular-monolith-store .
#   run   : docker run --rm -p 5088:8080 -v modular-data:/app/data modular-monolith-store
#
# The image contains every module of the solution. They are separate projects
# with separate assemblies, but they are published together into a single
# deployable unit: a modular monolith is still ONE process.
# ---------------------------------------------------------------------------

# ---------- build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0-noble AS build
WORKDIR /src

# Restore first, on its own layer: as long as the .csproj files do not change,
# the NuGet packages stay cached in the image and the build is fast.
COPY ModularMonolith.sln ./

COPY src/Shared/Kernel/ModularMonolith.Shared.Kernel/ModularMonolith.Shared.Kernel.csproj src/Shared/Kernel/ModularMonolith.Shared.Kernel/
COPY src/Shared/Platform/ModularMonolith.Shared.Platform/ModularMonolith.Shared.Platform.csproj src/Shared/Platform/ModularMonolith.Shared.Platform/
COPY src/Modules/Catalog/ModularMonolith.Modules.Catalog/ModularMonolith.Modules.Catalog.csproj src/Modules/Catalog/ModularMonolith.Modules.Catalog/
COPY src/Modules/Catalog/ModularMonolith.Modules.Catalog.Contracts/ModularMonolith.Modules.Catalog.Contracts.csproj src/Modules/Catalog/ModularMonolith.Modules.Catalog.Contracts/
COPY src/Modules/Inventory/ModularMonolith.Modules.Inventory/ModularMonolith.Modules.Inventory.csproj src/Modules/Inventory/ModularMonolith.Modules.Inventory/
COPY src/Modules/Inventory/ModularMonolith.Modules.Inventory.Contracts/ModularMonolith.Modules.Inventory.Contracts.csproj src/Modules/Inventory/ModularMonolith.Modules.Inventory.Contracts/
COPY src/Modules/Cart/ModularMonolith.Modules.Cart/ModularMonolith.Modules.Cart.csproj src/Modules/Cart/ModularMonolith.Modules.Cart/
COPY src/Modules/Cart/ModularMonolith.Modules.Cart.Contracts/ModularMonolith.Modules.Cart.Contracts.csproj src/Modules/Cart/ModularMonolith.Modules.Cart.Contracts/
COPY src/Modules/Payments/ModularMonolith.Modules.Payments/ModularMonolith.Modules.Payments.csproj src/Modules/Payments/ModularMonolith.Modules.Payments/
COPY src/Modules/Payments/ModularMonolith.Modules.Payments.Contracts/ModularMonolith.Modules.Payments.Contracts.csproj src/Modules/Payments/ModularMonolith.Modules.Payments.Contracts/
COPY src/Modules/Orders/ModularMonolith.Modules.Orders/ModularMonolith.Modules.Orders.csproj src/Modules/Orders/ModularMonolith.Modules.Orders/
COPY src/Modules/Orders/ModularMonolith.Modules.Orders.Contracts/ModularMonolith.Modules.Orders.Contracts.csproj src/Modules/Orders/ModularMonolith.Modules.Orders.Contracts/
COPY src/Modules/Identity/ModularMonolith.Modules.Identity/ModularMonolith.Modules.Identity.csproj src/Modules/Identity/ModularMonolith.Modules.Identity/
COPY src/Modules/Identity/ModularMonolith.Modules.Identity.Contracts/ModularMonolith.Modules.Identity.Contracts.csproj src/Modules/Identity/ModularMonolith.Modules.Identity.Contracts/
COPY src/Host/ModularMonolith.Host/ModularMonolith.Host.csproj src/Host/ModularMonolith.Host/

RUN dotnet restore src/Host/ModularMonolith.Host/ModularMonolith.Host.csproj

COPY . .

RUN dotnet publish src/Host/ModularMonolith.Host/ModularMonolith.Host.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

# ---------- runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0-noble AS runtime
WORKDIR /app

# curl is used by the container HEALTHCHECK below.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

# The SQLite database lives here so it can be mounted on a volume
# and survive container restarts / image upgrades.
RUN mkdir -p /app/data && chown -R app:app /app
VOLUME ["/app/data"]

COPY --from=build --chown=app:app /app/publish ./

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    ConnectionStrings__Sqlite="Data Source=/app/data/modular-monolith.db"

EXPOSE 8080

HEALTHCHECK --interval=15s --timeout=5s --start-period=20s --retries=5 \
    CMD curl -fsS http://localhost:8080/health || exit 1

# Run as the non-root "app" user provided by the base image.
USER app

ENTRYPOINT ["dotnet", "ModularMonolith.Host.dll"]
