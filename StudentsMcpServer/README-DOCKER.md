# StudentsMcpServer - Docker Build and Publish

## Building the Docker Image

### Using Docker directly:

```bash
# Build the image
docker build -t your-username/studentsmcp:latest .

# Tag for specific version
docker tag your-username/studentsmcp:latest your-username/studentsmcp:v1.0.0
```

### Using the provided scripts:

**PowerShell (Windows):**
```powershell
.\publish-to-dockerhub.ps1 -DockerHubUsername "your-username" -ImageTag "latest"
```

**Bash (Linux/Mac):**
```bash
chmod +x publish-to-dockerhub.sh
./publish-to-dockerhub.sh your-username latest
```

## Publishing to Docker Hub

1. **Login to Docker Hub:**
   ```bash
   docker login
   ```

2. **Build and tag the image:**
   ```bash
   docker build -t your-username/studentsmcp:latest .
   ```

3. **Push to Docker Hub:**
   ```bash
   docker push your-username/studentsmcp:latest
   ```

## Running the Container

```bash
# Run the container
docker run -i --rm your-username/studentsmcp:latest

# Run with volume mount for logs
docker run -i --rm -v $(pwd)/logs:/app/logs your-username/studentsmcp:latest
```

## Logs

### Lokacija logova

Logovi se čuvaju u `/app/logs/` direktorijumu unutar kontejnera.

**Format imena fajla:** `studentsmcp-YYYYMMDD.log`

**Primer:** `studentsmcp-20240115.log`

### Persistovanje logova na host mašini

Da biste sačuvali logove na host mašini, mount-ujte volume:

**Windows PowerShell:**
```powershell
docker run -i --rm -v ${PWD}/logs:/app/logs your-username/studentsmcp:latest
```

**Linux/Mac:**
```bash
docker run -i --rm -v $(pwd)/logs:/app/logs your-username/studentsmcp:latest
```

**Primer:**
```powershell
# Kreirajte logs direktorijum lokalno (opciono)
mkdir logs

# Pokrenite kontejner sa mount-ovanim logs direktorijumom
docker run -i --rm -v ${PWD}/logs:/app/logs lukagacic/studentsmcp:latest
```

### Pregled logova

**Unutar kontejnera:**
```bash
docker exec -it <container-id> cat /app/logs/studentsmcp-20240115.log
```

**Na host mašini (ako je volume mount-ovan):**
```powershell
# Windows
cat logs\studentsmcp-20240115.log

# Linux/Mac
cat logs/studentsmcp-20240115.log
```

## Example Usage

The MCP server is used by the GitHubModelsMcpClient. In `appsettings.json`:

```json
{
  "McpServerDockerCommand": "docker run -i --rm your-username/studentsmcp:latest"
}
```

