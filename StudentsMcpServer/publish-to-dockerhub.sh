#!/bin/bash
# Bash script to build and publish StudentsMcpServer to Docker Hub
# Usage: ./publish-to-dockerhub.sh your-username [tag] [image-name]

set -e

DOCKER_HUB_USERNAME=${1:-""}
IMAGE_TAG=${2:-"latest"}
IMAGE_NAME=${3:-"studentsmcp"}

if [ -z "$DOCKER_HUB_USERNAME" ]; then
    echo "Error: Docker Hub username is required"
    echo "Usage: ./publish-to-dockerhub.sh <docker-hub-username> [tag] [image-name]"
    exit 1
fi

IMAGE_FULL_NAME="${DOCKER_HUB_USERNAME}/${IMAGE_NAME}:${IMAGE_TAG}"

echo "Building Docker image: $IMAGE_FULL_NAME"
docker build -t "$IMAGE_FULL_NAME" .

if [ $? -ne 0 ]; then
    echo "Docker build failed!"
    exit 1
fi

echo "Docker image built successfully: $IMAGE_FULL_NAME"

read -p "Do you want to push the image to Docker Hub? (y/n) " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    echo "Pushing image to Docker Hub..."
    docker push "$IMAGE_FULL_NAME"
    
    if [ $? -ne 0 ]; then
        echo "Docker push failed!"
        exit 1
    fi
    
    echo "Image pushed successfully to Docker Hub!"
    echo "Image: $IMAGE_FULL_NAME"
else
    echo "Skipping push. Image is available locally: $IMAGE_FULL_NAME"
fi

