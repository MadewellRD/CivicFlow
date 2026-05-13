using CivicFlow.Domain.Common;
using CivicFlow.Domain.Enums;

namespace CivicFlow.Domain.Entities;

public sealed class Request : AuditableEntity
{
    private readonly List<RequestStatusHistory> _statusHistory = [];
    private readonly List<RequestComment> _comments = [];

    private Request()
    {
    }

    public Request(
        string title,
        RequestCategory category,
        Guid agencyId,
        Guid requesterId,
        Guid? fundId,
        Guid? budgetProgramId,
        decimal estimatedAmount,
        string businessJustification)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new DomainException("Request title is required.");
        if (string.IsNullOrWhiteSpace(businessJustification)) throw new DomainException("Business justification is required.");
        if (estimatedAmount < 0) throw new DomainException("Estimated amount cannot be negative.");

        Title = title.Trim();
        Category = category;
        AgencyId = agencyId;
        RequesterId = requesterId;
        FundId = fundId;
        BudgetProgramId = budgetProgramId;
        EstimatedAmount = estimatedAmount;
        BusinessJustification = businessJustification.Trim();
        Status = RequestStatus.Draft;
        CreatedByUserId = requesterId;
        AddStatusHistory(RequestStatus.Draft, requesterId, "Request created in draft state.");
    }

    public string RequestNumber { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public RequestCategory Category { get; private set; }
    public Guid AgencyId { get; private set; }
    public Agency? Agency { get; private set; }
    public Guid RequesterId { get; private set; }
    public AppUser? Requester { get; private set; }
    public Guid? FundId { get; private set; }
    public Fund? Fund { get; private set; }
    public Guid? BudgetProgramId { get; private set; }
    public BudgetProgram? BudgetProgram { get; private set; }
    public decimal EstimatedAmount { get; private set; }
    public string BusinessJustification { get; private set; } = string.Empty;
    public RequestStatus Status { get; private set; }
    public Guid? AssignedGroupId { get; private set; }
    public Group? AssignedGroup { get; private set; }
    public DateTimeOffset? SubmittedAt { get; private set; }
    public IReadOnlyCollection<RequestStatusHistory> StatusHistory => _statusHistory.AsReadOnly();
    public IReadOnlyCollection<RequestComment> Comments => _comments.AsReadOnly();

    public void SetRequestNumber(string requestNumber)
    {
        if (!string.IsNullOrWhiteSpace(RequestNumber)) throw new DomainException("Request number has already been assigned.");
        RequestNumber = string.IsNullOrWhiteSpace(requestNumber) ? throw new DomainException("Request number is required.") : requestNumber.Trim().ToUpperInvariant();
    }

    public void TransitionTo(RequestStatus nextStatus, Guid actorUserId, string reason, DateTimeOffset now)
    {
        if (!RequestWorkflow.CanTransition(Status, nextStatus))
        {
            throw new DomainException($"Cannot transition request from {Status} to {nextStatus}.");
        }

        Status = nextStatus;
        UpdatedAt = now;
        UpdatedByUserId = actorUserId;

        if (nextStatus == RequestStatus.Submitted)
        {
            SubmittedAt = now;
        }

        AddStatusHistory(nextStatus, actorUserId, reason);
    }

    public void AssignToGroup(Guid groupId, Guid actorUserId, DateTimeOffset now)
    {
        AssignedGroupId = groupId;
        UpdatedAt = now;
        UpdatedByUserId = actorUserId;
    }

    public void AddComment(Guid authorUserId, string body, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(body)) throw new DomainException("Comment body is required.");
        _comments.Add(new RequestComment(Id, authorUserId, body.Trim(), now));
        UpdatedAt = now;
        UpdatedByUserId = authorUserId;
    }

    private void AddStatusHistory(RequestStatus status, Guid actorUserId, string reason)
    {
        _statusHistory.Add(new RequestStatusHistory(Id, status, actorUserId, reason));
    }
}
