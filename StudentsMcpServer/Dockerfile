# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

# Copy csproj and restore dependencies
COPY ["StudentsMcpServer.csproj", "./"]
RUN dotnet restore "StudentsMcpServer.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "StudentsMcpServer.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "StudentsMcpServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS final
WORKDIR /app

# Copy published app
COPY --from=publish /app/publish .

# Create logs directory
RUN mkdir -p /app/logs

# Define volume for logs (optional - allows mounting from host)
VOLUME ["/app/logs"]

# Set environment variables
ENV DOTNET_ENVIRONMENT=Production

# Logs will be saved to: /app/logs/studentsmcp-YYYYMMDD.log
# To persist logs on host, mount a volume:
# docker run -i --rm -v $(pwd)/logs:/app/logs your-username/studentsmcp:latest

ENTRYPOINT ["dotnet", "StudentsMcpServer.dll"]

