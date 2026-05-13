using CivicFlow.Domain.Common;

namespace CivicFlow.Domain.Entities;

public sealed class IncidentReport : AuditableEntity
{
    private IncidentReport()
    {
    }

    public IncidentReport(string title, string symptom, string rootCause, string correctiveAction, Guid createdByUserId)
    {
        Title = string.IsNullOrWhiteSpace(title) ? throw new DomainException("Incident title is required.") : title.Trim();
        Symptom = string.IsNullOrWhiteSpace(symptom) ? throw new DomainException("Symptom is required.") : symptom.Trim();
        RootCause = rootCause.Trim();
        CorrectiveAction = correctiveAction.Trim();
        CreatedByUserId = createdByUserId;
    }

    public string Title { get; private set; } = string.Empty;
    public string Symptom { get; private set; } = string.Empty;
    public string RootCause { get; private set; } = string.Empty;
    public string CorrectiveAction { get; private set; } = string.Empty;
}
