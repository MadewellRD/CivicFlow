namespace CivicFlow.Api.Auth;

/// <summary>
/// Auth constants for the CivicFlow demo handler.
///
/// In production this would be replaced by the WA OFM Entra ID / AD FS / SAML
/// identity provider. The handler is intentionally header-driven so the demo
/// can be operated by an interviewer without an IdP, while preserving the same
/// claim shape as a real federated login.
/// </summary>
public static class AuthConstants
{
    public const string DemoSchemeName = "CivicFlowDemo";
    public const string DemoUserHeader = "X-CivicFlow-User";

    public static class Policies
    {
        public const string AuthenticatedUser = "AuthenticatedUser";
        public const string CanCreateRequest = "CanCreateRequest";
        public const string CanTriageRequest = "CanTriageRequest";
        public const string CanReviewAnalyst = "CanReviewAnalyst";
        public const string CanReviewTechnical = "CanReviewTechnical";
        public const string CanApproveRequest = "CanApproveRequest";
        public const string CanImplementRequest = "CanImplementRequest";
        public const string CanCloseRequest = "CanCloseRequest";
        public const string CanReopenRequest = "CanReopenRequest";
        public const string CanRejectRequest = "CanRejectRequest";
        public const string CanRunImport = "CanRunImport";
        public const string CanReadReferenceData = "CanReadReferenceData";
    }

    public static class Roles
    {
        public const string Requester = "Requester";
        public const string BudgetAnalyst = "BudgetAnalyst";
        public const string ApplicationDeveloper = "ApplicationDeveloper";
        public const string Approver = "Approver";
        public const string Admin = "Admin";
        public const string ReadOnlyAuditor = "ReadOnlyAuditor";
    }

    public static class ClaimTypes
    {
        public const string UserId = "civicflow:user_id";
        public const string DisplayName = "civicflow:display_name";
    }
}
