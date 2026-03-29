namespace TireRecognition.Infrastructure.Options;

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
}