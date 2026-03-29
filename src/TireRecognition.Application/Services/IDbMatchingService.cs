using TireRecognition.Domain.DbMatching;
using TireRecognition.Domain.Postprocessing;

namespace TireRecognition.Application.Services;

public interface IDbMatchingService
{
    public Task<DbMatchingResult> GetOrderedDbMatchesForTireCodeAsync(PostprocessingResult tireCode, int? limit);
}