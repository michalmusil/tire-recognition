# Tire Recognition API

This archive contains the implementation of the **production service prototype** service described in the thesis: "Evaluation and Application of Contemporary Text Recognition Solutions for Tire Parameter Extraction".

Tire Recognition is a .NET 10 Web API service designed for automated tire sidewall parameter extraction via text recognition from images. The system integrates computer vision for preprocessing, vision language models (VLMs) for text recognition (TR), regex-based logic for postprocessing, and string similarity matching against database entries for end-user validation.

The architecture of the service is based on the [Clean/Onion Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html). Consequently, the solution is divided into several projects:

- **Domain**: Domain entities and fundamental business rules
- **Application**: Core business logic and service interfaces
- **Infrastructure**: Service implementations via external service and library integrations
- **Presentation (WebApi)**: REST controllers, middleware, and API configuration
- **Shared**: Foundational components shared across the layers

## Core Features

- **Preprocessing**: Performs automatic tire rim segmentation using ONNX models, followed by unwarping and slicing of the tire sidewall.
- **Recognition**: Integrates with vision language models (Gemini 2.5 Flash by default) to achieve high-accuracy text recognition from tire images.
- **Postprocessing**: Utilizes regex-based analysis to perform structured parameter extraction, sanitization, and validation.
- **DbMatching**: Validates recognized parameters by matching them against databases of known and supported tire values.
- **Resilience**: Implements built-in retry policies and circuit breakers for robust communication with external services.
- **Observability**: Includes full OpenTelemetry (OTEL) integration for comprehensive tracing and logging.

## API Documentation

The service provides interactive documentation via **Swagger/OpenAPI**. When running in a local or development environment, the UI is accessible at:

`http://localhost:<PORT>/swagger`

---

## Deployment & Execution

### 1. Docker Deployment

The service is containerized and pre-built image targetting the x86_64 architecture is available on Docker Hub registry as `michalmusil/tire-recognition`. Deployment assets are located in the `/deploy/service` directory.

#### Prerequisites

- Docker
- Sensitive configurations must be defined in a `.env` file within the `/deploy/service` directory. Refer to `env-example.txt` for a template. Following secrets must be specified:

| Variable                                             | AppSettings / User Secrets Mapping                  | Description              |
| :--------------------------------------------------- | :-------------------------------------------------- | :----------------------- |
| `RecognitionOptions__VlmApiKey`                      | `RecognitionOptions:VlmApiKey`                      | Gemini/VLM API Key.      |
| `RecognitionOptions__VlmPrompt`                      | `RecognitionOptions:VlmPrompt`                      | VLM recognition prompt.  |
| `TireDbMatchingOptions__TireCodeEndpointUri`         | `TireDbMatchingOptions:TireCodeEndpointUri`         | URI for tire code DB.    |
| `TireDbMatchingOptions__TireManufacturerEndpointUri` | `TireDbMatchingOptions:TireManufacturerEndpointUri` | URI for manufacturer DB. |

#### Building and Publishing the Image

Execute the `build.sh` script to build the Docker image of the service (defined in `src/TireRecognition.WebApi/Dockerfile`). The image name and tag can be customized within the script variables.

```bash
deploy/service/build.sh
```

To publish the image to a registry, execute the `publish.sh` script. The image name and tag can be customized within the script variables.

```bash
deploy/service/publish.sh
```

#### Running the Container

The `/deploy/service/run.sh` utility script facilitates launching the container with simple overrides of the configuration defaults. It maps local script variables to the container environment variables using the .NET `Section__Property` naming convention.

To run the container, execute:

```bash

deploy/service/run.sh
```

The service defaults to host port `8079` (mapping to container port `8080`).

### 2. Local Development

Some of the preprocessing service's dependendies include native packages, which are installed differently depending on the platform. While efforts were made to ensure cross-platform compatibility, some dependencies may cause issues requiring dependency updates on untested environments, such as Windows.

#### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- The same required secrets (described in the Docker Deployment prerequisites) must also be configured for local development. Values can be specified in `src/TireRecognition.WebApi/appsettings.json`, or more preferrably by providing them via [.NET user secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-10.0&tabs=linux).

To run the application locally using the .NET CLI:

```bash
dotnet run --project src/TireRecognition.WebApi/TireRecognition.WebApi.csproj
```

---

## Configuration and Customizability

The service is highly configurable via `appsettings.json` or environment variables. Configuration is mapped to POCO classes located in `/src/TireRecognition.Application/Options`.

### 1. Authentication (`AuthOptions`)

- **`Enabled`**: Toggles API key authentication. (default: `false`).
- **`ApiKey`**: Specifies the required value for the `X-API-KEY` header when authentication is enabled.

### 2. Preprocessing (`PreprocessingOptions`)

- **`MaxInputImageSize`**: Maximum dimension (width or height) of an image before preprocessing.
- **`MaxOutputImageSize`**: Maximum dimension of an image before it is sent for recognition.
- **`NumberOfSlices`**: The number of segments the extracted tire strip is divided into.
- **`TireInnerRadiusRatio`**: Ratio to tune the detected rim radius.
- **`TireOuterRadiusRatio`**: Ratio to determine the whole wheel radius from the rim radius.
- **`TireStripProlongWidthRatio`**: Ratio used to prolong the tire strip to prevent text area splitting.
- **`SliceOverlapRatio`**: The ratio by which adjacent tire strip slices overlap to prevent text area splitting.
- **`TireRimDetectionConfidenceThreshold`**: The confidence threshold for the rim segmentation model.

### 3. Recognition (`RecognitionOptions`)

- **`VlmEndpoint`**: The API endpoint for the utilized vision language model.
- **`VlmTemperature` / `VlmSeed`**: Parameters for controlling the randomness and stability of the VLM output.
- **`VlmPrompt`**: The specific prompt used to guide the VLM during text recognition.
- **`VlmApiKey`**: The API key required for VLM access.
- **`InputTokenPricePerMillion` / `OutputTokenPricePerMillion`**: Cost estimation parameters (USD per 1M tokens).

### 4. Database Matching (`TireDbMatchingOptions`)

- **`TireCodeEndpointUri`**: Endpoint for retrieving valid tire codes.
- **`TireManufacturerEndpointUri`**: Endpoint for retrieving valid manufacturer names.
- **`RemoteDbCacheExpirationMinutes`**: TTL for the cache of remote database responses.
- **`DefaultTireDbMatchingResultLimit`**: The default limit for returned matching results.

### 5. Resilience (`ResilienceOptions`)

- **`MaxRetries` / `RetryDelayMs`**: Configuration for failed HTTP request attempts.
- **`AttemptTimeoutSeconds` / `TotalTimeoutSeconds`**: Timeouts for individual requests and the entire execution chain.
- **`CircuitBreakerSamplingDurationSeconds`**: The duration used for circuit breaker state sampling.

### 6. OpenTelemetry (OTEL)

Observability is configured via standard environment variables:

- **`OTEL_ENABLED`**: Toggles OpenTelemetry integration.
- **`OTEL_SERVICE_NAME`**: The identifier for the service in traces and logs.
- **`OTEL_EXPORTER_OTLP_ENDPOINT`**: The OTLP collector endpoint.
- **`OTEL_EXPORTER_OTLP_PROTOCOL`**: The export protocol (e.g., `http/protobuf`).
