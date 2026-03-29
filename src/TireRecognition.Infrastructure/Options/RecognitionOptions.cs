namespace TireRecognition.Infrastructure.Options;

public class RecognitionOptions
{
    public int VlmTemperature { get; set; }
    public int VlmSeed { get; set; }
    public required string VlmApiKey { get; set; }
}