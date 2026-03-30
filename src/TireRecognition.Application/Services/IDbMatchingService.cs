using TireRecognition.Domain.DbMatching;
using TireRecognition.Domain.Postprocessing;

namespace TireRecognition.Application.Services;

public interface IDbMatchingService
{
    public Task<List<TireDbMatch>> GetOrderedDbMatchesForTireCodeAsync(TireCode tireCode, int? limit);
    public Task<string?> GetManufacturerNameDbMatch(string rawTireManufacturerName);
}