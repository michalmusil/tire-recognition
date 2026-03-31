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
    
    /// <summary>
    /// The default limit for results when matching recognized tire codes with actual tire code db entries 
    /// </summary>
    public int DefaultTireDbMatchingResultLimit { get; set; } = 30;
}