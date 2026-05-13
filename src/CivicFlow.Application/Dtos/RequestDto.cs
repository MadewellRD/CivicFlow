using CivicFlow.Domain.Entities;
using CivicFlow.Domain.Enums;

namespace CivicFlow.Application.Dtos;

public sealed record RequestDto(
    Guid Id,
    string RequestNumber,
    string Title,
    RequestCategory Category,
    RequestStatus Status,
    Guid AgencyId,
    Guid RequesterId,
    decimal EstimatedAmount,
    string BusinessJustification,
    DateTimeOffset? SubmittedAt)
{
    public static RequestDto FromEntity(Request request)
    {
        return new RequestDto(
            request.Id,
            request.RequestNumber,
            request.Title,
            request.Category,
            request.Status,
            request.AgencyId,
            request.RequesterId,
            request.EstimatedAmount,
            request.BusinessJustification,
            request.SubmittedAt);
    }
}
