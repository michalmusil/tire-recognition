using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TireRecognition.Application.Options;
using TireRecognition.Application.Services;
using TireRecognition.Domain.Preprocessing;
using TireRecognition.Domain.Recognition;
using TireRecognition.Infrastructure.Dtos.GeminiResponse;
using TireRecognition.Infrastructure.Exceptions;

namespace TireRecognition.Infrastructure.Services;

public class GeminiRecognitionService : IRecognitionService
{
    private readonly HttpClient _httpClient;
    private readonly RecognitionOptions _recognitionOptions;
    private readonly ILogger<GeminiRecognitionService> _logger;

    public GeminiRecognitionService(HttpClient httpClient, IOptions<RecognitionOptions> recognitionOptions,
        ILogger<GeminiRecognitionService> logger)
    {
        _httpClient = httpClient;
        _recognitionOptions = recognitionOptions.Value;
        _logger = logger;
    }

    public async Task<RecognitionResult> RecognizeTireCodeAsync(ImageDataHandle image, string contentType)
    {
        try
        {
            using var prompt = GetPromptJsonBody(image, contentType);
            _logger.LogInformation($"Sending recognition request via {nameof(GeminiRecognitionService)}");
            using var response = await _httpClient.PostAsync(GetPromptEndpointUri(), prompt);
            _logger.LogInformation($"Received recognition response via {nameof(GeminiRecognitionService)}");
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var responseDto = JsonSerializer.Deserialize<GeminiResponseDto>(responseContent);

            var responseString = responseDto?.ContentCandidates
                .FirstOrDefault(c => c.Content.Role == "model")?
                .Content.Parts
                .FirstOrDefault()?.Text;
            if (responseString is not null)
                _logger.LogInformation($"{nameof(GeminiRecognitionService)} responsed with '{responseString}'");

            var validResponseString = responseDto?.ContentCandidates
                .SelectMany(c => c.Content.Parts)
                .FirstOrDefault(p => !string.IsNullOrEmpty(p.Text) && p.Text.Contains('/'))
                ?.Text;

            string? foundTireCode = validResponseString;
            string? foundManufacturer = null;

            if (validResponseString is not null)
            {
                var indexOfManufacturerSplit = validResponseString.IndexOf('|');
                var manufacturerFound = indexOfManufacturerSplit > 0;
                // Manufacturer presence is only optional
                if (manufacturerFound)
                {
                    foundTireCode = validResponseString.Substring(0, indexOfManufacturerSplit);
                    if (indexOfManufacturerSplit < validResponseString.Length - 1)
                        foundManufacturer = validResponseString.Substring(indexOfManufacturerSplit + 1);
                }
            }


            return new RecognitionResult(
                RecognizedTireCode: foundTireCode,
                RecognizedManufacturer: foundManufacturer,
                InputTokenCount: responseDto!.UsageMetadata.PromptTokenCount,
                OutputTokenCount: responseDto.UsageMetadata.CandidatesTokenCount
            );
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Recognition via {nameof(GeminiRecognitionService)} failed.");
            throw new RemoteRecognitionEngineFailed(nameof(GeminiRecognitionService));
        }
    }

    private string GetPromptEndpointUri() => $"{_recognitionOptions.VlmEndpoint}?key={_recognitionOptions.VlmApiKey}";

    private StringContent GetPromptJsonBody(ImageDataHandle image, string contentType)
    {
        var prompt = _recognitionOptions.VlmPrompt;
        var base64Image = Convert.ToBase64String(image.Bytes);
        var payload = new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new object[]
                    {
                        new { text = prompt },
                        new
                        {
                            inline_data = new
                            {
                                mime_type = contentType,
                                data = base64Image
                            }
                        }
                    }
                }
            },
            generationConfig = new
            {
                temperature = _recognitionOptions.VlmTemperature,
                seed = _recognitionOptions.VlmSeed,
                thinkingConfig = new
                {
                    thinkingBudget = 0
                },
                media_resolution = "MEDIA_RESOLUTION_HIGH"
            }
        };
        var jsonPayload = JsonSerializer.Serialize(payload);
        return new StringContent(jsonPayload, Encoding.UTF8, "application/json");
    }
}