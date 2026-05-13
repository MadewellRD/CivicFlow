using CivicFlow.Domain.Enums;

namespace CivicFlow.Domain.Entities;

public static class RequestWorkflow
{
    private static readonly IReadOnlyDictionary<RequestStatus, RequestStatus[]> AllowedTransitions =
        new Dictionary<RequestStatus, RequestStatus[]>
        {
            [RequestStatus.Draft] = [RequestStatus.Submitted, RequestStatus.Cancelled],
            [RequestStatus.Submitted] = [RequestStatus.Triage, RequestStatus.ReturnedForCorrection, RequestStatus.Cancelled],
            [RequestStatus.ReturnedForCorrection] = [RequestStatus.Submitted, RequestStatus.Cancelled],
            [RequestStatus.Triage] = [RequestStatus.AnalystReview, RequestStatus.TechnicalReview, RequestStatus.Rejected],
            [RequestStatus.AnalystReview] = [RequestStatus.TechnicalReview, RequestStatus.Approved, RequestStatus.Rejected, RequestStatus.Blocked],
            [RequestStatus.TechnicalReview] = [RequestStatus.Approved, RequestStatus.Blocked, RequestStatus.Rejected],
            [RequestStatus.Approved] = [RequestStatus.Implemented, RequestStatus.Cancelled],
            [RequestStatus.Implemented] = [RequestStatus.Closed, RequestStatus.Reopened],
            [RequestStatus.Reopened] = [RequestStatus.Triage, RequestStatus.Closed],
            [RequestStatus.Blocked] = [RequestStatus.Triage, RequestStatus.AnalystReview, RequestStatus.TechnicalReview, RequestStatus.Cancelled],
            [RequestStatus.Rejected] = [RequestStatus.Reopened],
            [RequestStatus.Closed] = [RequestStatus.Reopened],
            [RequestStatus.Cancelled] = []
        };

    public static bool CanTransition(RequestStatus currentStatus, RequestStatus nextStatus)
    {
        return AllowedTransitions.TryGetValue(currentStatus, out var allowed) && allowed.Contains(nextStatus);
    }

    public static IReadOnlyCollection<RequestStatus> GetAllowedNextStatuses(RequestStatus currentStatus)
    {
        return AllowedTransitions.TryGetValue(currentStatus, out var allowed) ? allowed : [];
    }
}
