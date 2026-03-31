using TireRecognition.Domain.DbMatching;

namespace TireRecognition.WebApi.Contracts.Run.Dtos;

public record ParameterMatchDto(
    int RequiredCharEdits,
    decimal EstimatedAccuracy
)
{
    public static ParameterMatchDto FromDomain(ParameterMatch domain) =>
        new(domain.RequiredCharEdits, domain.EstimatedAccuracy);
}