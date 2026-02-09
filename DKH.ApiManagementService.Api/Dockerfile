# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5012

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1 \
  DOTNET_CLI_TELEMETRY_OPTOUT=1

RUN if [ "$(uname -m)" = "aarch64" ] || [ "$(uname -m)" = "arm64" ]; then \
  echo "==> Installing system protoc for arm64 to fix Grpc.Tools segfault"; \
  apt-get update && \
  apt-get install -y --no-install-recommends protobuf-compiler && \
  rm -rf /var/lib/apt/lists/* && \
  protoc --version; \
  else \
  echo "==> Using bundled Grpc.Tools protoc on $(uname -m)"; \
  fi

WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["nuget.config", "."]
COPY ["project.props", "."]

RUN --mount=type=secret,id=github_nuget_username \
    --mount=type=secret,id=github_nuget_token \
    if [ ! -s /run/secrets/github_nuget_username ] || [ ! -s /run/secrets/github_nuget_token ]; then \
    echo "Missing GitHub NuGet secrets (GITHUB_NUGET_USERNAME/TOKEN)" && exit 1; \
    fi && \
    dotnet nuget update source github-dotnet-gzdkh \
    --username "$(cat /run/secrets/github_nuget_username)" \
    --password "$(cat /run/secrets/github_nuget_token)" \
    --store-password-in-clear-text \
    --configfile nuget.config

COPY ["DKH.ApiManagementService.Api/DKH.ApiManagementService.Api.csproj", "DKH.ApiManagementService.Api/"]
COPY ["DKH.ApiManagementService.Application/DKH.ApiManagementService.Application.csproj", "DKH.ApiManagementService.Application/"]
COPY ["DKH.ApiManagementService.Contracts/DKH.ApiManagementService.Contracts.csproj", "DKH.ApiManagementService.Contracts/"]
COPY ["DKH.ApiManagementService.Domain/DKH.ApiManagementService.Domain.csproj", "DKH.ApiManagementService.Domain/"]
COPY ["DKH.ApiManagementService.Infrastructure/DKH.ApiManagementService.Infrastructure.csproj", "DKH.ApiManagementService.Infrastructure/"]
RUN --mount=type=cache,target=/root/.nuget/packages,sharing=locked \
    dotnet restore "DKH.ApiManagementService.Api/DKH.ApiManagementService.Api.csproj"
COPY . .

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR "/src/DKH.ApiManagementService.Api"
RUN --mount=type=cache,target=/root/.nuget/packages,sharing=locked \
    if [ "$(uname -m)" = "aarch64" ] || [ "$(uname -m)" = "arm64" ]; then \
    Protobuf_ProtocPath=/usr/bin/protoc \
    dotnet publish "./DKH.ApiManagementService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore; \
    else \
    dotnet publish "./DKH.ApiManagementService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore; \
    fi

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DKH.ApiManagementService.Api.dll"]
