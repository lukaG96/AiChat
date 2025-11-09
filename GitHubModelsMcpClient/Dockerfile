# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

# Copy csproj and restore dependencies
COPY ["GitHubModelsMcpClient.csproj", "./"]
RUN dotnet restore "GitHubModelsMcpClient.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "GitHubModelsMcpClient.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "GitHubModelsMcpClient.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Install Docker CLI for MCP client to run Docker commands
# Note: Docker socket is mounted from host in docker-compose.yml
RUN apt-get update && \
    apt-get install -y docker.io curl && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Copy published app
COPY --from=publish /app/publish .

# Expose port
EXPOSE 5000

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "GitHubModelsMcpClient.dll"]

