# PowerShell script to build and publish StudentsMcpServer to Docker Hub
# Usage: .\publish-to-dockerhub.ps1 -DockerHubUsername "your-username" -ImageTag "latest"

param(
    [Parameter(Mandatory=$true)]
    [string]$DockerHubUsername,
    
    [Parameter(Mandatory=$false)]
    [string]$ImageTag = "latest",
    
    [Parameter(Mandatory=$false)]
    [string]$ImageName = "studentsmcp"
)

$ErrorActionPreference = "Stop"

Write-Host "Building Docker image..." -ForegroundColor Green
$imageFullName = "${DockerHubUsername}/${ImageName}:${ImageTag}"

# Build the Docker image
docker build -t $imageFullName .

if ($LASTEXITCODE -ne 0) {
    Write-Host "Docker build failed!" -ForegroundColor Red
    exit 1
}

Write-Host "Docker image built successfully: $imageFullName" -ForegroundColor Green

# Ask for confirmation before pushing
$confirmation = Read-Host "Do you want to push the image to Docker Hub? (y/n)"
if ($confirmation -eq "y" -or $confirmation -eq "Y") {
    Write-Host "Pushing image to Docker Hub..." -ForegroundColor Green
    docker push $imageFullName
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Docker push failed!" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "Image pushed successfully to Docker Hub!" -ForegroundColor Green
    Write-Host "Image: $imageFullName" -ForegroundColor Cyan
} else {
    Write-Host "Skipping push. Image is available locally: $imageFullName" -ForegroundColor Yellow
}

