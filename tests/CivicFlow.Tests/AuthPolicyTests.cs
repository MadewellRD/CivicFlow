using System.Security.Claims;
using CivicFlow.Api.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CivicFlow.Tests;

/// <summary>
/// Locks down the role-to-policy mapping so an accidental edit to
/// <c>AuthRegistration</c> cannot silently widen authorization.
/// </summary>
public sealed class AuthPolicyTests
{
    private readonly IAuthorizationService _authz;

    public AuthPolicyTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCivicFlowAuth();
        var provider = services.BuildServiceProvider();
        _authz = provider.GetRequiredService<IAuthorizationService>();
    }

    [Theory]
    [InlineData(AuthConstants.Policies.CanCreateRequest, AuthConstants.Roles.Requester, true)]
    [InlineData(AuthConstants.Policies.CanCreateRequest, AuthConstants.Roles.Admin, true)]
    [InlineData(AuthConstants.Policies.CanCreateRequest, AuthConstants.Roles.Approver, false)]
    [InlineData(AuthConstants.Policies.CanTriageRequest, AuthConstants.Roles.BudgetAnalyst, true)]
    [InlineData(AuthConstants.Policies.CanTriageRequest, AuthConstants.Roles.Requester, false)]
    [InlineData(AuthConstants.Policies.CanReviewTechnical, AuthConstants.Roles.ApplicationDeveloper, true)]
    [InlineData(AuthConstants.Policies.CanReviewTechnical, AuthConstants.Roles.Requester, false)]
    [InlineData(AuthConstants.Policies.CanApproveRequest, AuthConstants.Roles.Approver, true)]
    [InlineData(AuthConstants.Policies.CanApproveRequest, AuthConstants.Roles.BudgetAnalyst, false)]
    [InlineData(AuthConstants.Policies.CanImplementRequest, AuthConstants.Roles.ApplicationDeveloper, true)]
    [InlineData(AuthConstants.Policies.CanImplementRequest, AuthConstants.Roles.Requester, false)]
    [InlineData(AuthConstants.Policies.CanReopenRequest, AuthConstants.Roles.Admin, true)]
    [InlineData(AuthConstants.Policies.CanReopenRequest, AuthConstants.Roles.Approver, false)]
    [InlineData(AuthConstants.Policies.CanRunImport, AuthConstants.Roles.BudgetAnalyst, true)]
    [InlineData(AuthConstants.Policies.CanRunImport, AuthConstants.Roles.ReadOnlyAuditor, false)]
    [InlineData(AuthConstants.Policies.CanReadReferenceData, AuthConstants.Roles.ReadOnlyAuditor, true)]
    public async Task PolicyAllowsExpectedRole(string policy, string role, bool expected)
    {
        var principal = BuildPrincipal(role);
        var result = await _authz.AuthorizeAsync(principal, null, policy);
        Assert.Equal(expected, result.Succeeded);
    }

    [Fact]
    public async Task AnonymousUserIsAlwaysRejected()
    {
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        foreach (var policy in new[]
        {
            AuthConstants.Policies.CanCreateRequest,
            AuthConstants.Policies.CanTriageRequest,
            AuthConstants.Policies.CanApproveRequest,
            AuthConstants.Policies.CanRunImport,
            AuthConstants.Policies.AuthenticatedUser
        })
        {
            var result = await _authz.AuthorizeAsync(anonymous, null, policy);
            Assert.False(result.Succeeded, $"Anonymous user should not satisfy {policy}.");
        }
    }

    private static ClaimsPrincipal BuildPrincipal(string role)
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "fake-user"),
            new Claim(ClaimTypes.Role, role)
        }, authenticationType: "Test");
        return new ClaimsPrincipal(identity);
    }
}
