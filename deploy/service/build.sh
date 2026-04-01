IMAGE_NAME="michalmusil/tire-recognition"
TAG="latest"

echo "Starting docker build of ${IMAGE_NAME}:${TAG}"
docker build --platform linux/amd64 -t "${IMAGE_NAME}:${TAG}" -f src/TireRecognition.WebApi/Dockerfile .
echo "Successfully finished docker build of ${IMAGE_NAME}:${TAG}"