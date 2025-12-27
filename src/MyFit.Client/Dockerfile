# Use the official .NET 8 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file
COPY ["src/MyFit.Client/MyFit.Client.csproj", "src/MyFit.Client/"]

# Restore dependencies
RUN dotnet restore "src/MyFit.Client/MyFit.Client.csproj"

# Copy all source code
COPY ["src/MyFit.Client/", "src/MyFit.Client/"]

# Build and publish
WORKDIR "/src/src/MyFit.Client"
RUN dotnet publish "MyFit.Client.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use nginx to serve the Blazor WASM app
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

# Copy published Blazor WASM files
COPY --from=build /app/publish/wwwroot .

# Copy custom nginx configuration
COPY src/MyFit.Client/nginx.conf /etc/nginx/nginx.conf

EXPOSE 80
