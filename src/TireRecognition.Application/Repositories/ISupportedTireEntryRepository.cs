using TireRecognition.Domain.DbMatching;

namespace TireRecognition.Application.Repositories;

public interface ISupportedTireEntryRepository
{
    public Task<IEnumerable<TireDbEntry>> GetSupportedTireEntries();
}