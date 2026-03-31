using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TireRecognition.Application.Options;
using TireRecognition.Application.Repositories;
using TireRecognition.Infrastructure.Exceptions;

namespace TireRecognition.Infrastructure.Repositories;

public class RemoteSupportedManufacturerRepository : ISupportedManufacturerRepository
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _responseCache;
    private readonly TireDbMatchingOptions _options;
    private readonly ILogger<RemoteSupportedManufacturerRepository> _logger;

    public RemoteSupportedManufacturerRepository(HttpClient httpClient, IMemoryCache responseCache,
        IOptions<TireDbMatchingOptions> options, ILogger<RemoteSupportedManufacturerRepository> logger)
    {
        _httpClient = httpClient;
        _responseCache = responseCache;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IEnumerable<string>> GetSupportedManufacturers()
    {
        try
        {
            var uri = _options.TireManufacturerEndpointUri;
            if (_responseCache.TryGetValue(uri, out IEnumerable<string>? result))
                return result!;

            var res = await _httpClient.GetAsync(uri);
            if (!res.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to access remote manufacturer db matching API: {res.StatusCode}");
                throw new RemoteDbMatchingUnreachableException(uri, (int)res.StatusCode);
            }

            var manufacturerNames = await res.Content.ReadFromJsonAsync<List<string>>();
            if (manufacturerNames is null)
                throw new JsonException("Failed to parse remote manufacturer db response");

            _responseCache.Set(uri, manufacturerNames, GetCacheEntryDuration());
            return manufacturerNames;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected exception while getting manufacturers from remote DB.");
            return [];
        }
    }

    private TimeSpan GetCacheEntryDuration() => TimeSpan.FromMinutes(_options.RemoteDbCacheExpirationMinutes);
}