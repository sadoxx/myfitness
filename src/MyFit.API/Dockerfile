# Use the official .NET 8 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["MyFit.sln", "./"]
COPY ["src/MyFit.Domain/MyFit.Domain.csproj", "src/MyFit.Domain/"]
COPY ["src/MyFit.Application/MyFit.Application.csproj", "src/MyFit.Application/"]
COPY ["src/MyFit.Infrastructure/MyFit.Infrastructure.csproj", "src/MyFit.Infrastructure/"]
COPY ["src/MyFit.API/MyFit.API.csproj", "src/MyFit.API/"]

# Restore dependencies
RUN dotnet restore "src/MyFit.API/MyFit.API.csproj"

# Copy all source code
COPY . .

# Build the API project
WORKDIR "/src/src/MyFit.API"
RUN dotnet build "MyFit.API.csproj" -c Release -o /app/build

# Publish the API
FROM build AS publish
RUN dotnet publish "MyFit.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=publish /app/publish .

# Copy the seed data
COPY ["src/MyFit.Infrastructure/Seeds/", "./Seeds/"]

# Set entrypoint
ENTRYPOINT ["dotnet", "MyFit.API.dll"]
