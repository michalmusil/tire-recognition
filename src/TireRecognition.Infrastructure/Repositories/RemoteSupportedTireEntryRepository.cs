using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TireRecognition.Application.Options;
using TireRecognition.Application.Repositories;
using TireRecognition.Domain.DbMatching;
using TireRecognition.Infrastructure.Dtos.DbMatchingResponse;
using TireRecognition.Infrastructure.Exceptions;

namespace TireRecognition.Infrastructure.Repositories;

public class RemoteSupportedTireEntryRepository : ISupportedTireEntryRepository
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _responseCache;
    private readonly TireDbMatchingOptions _options;
    private readonly ILogger<RemoteSupportedTireEntryRepository> _logger;

    public RemoteSupportedTireEntryRepository(HttpClient httpClient, IMemoryCache responseCache,
        IOptions<TireDbMatchingOptions> options, ILogger<RemoteSupportedTireEntryRepository> logger)
    {
        _httpClient = httpClient;
        _responseCache = responseCache;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IEnumerable<TireDbEntry>> GetSupportedTireEntries()
    {
        try
        {
            var uri = _options.TireCodeEndpointUri;
            if (_responseCache.TryGetValue(uri, out IEnumerable<TireDbEntry>? result))
                return result!;

            var res = await _httpClient.GetAsync(uri);
            if (!res.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to access remote tire code db matching API: {res.StatusCode}");
                throw new RemoteDbMatchingUnreachableException(uri, (int)res.StatusCode);
            }

            var rawDbEntries = await res.Content.ReadFromJsonAsync<List<RawTireDbEntryDto>>();
            if (rawDbEntries is null)
                throw new JsonException("Failed to parse remote manufacturer db response");

            var dbEntries = rawDbEntries!
                .Select(raw => raw.ToDomain())
                .ToList();

            _responseCache.Set(uri, dbEntries, GetCacheEntryDuration());
            return dbEntries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected exception while getting manufacturers from remote DB.");
            throw;
        }
    }

    private TimeSpan GetCacheEntryDuration() => TimeSpan.FromMinutes(_options.RemoteDbCacheExpirationMinutes);
}