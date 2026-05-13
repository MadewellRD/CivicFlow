namespace CivicFlow.Domain.Enums;

public enum RequestStatus
{
    Draft = 0,
    Submitted = 1,
    ReturnedForCorrection = 2,
    Triage = 3,
    AnalystReview = 4,
    TechnicalReview = 5,
    Approved = 6,
    Implemented = 7,
    Closed = 8,
    Rejected = 9,
    Blocked = 10,
    Cancelled = 11,
    Reopened = 12
}
