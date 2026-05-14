using System.Security.Claims;
using System.Text.Encodings.Web;
using CivicFlow.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CivicFlow.Api.Auth;

/// <summary>
/// Demo-only authentication handler.
///
/// Reads the <c>X-CivicFlow-User</c> request header, expects a seeded user id,
/// looks up the user in SQL Server, and issues a <see cref="ClaimsPrincipal"/>
/// carrying their primary role as a standard role claim. This mirrors the
/// claim shape a production IdP (Entra ID / AD FS) would emit, so the rest of
/// the pipeline — policies, attributes, audit writes — does not change when
/// the handler is swapped for a real provider.
///
/// The handler is intentionally simple: it does not validate signatures,
/// expirations, or audiences. It must never be enabled in production. The
/// dependency injection wiring marks the scheme name explicitly so an
/// operator can confirm which handler is active from a single grep.
/// </summary>
public sealed class DemoAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly CivicFlowDbContext _db;

    public DemoAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        CivicFlowDbContext db)
        : base(options, logger, encoder)
    {
        _db = db;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(AuthConstants.DemoUserHeader, out var headerValues))
        {
            return AuthenticateResult.NoResult();
        }

        var headerValue = headerValues.ToString();
        if (string.IsNullOrWhiteSpace(headerValue) || !Guid.TryParse(headerValue, out var userId))
        {
            return AuthenticateResult.Fail("X-CivicFlow-User header was present but not a valid user id.");
        }

        var user = await _db.Users.AsNoTracking()
            .FirstOrDefaultAsync(candidate => candidate.Id == userId);

        if (user is null)
        {
            return AuthenticateResult.Fail("X-CivicFlow-User did not resolve to a known CivicFlow user.");
        }

        var claims = new List<Claim>
        {
            new(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(System.Security.Claims.ClaimTypes.Name, user.DisplayName),
            new(System.Security.Claims.ClaimTypes.Email, user.Email),
            new(AuthConstants.ClaimTypes.UserId, user.Id.ToString()),
            new(AuthConstants.ClaimTypes.DisplayName, user.DisplayName),
            new(System.Security.Claims.ClaimTypes.Role, user.PrimaryRole.ToString())
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
