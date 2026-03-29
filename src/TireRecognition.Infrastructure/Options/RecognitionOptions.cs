namespace TireRecognition.Infrastructure.Options;

public class RecognitionOptions
{
    /// <summary>
    /// Endpoint of VLM for calling the text recognition query  
    /// </summary>
    public string VlmEndpoint { get; set; } = "";

    /// <summary>
    /// Temperature used for the VLM model  
    /// </summary>
    public int VlmTemperature { get; set; }

    /// <summary>
    /// Seed to use with the VLM model (for better output stability)  
    /// </summary>
    public int VlmSeed { get; set; }

    /// <summary>
    /// Prompt to use when calling text recognition query  
    /// </summary>
    public string VlmPrompt { get; set; } = "";

    /// <summary>
    /// Api key to the used VLM model  
    /// </summary>
    public string VlmApiKey { get; set; } = "";
}