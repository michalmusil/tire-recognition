namespace TireRecognition.Infrastructure.Options;

public class RecognitionOptions
{
    public string VlmEndpoint { get; set; } = "";
    public int VlmTemperature { get; set; }
    public int VlmSeed { get; set; }
    public string VlmPrompt { get; set; } = "";
    public string VlmApiKey { get; set; } = "";
}