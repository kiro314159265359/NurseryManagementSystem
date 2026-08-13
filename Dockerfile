# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files
COPY ["NurseryManagementSystem.API/NurseryManagementSystem.API.csproj", "NurseryManagementSystem.API/"]
COPY ["NurseryManagementSystem.Application/NurseryManagementSystem.Application.csproj", "NurseryManagementSystem.Application/"]
COPY ["NurseryManagementSystem.Domain/NurseryManagementSystem.Domain.csproj", "NurseryManagementSystem.Domain/"]
COPY ["NurseryManagementSystem.Infrastructure/NurseryManagementSystem.Infrastructure.csproj", "NurseryManagementSystem.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "NurseryManagementSystem.API/NurseryManagementSystem.API.csproj"

# Copy all source files
COPY . .

# Build the project
WORKDIR "/src/NurseryManagementSystem.API"
RUN dotnet build "NurseryManagementSystem.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "NurseryManagementSystem.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Configure the application to listen on port 8080 (Render standard)
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "NurseryManagementSystem.API.dll"]
