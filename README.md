# Tire Recognition API

A .NET 10 Web API application designed for tire sidewall parameter extraction and recognition. This project leverages computer vision for preprocessing, Vision Language Models (VLMs) for text recognition, custom regex-based logic for postprocessing, and string similarity matching with valid database parameter entries for further validation.

## Features

- **Preprocessing**: Automatic tire rim segmentation (ONNX), unwarping, and slicing.
- **Recognition**: Integration with VLMs (e.g., Gemini) for high-accuracy text extraction.
- **Postprocessing**: Regex-based analysis for structured parameter extraction, sanitization and validation.
- **DbMatching**: Matching recognized parameters against databases of well known and supported values.
- **Resilience**: Built-in retry policies and circuit breakers for external service calls.
- **Observability**: Full OpenTelemetry (OTEL) integration for tracing and logging.

---

## Customizability and Configuration

The application is customizable via `appsettings.json` or Environment Variables. Configuration is mapped to POCO objects located in `/src/TireRecognition.Application/Options`, which also contain further documentation for individual values.

### 1. Authentication (`AuthOptions`)

- **`Enabled`**: Toggle API key authentication for the API.
- **`SchemeName`**: Name of the auth scheme.
- **`ApiKey`**: The valid value for the `X-API-KEY` header when API key authentication is enabled.

### 2. Preprocessing (`PreprocessingOptions`)

- **`MaxInputImageSize`**: Max side (width or height) of image before preprocessing begins.
- **`MaxOutputImageSize`**: Max side (width or height) of image before recognition begins.
- **`NumberOfSlices`**: How many slices the extracted tire strip should be divided into.
- **`TireOuterRadiusRatio`**: Ratio to determine the whole wheel radius from the rim radius.
- **`TireInnerRadiusRatio`**: Ratio to tune the detected rim radius.
- **`TireStripProlongWidthRatio`**: Ratio by which the tire strip is prolonged to prevent text splitting.
- **`SliceOverlapRatio`**: Ratio by which tire strip slices overlap.
- **`TireRimDetectionConfidenceThreshold`**: Confidence threshold for the rim segmentation model.

### 3. Recognition (`RecognitionOptions`)

- **`VlmEndpoint`**: Endpoint of the Vision Language Model.
- **`VlmTemperature`**: Temperature for the VLM model.
- **`VlmSeed`**: Seed for VLM output stability.
- **`VlmPrompt`**: Prompt used for text recognition.
- **`VlmApiKey`**: API key for the VLM model.
- **`InputTokenPricePerMillion`**: Price in USD per 1M input tokens. Used for cost estimation.
- **`OutputTokenPricePerMillion`**: Price in USD per 1M output tokens. Used for cost estimation.

### 4. Database Matching (`TireDbMatchingOptions`)

- **`TireCodeEndpointUri`**: Endpoint for retrieving valid tire codes.
- **`TireManufacturerEndpointUri`**: Endpoint for retrieving valid manufacturers.
- **`RemoteDbCacheExpirationMinutes`**: Cache TTL for remote database responses.
- **`DefaultTireDbMatchingResultLimit`**: Default limit for matching results.

### 5. Resilience (`ResilienceOptions`)

- **`MaxRetries`**: Number of retries for failed HTTP requests.
- **`RetryDelayMs`**: Wait time before attempting a retry.
- **`AttemptTimeoutSeconds`**: Timeout for a single request attempt.
- **`TotalTimeoutSeconds`**: Timeout for the entire call (including retries).
- **`CircuitBreakerSamplingDurationSeconds`**: Sampling duration for the circuit breaker.

### 6. OpenTelemetry (OTEL)

The application supports observability via OpenTelemetry. These settings are configured via environment variables:

- **`OTEL_ENABLED`**: Toggle OTEL integration.
- **`OTEL_SERVICE_NAME`**: Name of the service in traces/logs.
- **`OTEL_EXPORTER_OTLP_ENDPOINT`**: OTLP collector endpoint.
- **`OTEL_EXPORTER_OTLP_PROTOCOL`**: Protocol for exporting (e.g., `http/protobuf`).

---

## API Documentation

The API provides interactive documentation via **Swagger/OpenAPI**. When running the application locally or in a development environment, you can access the Swagger UI at:

- `http://localhost:<PORT>/swagger`

---

## Deployment and Running

### Docker

Deployment scripts are located in `/deploy/service`.

#### Building the Image

Use `build.sh` to create the Docker image. Modify the script image name variable to customize it for your needs.

```bash
./build.sh
```

#### Running the Container

The `/deploy/service/run.sh` script is the primary way to launch the service. It maps local variables to container environment variables using the `Section__Property` syntax.

##### Essential Variables

Secret values and sensitive configurations must be specified in a `.env` file located in the `/deploy/service` directory. This file should follow the `Section__Property` syntax to correctly map to the application's configuration.

Refer to `env-example.txt` for a template of the required variables.

| Variable                                             | appsettings Mapping                                 | Description             |
| :--------------------------------------------------- | :-------------------------------------------------- | :---------------------- |
| `RecognitionOptions__VlmApiKey`                      | `RecognitionOptions.VlmApiKey`                      | Gemini/VLM API Key      |
| `TireDbMatchingOptions__TireCodeEndpointUri`         | `TireDbMatchingOptions.TireCodeEndpointUri`         | URI for tire code DB    |
| `TireDbMatchingOptions__TireManufacturerEndpointUri` | `TireDbMatchingOptions.TireManufacturerEndpointUri` | URI for manufacturer DB |

##### Execution

```bash
# Ensure .env is populated
./run.sh
```

The script defaults to port `8079` on the host, mapping to port `8080` in the container.

### Local Development

For local development, you can run the application directly using .NET:

```bash
dotnet run --project src/TireRecognition.WebApi/TireRecognition.WebApi.csproj
```

##### Essential Variables

The default configuration variables are the same as for the Docker deployment. Defaults are located in `appsettings.json` with invalid placeholder values for secrets. Required secrets should be provided via environment variables or dotnet user secrets.

| Variable                                             | appsettings Mapping                                 | Description             |
| :--------------------------------------------------- | :-------------------------------------------------- | :---------------------- |
| `RecognitionOptions__VlmApiKey`                      | `RecognitionOptions.VlmApiKey`                      | Gemini/VLM API Key      |
| `TireDbMatchingOptions__TireCodeEndpointUri`         | `TireDbMatchingOptions.TireCodeEndpointUri`         | URI for tire code DB    |
| `TireDbMatchingOptions__TireManufacturerEndpointUri` | `TireDbMatchingOptions.TireManufacturerEndpointUri` | URI for manufacturer DB |
