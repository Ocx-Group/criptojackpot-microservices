FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Crear usuario non-root
RUN addgroup --gid 1000 appgroup && \
    adduser --uid 1000 --gid 1000 --disabled-password --gecos "" appuser

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Domain/CryptoJackpot.Domain.Core.csproj", "Domain/"]
COPY ["Infra.Bus/CryptoJackpot.Infra.Bus.csproj", "Infra.Bus/"]
COPY ["Infra.IoC/CryptoJackpot.Infra.IoC.csproj", "Infra.IoC/"]
COPY ["Microservices/Notification/Domain/CryptoJackpot.Notification.Domain.csproj", "Microservices/Notification/Domain/"]
COPY ["Microservices/Notification/Data/CryptoJackpot.Notification.Data.csproj", "Microservices/Notification/Data/"]
COPY ["Microservices/Notification/Application/CryptoJackpot.Notification.Application.csproj", "Microservices/Notification/Application/"]
COPY ["Microservices/Notification/Api/CryptoJackpot.Notification.Api.csproj", "Microservices/Notification/Api/"]

RUN dotnet restore "Microservices/Notification/Api/CryptoJackpot.Notification.Api.csproj"
COPY . .

WORKDIR "/src/Microservices/Notification/Api"
RUN dotnet build "CryptoJackpot.Notification.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CryptoJackpot.Notification.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Cambiar ownership y usuario
RUN chown -R appuser:appgroup /app
USER appuser

ENTRYPOINT ["dotnet", "CryptoJackpot.Notification.Api.dll"]

