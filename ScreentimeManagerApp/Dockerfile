# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Temporarily become root to install the required utilities (ping & net)
USER root
RUN apt-get update \
 && apt-get install -y --no-install-recommends iputils-ping samba-common-bin \
 && rm -rf /var/lib/apt/lists/*

# Revert to the non-root app user that the base image expects
USER $APP_UID
WORKDIR /app

# Define Environment Variables with some default values
ENV CULTURE de-De
ENV SCREENTIME_LIMIT_MINUTES 1
ENV SCREENTIME_COUNT_INTERVAL_MS 2000
ENV PING_INTERVAL_MS 10000
ENV PING_TIMEOUT_MS 200
ENV WEBHOOK_URL https://localhost
ENV PING_HOST localhost
ENV HOST_USER Admin
ENV PASSWD ChangeMe

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ScreentimeManagerApp/ScreentimeManagerApp.csproj", "ScreentimeManagerApp/"]
RUN dotnet restore "./ScreentimeManagerApp/ScreentimeManagerApp.csproj"
COPY . .
WORKDIR "/src/ScreentimeManagerApp"
RUN dotnet build "./ScreentimeManagerApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ScreentimeManagerApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ScreentimeManagerApp.dll"]