FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Crear usuario non-root
RUN addgroup --gid 1000 appgroup && \
    adduser --uid 1000 --gid 1000 --disabled-password --gecos "" appuser

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto
COPY ["Domain/CryptoJackpot.Domain.Core.csproj", "Domain/"]
COPY ["Infra.Bus/CryptoJackpot.Infra.Bus.csproj", "Infra.Bus/"]
COPY ["Infra.IoC/CryptoJackpot.Infra.IoC.csproj", "Infra.IoC/"]
COPY ["Microservices/Content/Domain/CryptoJackpot.Content.Domain.csproj", "Microservices/Content/Domain/"]
COPY ["Microservices/Content/Data/CryptoJackpot.Content.Data.csproj", "Microservices/Content/Data/"]
COPY ["Microservices/Content/Application/CryptoJackpot.Content.Application.csproj", "Microservices/Content/Application/"]
COPY ["Microservices/Content/CryptoJackpot.Content.Infra.IoC/CryptoJackpot.Content.Infra.IoC.csproj", "Microservices/Content/CryptoJackpot.Content.Infra.IoC/"]
COPY ["Microservices/Content/Api/CryptoJackpot.Content.Api.csproj", "Microservices/Content/Api/"]

# Restaurar dependencias con cache mount
RUN --mount=type=cache,id=nuget-content,target=/root/.nuget/packages \
    dotnet restore "Microservices/Content/Api/CryptoJackpot.Content.Api.csproj"

# Copiar código fuente (solo lo necesario)
COPY Domain/ Domain/
COPY Infra.Bus/ Infra.Bus/
COPY Infra.IoC/ Infra.IoC/
COPY Microservices/Content/ Microservices/Content/

# Build con cache mount
WORKDIR "/src/Microservices/Content/Api"
RUN --mount=type=cache,id=nuget-content,target=/root/.nuget/packages \
    dotnet build "CryptoJackpot.Content.Api.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN --mount=type=cache,id=nuget-content,target=/root/.nuget/packages \
    dotnet publish "CryptoJackpot.Content.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Cambiar ownership y usuario
RUN chown -R appuser:appgroup /app
USER appuser

ENTRYPOINT ["dotnet", "CryptoJackpot.Content.Api.dll"]
