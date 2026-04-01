#!/bin/bash

# Configuration variables (excl-secrets)
# These reflect appsettings.json values
OTEL_ENABLED=false
OTEL_SERVICE_NAME="TireReconition"
OTEL_EXPORTER_OTLP_PROTOCOL="http/protobuf"
OTEL_RESOURCE_ATTRIBUTES="service.namespace=tire-recognition"
OTEL_DOTNET_EXPERIMENTAL_OTLP_EMIT_EXCEPTION_LOG_ATTRIBUTES="true"
OTEL_DOTNET_EXPERIMENTAL_OTLP_EMIT_EVENT_LOG_ATTRIBUTES="true"
OTEL_DOTNET_EXPERIMENTAL_OTLP_RETRY="in_memory"

AUTH_ENABLED=false
AUTH_SCHEME="ApiKey"

RESILIENCE_MAX_RETRIES=2
RESILIENCE_RETRY_DELAY=1000
RESILIENCE_ATTEMPT_TIMEOUT=10
RESILIENCE_TOTAL_TIMEOUT=40
RESILIENCE_CB_SAMPLING=60

PREPROCESSING_MAX_INPUT=4000
PREPROCESSING_MAX_OUTPUT=2048
PREPROCESSING_SLICES=2
PREPROCESSING_OUTER_RATIO=1.3
PREPROCESSING_INNER_RATIO=0.9
PREPROCESSING_PROLONG_WIDTH=0.17
PREPROCESSING_OVERLAP=0.12
PREPROCESSING_CONF_THRESHOLD=0.6
PREPROCESSING_MODEL_PATH="Models/tire_segmentation_v5.onnx"

RECOGNITION_VLM_ENDPOINT="https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent"
RECOGNITION_VLM_TEMP=0
RECOGNITION_VLM_SEED=42
RECOGNITION_PRICE_IN=0.3
RECOGNITION_PRICE_OUT=2.5

DBMATCHING_CACHE_EXP=60
DBMATCHING_MATCH_LIMIT=30

IMAGE_NAME="michalmusil/tire-recognition:latest"
PORT=8079

echo "Starting container: $IMAGE_NAME..."

docker run -d \
  --name tire-recognition-service \
  -p "${PORT}":8080 \
  --env-file .env \
  -e OTEL_ENABLED="$OTEL_ENABLED" \
  -e OTEL_SERVICE_NAME="$OTEL_SERVICE_NAME" \
  -e OTEL_EXPORTER_OTLP_PROTOCOL="$OTEL_EXPORTER_OTLP_PROTOCOL" \
  -e OTEL_RESOURCE_ATTRIBUTES="$OTEL_RESOURCE_ATTRIBUTES" \
  -e OTEL_DOTNET_EXPERIMENTAL_OTLP_EMIT_EXCEPTION_LOG_ATTRIBUTES="$OTEL_DOTNET_EXPERIMENTAL_OTLP_EMIT_EXCEPTION_LOG_ATTRIBUTES" \
  -e OTEL_DOTNET_EXPERIMENTAL_OTLP_EMIT_EVENT_LOG_ATTRIBUTES="$OTEL_DOTNET_EXPERIMENTAL_OTLP_EMIT_EVENT_LOG_ATTRIBUTES" \
  -e OTEL_DOTNET_EXPERIMENTAL_OTLP_RETRY="$OTEL_DOTNET_EXPERIMENTAL_OTLP_RETRY" \
  -e AuthOptions__Enabled="$AUTH_ENABLED" \
  -e AuthOptions__SchemeName="$AUTH_SCHEME" \
  -e ResilienceOptions__MaxRetries="$RESILIENCE_MAX_RETRIES" \
  -e ResilienceOptions__RetryDelayMs="$RESILIENCE_RETRY_DELAY" \
  -e ResilienceOptions__AttemptTimeoutSeconds="$RESILIENCE_ATTEMPT_TIMEOUT" \
  -e ResilienceOptions__TotalTimeoutSeconds="$RESILIENCE_TOTAL_TIMEOUT" \
  -e ResilienceOptions__CircuitBreakerSamplingDurationSeconds="$RESILIENCE_CB_SAMPLING" \
  -e PreprocessingOptions__MaxInputImageSize="$PREPROCESSING_MAX_INPUT" \
  -e PreprocessingOptions__MaxOutputImageSize="$PREPROCESSING_MAX_OUTPUT" \
  -e PreprocessingOptions__NumberOfSlices="$PREPROCESSING_SLICES" \
  -e PreprocessingOptions__TireOuterRadiusRatio="$PREPROCESSING_OUTER_RATIO" \
  -e PreprocessingOptions__TireInnerRadiusRatio="$PREPROCESSING_INNER_RATIO" \
  -e PreprocessingOptions__TireStripProlongWidthRatio="$PREPROCESSING_PROLONG_WIDTH" \
  -e PreprocessingOptions__SliceOverlapRatio="$PREPROCESSING_OVERLAP" \
  -e PreprocessingOptions__TireRimDetectionConfidenceThreshold="$PREPROCESSING_CONF_THRESHOLD" \
  -e PreprocessingOptions__TireRimDetectionModelRelativePath="$PREPROCESSING_MODEL_PATH" \
  -e RecognitionOptions__VlmEndpoint="$RECOGNITION_VLM_ENDPOINT" \
  -e RecognitionOptions__VlmTemperature="$RECOGNITION_VLM_TEMP" \
  -e RecognitionOptions__VlmSeed="$RECOGNITION_VLM_SEED" \
  -e RecognitionOptions__InputTokenPricePerMillion="$RECOGNITION_PRICE_IN" \
  -e RecognitionOptions__OutputTokenPricePerMillion="$RECOGNITION_PRICE_OUT" \
  -e TireDbMatchingOptions__RemoteDbCacheExpirationMinutes="$DBMATCHING_CACHE_EXP" \
  -e TireDbMatchingOptions__DefaultTireDbMatchingResultLimit="$DBMATCHING_MATCH_LIMIT" \
  "$IMAGE_NAME"