namespace TireRecognition.Application.Options;

public class AuthOptions
{
    /// <summary>
    /// Determines whether authentication is enabled. If set to false, API is accessible to anyone.
    /// If set to true, every request must contain X-API-KEY header containing the api key specified by 'ApiKey' option.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// The name of the authentication scheme (can be arbitrary string)
    /// </summary>
    public string SchemeName { get; set; } = "";
    
    /// <summary>
    /// Sets the only acceptable X-API-KEY header value for authenticating requests when authentication is enabled
    /// </summary>
    public string ApiKey { get; set; } = "";
}