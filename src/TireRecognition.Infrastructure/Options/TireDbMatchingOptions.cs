namespace TireRecognition.Infrastructure.Options;

public class TireDbMatchingOptions
{
    public required string TireCodeEndpointUri { get; set; }
    public required string TireManufacturerEndpointUri { get; set; }
}