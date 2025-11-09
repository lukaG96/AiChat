# 📦 Instrukcije za Publish na Docker Hub

## Korak 1: Priprema

### 1.1. Kreiraj Docker Hub nalog
- Idite na [https://hub.docker.com](https://hub.docker.com)
- Registrujte se ili se prijavite

### 1.2. Login u Docker Hub
```powershell
docker login
```
Unesite vaše Docker Hub korisničko ime i lozinku.

## Korak 2: Build Docker Image

### Opcija A: Koristite PowerShell skriptu (Preporučeno)

```powershell
cd StudentsMcpServer
.\publish-to-dockerhub.ps1 -DockerHubUsername "vaše-korisničko-ime" -ImageTag "latest"
```

**Primer:**
```powershell
.\publish-to-dockerhub.ps1 -DockerHubUsername "lukagacic" -ImageTag "latest"
```

### Opcija B: Ručno build i push

```powershell
# 1. Build image
docker build -t vaše-korisničko-ime/studentsmcp:latest .

# 2. Tag za verziju (opciono)
docker tag vaše-korisničko-ime/studentsmcp:latest vaše-korisničko-ime/studentsmcp:v1.0.0

# 3. Push na Docker Hub
docker push vaše-korisničko-ime/studentsmcp:latest
```

**Primer:**
```powershell
docker build -t lukagacic/studentsmcp:latest .
docker push lukagacic/studentsmcp:latest
```

## Korak 3: Verifikacija

### Proverite da li je image na Docker Hub:
1. Idite na [https://hub.docker.com](https://hub.docker.com)
2. Kliknite na vaš profil
3. Pronađite `studentsmcp` repository

### Testirajte image lokalno:
```powershell
docker run -i --rm vaše-korisničko-ime/studentsmcp:latest
```

## Korak 4: Korišćenje u GitHubModelsMcpClient

Ažurirajte `appsettings.json` u `GitHubModelsMcpClient`:

**Bez persistovanja logova:**
```json
{
  "McpServerDockerCommand": "docker run -i --rm vaše-korisničko-ime/studentsmcp:latest"
}
```

**Sa persistovanjem logova (preporučeno):**
```json
{
  "McpServerDockerCommand": "docker run -i --rm -v ${PWD}/logs:/app/logs vaše-korisničko-ime/studentsmcp:latest"
}
```

**Primer:**
```json
{
  "McpServerDockerCommand": "docker run -i --rm -v ${PWD}/logs:/app/logs lukagacic/studentsmcp:latest"
}
```

### Gde se čuvaju logovi?

- **U kontejneru:** `/app/logs/studentsmcp-YYYYMMDD.log`
- **Na host mašini (sa volume mount):** `./logs/studentsmcp-YYYYMMDD.log`

**Format:** `studentsmcp-20240115.log` (datum u formatu YYYYMMDD)

## Napredne opcije

### Publish više verzija:
```powershell
# Build i tag za različite verzije
docker build -t vaše-korisničko-ime/studentsmcp:latest .
docker tag vaše-korisničko-ime/studentsmcp:latest vaše-korisničko-ime/studentsmcp:v1.0.0
docker tag vaše-korisničko-ime/studentsmcp:latest vaše-korisničko-ime/studentsmcp:1.0.0

# Push sve verzije
docker push vaše-korisničko-ime/studentsmcp:latest
docker push vaše-korisničko-ime/studentsmcp:v1.0.0
docker push vaše-korisničko-ime/studentsmcp:1.0.0
```

### Build za različite platforme (multi-arch):
```powershell
# Instalirajte buildx (ako već nije instaliran)
docker buildx create --use

# Build i push za više platformi
docker buildx build --platform linux/amd64,linux/arm64 -t vaše-korisničko-ime/studentsmcp:latest --push .
```

## Troubleshooting

### Problem: "unauthorized: authentication required"
**Rešenje:** Pokrenite `docker login` ponovo

### Problem: "denied: requested access to the resource is denied"
**Rešenje:** Proverite da li je ime image-a u formatu `korisničko-ime/repository:tag`

### Problem: Build ne uspeva
**Rešenje:** 
1. Proverite da li ste u `StudentsMcpServer` direktorijumu
2. Proverite da li postoji `Dockerfile`
3. Proverite da li su svi fajlovi prisutni

## Checklist pre publish-a:

- [ ] Docker Hub nalog kreiran
- [ ] `docker login` uspešan
- [ ] Image build uspešan
- [ ] Image testiran lokalno
- [ ] Image push uspešan
- [ ] Image vidljiv na Docker Hub
- [ ] `appsettings.json` ažuriran sa novim image-om

## Korisni linkovi:

- [Docker Hub](https://hub.docker.com)
- [Docker Documentation](https://docs.docker.com/)
- [Docker Build Documentation](https://docs.docker.com/engine/reference/commandline/build/)

