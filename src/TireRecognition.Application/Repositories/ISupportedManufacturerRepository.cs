namespace TireRecognition.Application.Repositories;

public interface ISupportedManufacturerRepository
{
    public Task<IEnumerable<string>> GetSupportedManufacturers();
}