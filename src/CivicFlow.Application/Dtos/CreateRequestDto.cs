using CivicFlow.Domain.Enums;

namespace CivicFlow.Application.Dtos;

public sealed record CreateRequestDto(
    string Title,
    RequestCategory Category,
    Guid AgencyId,
    Guid RequesterId,
    Guid? FundId,
    Guid? BudgetProgramId,
    decimal EstimatedAmount,
    string BusinessJustification);
