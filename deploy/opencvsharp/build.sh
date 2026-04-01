#!/bin/bash

set -e
IMAGE_NAME="michalmusil/dotnet10-noble-opencv"

echo "Building opencv image"
docker build --platform linux/amd64 -t "${IMAGE_NAME}:latest" .
echo "Successfully built ${IMAGE_NAME}"