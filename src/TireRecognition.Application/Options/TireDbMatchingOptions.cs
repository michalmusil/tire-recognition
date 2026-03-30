namespace TireRecognition.Application.Options;

public class TireDbMatchingOptions
{
    /// <summary>
    /// Endpoint for returning valid tire codes used for DbMatching process   
    /// </summary>
    public string TireCodeEndpointUri { get; set; } = "";

    /// <summary>
    /// Endpoint for returning valid manufacturers used for the DbMatching process  
    /// </summary>
    public string TireManufacturerEndpointUri { get; set; } = "";

    /// <summary>
    /// How long should the cached responses for valid tire codes and manufacturers be valid
    /// </summary>
    public int RemoteDbCacheExpirationMinutes { get; set; } = 60;
}