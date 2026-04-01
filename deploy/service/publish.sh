IMAGE_NAME="michalmusil/tire-recognition"
TAG="latest"

echo "Pushing ${IMAGE_NAME}:${TAG}"
docker push "${IMAGE_NAME}:${TAG}"
echo "Successfully finished pushing ${IMAGE_NAME}:${TAG}"