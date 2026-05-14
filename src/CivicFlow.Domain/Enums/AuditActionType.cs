namespace CivicFlow.Domain.Enums;

public enum AuditActionType
{
    Created = 0,
    Updated = 1,
    StatusChanged = 2,
    Assigned = 3,
    CommentAdded = 4,
    ImportValidated = 5,
    ImportTransformed = 6,
    IncidentRecorded = 7,
    AiExplanationGenerated = 8,
    AiTriageGenerated = 9,
    BusinessRuleExecuted = 10
}
