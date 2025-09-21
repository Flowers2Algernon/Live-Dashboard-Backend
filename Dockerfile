# Use the official .NET runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["LERD_Backend/LERD_Backend.csproj", "LERD_Backend/"]
COPY ["LERD.Application/LERD.Application.csproj", "LERD.Application/"]
COPY ["LERD.Domain/LERD.Domain.csproj", "LERD.Domain/"]
COPY ["LERD.Infrastructure/LERD.Infrastructure.csproj", "LERD.Infrastructure/"]
COPY ["LERD.Shared/LERD.Shared.csproj", "LERD.Shared/"]

RUN dotnet restore "LERD_Backend/LERD_Backend.csproj"

# Copy the remaining source code
COPY . .

# Build the application
WORKDIR "/src/LERD_Backend"
RUN dotnet build "LERD_Backend.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "LERD_Backend.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "LERD_Backend.dll"]
