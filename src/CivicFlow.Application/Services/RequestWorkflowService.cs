using CivicFlow.Application.Abstractions;
using CivicFlow.Application.Dtos;
using CivicFlow.Application.Platform;
using CivicFlow.Domain.Common;
using CivicFlow.Domain.Entities;
using CivicFlow.Domain.Enums;

namespace CivicFlow.Application.Services;

public sealed class RequestWorkflowService
{
    private readonly IRequestRepository _requests;
    private readonly IAuditWriter _auditWriter;
    private readonly INotificationService _notificationService;
    private readonly IClock _clock;
    private readonly BusinessRuleEngine _businessRuleEngine;

    public RequestWorkflowService(
        IRequestRepository requests,
        IAuditWriter auditWriter,
        INotificationService notificationService,
        IClock clock,
        BusinessRuleEngine businessRuleEngine)
    {
        _requests = requests;
        _auditWriter = auditWriter;
        _notificationService = notificationService;
        _clock = clock;
        _businessRuleEngine = businessRuleEngine;
    }

    public async Task<RequestDto> CreateAsync(CreateRequestDto dto, CancellationToken cancellationToken)
    {
        var request = new Request(
            dto.Title,
            dto.Category,
            dto.AgencyId,
            dto.RequesterId,
            dto.FundId,
            dto.BudgetProgramId,
            dto.EstimatedAmount,
            dto.BusinessJustification);

        request.SetRequestNumber($"CF-{_clock.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}");

        // Before-phase rules can still inspect or mutate the entity before persistence.
        await _businessRuleEngine.RunPhaseAsync(
            BusinessRulePhase.Before,
            new BusinessRuleContext(BusinessRuleTable.Request, BusinessRuleTrigger.Inserted, dto.RequesterId, request, null),
            cancellationToken);

        await _requests.AddAsync(request, cancellationToken);
        await _auditWriter.WriteAsync(dto.RequesterId, AuditActionType.Created, nameof(Request), request.Id, "Request created.", cancellationToken);
        await _requests.SaveChangesAsync(cancellationToken);

        // Async-phase rules can fire-and-forget side effects after the commit.
        await _businessRuleEngine.RunPhaseAsync(
            BusinessRulePhase.Async,
            new BusinessRuleContext(BusinessRuleTable.Request, BusinessRuleTrigger.Inserted, dto.RequesterId, request, null),
            cancellationToken);
        await _requests.SaveChangesAsync(cancellationToken);

        return RequestDto.FromEntity(request);
    }

    public Task<RequestDto> SubmitAsync(Guid requestId, Guid actorUserId, CancellationToken cancellationToken)
    {
        return TransitionAsync(requestId, RequestStatus.Submitted, actorUserId, "Request submitted for triage.", cancellationToken);
    }

    public Task<RequestDto> MoveToTriageAsync(Guid requestId, Guid actorUserId, CancellationToken cancellationToken)
    {
        return TransitionAsync(requestId, RequestStatus.Triage, actorUserId, "Request moved into triage.", cancellationToken);
    }

    public Task<RequestDto> SendToAnalystReviewAsync(Guid requestId, Guid actorUserId, CancellationToken cancellationToken)
    {
        return TransitionAsync(requestId, RequestStatus.AnalystReview, actorUserId, "Request assigned for analyst review.", cancellationToken);
    }

    public Task<RequestDto> SendToTechnicalReviewAsync(Guid requestId, Guid actorUserId, CancellationToken cancellationToken)
    {
        return TransitionAsync(requestId, RequestStatus.TechnicalReview, actorUserId, "Request assigned for technical review.", cancellationToken);
    }

    public Task<RequestDto> ApproveAsync(Guid requestId, Guid actorUserId, CancellationToken cancellationToken)
    {
        return TransitionAsync(requestId, RequestStatus.Approved, actorUserId, "Request approved.", cancellationToken);
    }

    public Task<RequestDto> MarkImplementedAsync(Guid requestId, Guid actorUserId, CancellationToken cancellationToken)
    {
        return TransitionAsync(requestId, RequestStatus.Implemented, actorUserId, "Implementation completed.", cancellationToken);
    }

    public Task<RequestDto> CloseAsync(Guid requestId, Guid actorUserId, CancellationToken cancellationToken)
    {
        return TransitionAsync(requestId, RequestStatus.Closed, actorUserId, "Request closed.", cancellationToken);
    }

    public Task<RequestDto> ReopenAsync(Guid requestId, Guid actorUserId, CancellationToken cancellationToken)
    {
        return TransitionAsync(requestId, RequestStatus.Reopened, actorUserId, "Request reopened.", cancellationToken);
    }

    public Task<RequestDto> RejectAsync(Guid requestId, Guid actorUserId, string reason, CancellationToken cancellationToken)
    {
        return TransitionAsync(requestId, RequestStatus.Rejected, actorUserId, reason, cancellationToken);
    }

    public async Task<IReadOnlyCollection<RequestDto>> ListAsync(CancellationToken cancellationToken)
    {
        var requests = await _requests.ListAsync(cancellationToken);
        return requests.Select(RequestDto.FromEntity).ToArray();
    }

    public async Task<RequestDto> GetAsync(Guid requestId, CancellationToken cancellationToken)
    {
        var request = await LoadRequestAsync(requestId, cancellationToken);
        return RequestDto.FromEntity(request);
    }

    private async Task<RequestDto> TransitionAsync(Guid requestId, RequestStatus nextStatus, Guid actorUserId, string reason, CancellationToken cancellationToken)
    {
        var request = await LoadRequestAsync(requestId, cancellationToken);
        var priorStatus = request.Status;
        request.TransitionTo(nextStatus, actorUserId, reason, _clock.UtcNow);
        await _auditWriter.WriteAsync(
            actorUserId,
            AuditActionType.StatusChanged,
            nameof(Request),
            request.Id,
            $"Request status changed from {priorStatus} to {nextStatus}.",
            cancellationToken);
        await _notificationService.EnqueueAsync(request.RequesterId, $"CivicFlow request {request.RequestNumber} updated", $"Status changed to {nextStatus}.", cancellationToken);
        await _requests.SaveChangesAsync(cancellationToken);

        // After-phase rules see the committed state. Run them post-save so a rule that
        // reads from the audit log or notification queue can observe the change.
        await _businessRuleEngine.RunPhaseAsync(
            BusinessRulePhase.After,
            new BusinessRuleContext(BusinessRuleTable.Request, BusinessRuleTrigger.StatusChanged, actorUserId, request, null),
            cancellationToken);
        await _requests.SaveChangesAsync(cancellationToken);

        return RequestDto.FromEntity(request);
    }

    private async Task<Request> LoadRequestAsync(Guid requestId, CancellationToken cancellationToken)
    {
        return await _requests.GetByIdAsync(requestId, cancellationToken)
            ?? throw new DomainException($"Request {requestId} was not found.");
    }
}
