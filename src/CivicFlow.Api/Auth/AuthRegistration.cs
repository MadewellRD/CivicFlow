using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace CivicFlow.Api.Auth;

public static class AuthRegistration
{
    /// <summary>
    /// Registers the CivicFlow demo authentication scheme and the role-based
    /// authorization policies that gate every workflow transition. In a real
    /// OFM deployment the scheme registration is the only line that changes —
    /// the policy table is portable across federated identity providers.
    /// </summary>
    public static IServiceCollection AddCivicFlowAuth(this IServiceCollection services)
    {
        services
            .AddAuthentication(AuthConstants.DemoSchemeName)
            .AddScheme<AuthenticationSchemeOptions, DemoAuthenticationHandler>(
                AuthConstants.DemoSchemeName,
                _ => { });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthConstants.Policies.AuthenticatedUser, policy =>
                policy.RequireAuthenticatedUser());

            options.AddPolicy(AuthConstants.Policies.CanCreateRequest, policy =>
                policy.RequireRole(AuthConstants.Roles.Requester, AuthConstants.Roles.Admin));

            options.AddPolicy(AuthConstants.Policies.CanTriageRequest, policy =>
                policy.RequireRole(AuthConstants.Roles.BudgetAnalyst, AuthConstants.Roles.Admin));

            options.AddPolicy(AuthConstants.Policies.CanReviewAnalyst, policy =>
                policy.RequireRole(AuthConstants.Roles.BudgetAnalyst, AuthConstants.Roles.Admin));

            options.AddPolicy(AuthConstants.Policies.CanReviewTechnical, policy =>
                policy.RequireRole(
                    AuthConstants.Roles.ApplicationDeveloper,
                    AuthConstants.Roles.BudgetAnalyst,
                    AuthConstants.Roles.Admin));

            options.AddPolicy(AuthConstants.Policies.CanApproveRequest, policy =>
                policy.RequireRole(AuthConstants.Roles.Approver, AuthConstants.Roles.Admin));

            options.AddPolicy(AuthConstants.Policies.CanImplementRequest, policy =>
                policy.RequireRole(
                    AuthConstants.Roles.ApplicationDeveloper,
                    AuthConstants.Roles.Approver,
                    AuthConstants.Roles.Admin));

            options.AddPolicy(AuthConstants.Policies.CanCloseRequest, policy =>
                policy.RequireRole(AuthConstants.Roles.Approver, AuthConstants.Roles.Admin));

            options.AddPolicy(AuthConstants.Policies.CanReopenRequest, policy =>
                policy.RequireRole(AuthConstants.Roles.Admin));

            options.AddPolicy(AuthConstants.Policies.CanRejectRequest, policy =>
                policy.RequireRole(
                    AuthConstants.Roles.BudgetAnalyst,
                    AuthConstants.Roles.Approver,
                    AuthConstants.Roles.Admin));

            options.AddPolicy(AuthConstants.Policies.CanRunImport, policy =>
                policy.RequireRole(AuthConstants.Roles.BudgetAnalyst, AuthConstants.Roles.Admin));

            options.AddPolicy(AuthConstants.Policies.CanReadReferenceData, policy =>
                policy.RequireAuthenticatedUser());
        });

        return services;
    }
}
