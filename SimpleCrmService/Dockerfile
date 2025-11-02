# syntax=docker/dockerfile:1

# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
# Ensure the app listens on port 8080 in the container
ENV ASPNETCORE_URLS=http://+:8080

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore as distinct layers
COPY ["SimpleCrmService/SimpleCrmService.csproj", "SimpleCrmService/"]
COPY ["DbService/DbService.csproj", "DbService/"]
COPY ["Contracts/Contracts.csproj", "Contracts/"]
RUN dotnet restore "SimpleCrmService/SimpleCrmService.csproj"

# Copy the rest of the source and build
COPY . .
WORKDIR "/src/SimpleCrmService"
RUN dotnet build "SimpleCrmService.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "SimpleCrmService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SimpleCrmService.dll"]
