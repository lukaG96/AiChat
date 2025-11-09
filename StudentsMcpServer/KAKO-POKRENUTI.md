# 🚀 Kako pokrenuti publish skriptu

## Korak 1: Login u Docker Hub

Pre pokretanja skripte, morate biti prijavljeni u Docker Hub:

```powershell
docker login
```

Unesite vaše Docker Hub korisničko ime i lozinku.

## Korak 2: Pokrenite PowerShell skriptu

### Opcija A: Iz StudentsMcpServer direktorijuma

```powershell
# Navigirajte do StudentsMcpServer direktorijuma
cd StudentsMcpServer

# Pokrenite skriptu sa vašim Docker Hub korisničkim imenom
.\publish-to-dockerhub.ps1 -DockerHubUsername "vaše-korisničko-ime"
```

**Primer:**
```powershell
cd StudentsMcpServer
.\publish-to-dockerhub.ps1 -DockerHubUsername "lukagacic"
```

### Opcija B: Sa custom tag-om

```powershell
.\publish-to-dockerhub.ps1 -DockerHubUsername "vaše-korisničko-ime" -ImageTag "v1.0.0"
```

**Primer:**
```powershell
.\publish-to-dockerhub.ps1 -DockerHubUsername "lukagacic" -ImageTag "v1.0.0"
```

### Opcija C: Sa custom imenom image-a

```powershell
.\publish-to-dockerhub.ps1 -DockerHubUsername "vaše-korisničko-ime" -ImageName "moj-studentsmcp" -ImageTag "latest"
```

## Korak 3: Potvrdite push

Skripta će vas pitati da li želite da push-ujete image na Docker Hub:
```
Do you want to push the image to Docker Hub? (y/n)
```

Unesite `y` da push-ujete ili `n` da preskočite (image će ostati samo lokalno).

## Primer kompletnog procesa:

```powershell
# 1. Login u Docker Hub
docker login

# 2. Navigirajte do direktorijuma
cd StudentsMcpServer

# 3. Pokrenite skriptu
.\publish-to-dockerhub.ps1 -DockerHubUsername "lukagacic"

# 4. Sačekajte da se build završi
# 5. Unesite 'y' kada se pojavi pitanje za push
```

## Troubleshooting

### Problem: "cannot be loaded because running scripts is disabled"

**Rešenje:** Pokrenite PowerShell kao Administrator i izvršite:
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### Problem: "docker: command not found"

**Rešenje:** Proverite da li je Docker Desktop instaliran i pokrenut.

### Problem: "unauthorized: authentication required"

**Rešenje:** Pokrenite `docker login` ponovo.

## Alternativa: Ručno bez skripte

Ako ne želite da koristite skriptu, možete ručno:

```powershell
# Build
docker build -t vaše-korisničko-ime/studentsmcp:latest .

# Push
docker push vaše-korisničko-ime/studentsmcp:latest
```

