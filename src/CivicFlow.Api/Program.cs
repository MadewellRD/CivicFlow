using System.Security.Claims;
using System.Text.Json.Serialization;
using CivicFlow.Api;
using CivicFlow.Api.Auth;
using CivicFlow.Application.Platform;
using CivicFlow.Application.Dtos;
using CivicFlow.Application.Services;
using CivicFlow.Infrastructure;
using CivicFlow.Infrastructure.Seeding;
using CivicFlow.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddCivicFlowAuth();
builder.Services.AddHealthChecks().AddDbContextCheck<CivicFlowDbContext>("sqlserver");
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>()
    ?? new[] { "http://localhost:4200" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("CivicFlowUi", policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

var app = builder.Build();

// Apply migrations and seed demo data at startup. Idempotent: safe to re-run.
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DemoDataSeeder>();
    await seeder.SeedAsync(CancellationToken.None);
}

app.UseCors("CivicFlowUi");
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health");
app.MapHealthChecks("/readyz");

var api = app.MapGroup("/api");

api.MapGet("/auth/me", (ClaimsPrincipal user) =>
{
    if (user.Identity?.IsAuthenticated != true)
    {
        return Results.Unauthorized();
    }
    var userId = user.FindFirst(AuthConstants.ClaimTypes.UserId)?.Value;
    var displayName = user.FindFirst(AuthConstants.ClaimTypes.DisplayName)?.Value;
    var role = user.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
    return Results.Ok(new CurrentUserResponse(userId ?? string.Empty, displayName ?? string.Empty, role ?? string.Empty));
}).RequireAuthorization(AuthConstants.Policies.AuthenticatedUser);

api.MapGet("/auth/users", async (CivicFlowDbContext db, CancellationToken cancellationToken) =>
{
    var users = await db.Users
        .OrderBy(u => u.PrimaryRole)
        .ThenBy(u => u.DisplayName)
        .Select(u => new RosterUserResponse(u.Id, u.DisplayName, u.Email, u.PrimaryRole.ToString()))
        .ToArrayAsync(cancellationToken);
    return Results.Ok(users);
});

api.MapGet("/requests", async (RequestWorkflowService service, CancellationToken cancellationToken) =>
    Results.Ok(await service.ListAsync(cancellationToken)))
    .RequireAuthorization(AuthConstants.Policies.AuthenticatedUser);

api.MapGet("/stats/overview", async (StatsService service, CancellationToken cancellationToken) =>
    Results.Ok(await service.GetOverviewAsync(cancellationToken)))
    .RequireAuthorization(AuthConstants.Policies.AuthenticatedUser);

api.MapGet("/requests/{id:guid}", async (Guid id, RequestWorkflowService service, CancellationToken cancellationToken) =>
    Results.Ok(await service.GetAsync(id, cancellationToken)))
    .RequireAuthorization(AuthConstants.Policies.AuthenticatedUser);

api.MapPost("/requests", async (CreateRequestDto dto, RequestWorkflowService service, CancellationToken cancellationToken) =>
{
    var created = await service.CreateAsync(dto, cancellationToken);
    return Results.Created($"/api/requests/{created.Id}", created);
}).RequireAuthorization(AuthConstants.Policies.CanCreateRequest);

api.MapPost("/requests/{id:guid}/submit", async (Guid id, [FromQuery] Guid actorUserId, RequestWorkflowService service, CancellationToken cancellationToken) =>
    Results.Ok(await service.SubmitAsync(id, actorUserId, cancellationToken)))
    .RequireAuthorization(AuthConstants.Policies.CanCreateRequest);

api.MapPost("/requests/{id:guid}/triage", async (Guid id, [FromQuery] Guid actorUserId, RequestWorkflowService service, CancellationToken cancellationToken) =>
    Results.Ok(await service.MoveToTriageAsync(id, actorUserId, cancellationToken)))
    .RequireAuthorization(AuthConstants.Policies.CanTriageRequest);

api.MapPost("/requests/{id:guid}/analyst-review", async (Guid id, [FromQuery] Guid actorUserId, RequestWorkflowService service, CancellationToken cancellationToken) =>
    Results.Ok(await service.SendToAnalystReviewAsync(id, actorUserId, cancellationToken)))
    .RequireAuthorization(AuthConstants.Policies.CanReviewAnalyst);

api.MapPost("/requests/{id:guid}/technical-review", async (Guid id, [FromQuery] Guid actorUserId, RequestWorkflowService service, CancellationToken cancellationToken) =>
    Results.Ok(await service.SendToTechnicalReviewAsync(id, actorUserId, cancellationToken)))
    .RequireAuthorization(AuthConstants.Policies.CanReviewTechnical);

api.MapPost("/requests/{id:guid}/approve", async (Guid id, [FromQuery] Guid actorUserId, RequestWorkflowService service, CancellationToken cancellationToken) =>
    Results.Ok(await service.ApproveAsync(id, actorUserId, cancellationToken)))
    .RequireAuthorization(AuthConstants.Policies.CanApproveRequest);

api.MapPost("/requests/{id:guid}/implemented", async (Guid id, [FromQuery] Guid actorUserId, RequestWorkflowService service, CancellationToken cancellationToken) =>
    Results.Ok(await service.MarkImplementedAsync(id, actorUserId, cancellationToken)))
    .RequireAuthorization(AuthConstants.Policies.CanImplementRequest);

api.MapPost("/requests/{id:guid}/close", async (Guid id, [FromQuery] Guid actorUserId, RequestWorkflowService service, CancellationToken cancellationToken) =>
    Results.Ok(await service.CloseAsync(id, actorUserId, cancellationToken)))
    .RequireAuthorization(AuthConstants.Policies.CanCloseRequest);

api.MapPost("/requests/{id:guid}/reject", async (Guid id, RejectRequestBody body, RequestWorkflowService service, CancellationToken cancellationToken) =>
    Results.Ok(await service.RejectAsync(id, body.ActorUserId, body.Reason, cancellationToken)))
    .RequireAuthorization(AuthConstants.Policies.CanRejectRequest);

api.MapPost("/requests/{id:guid}/triage-recommendation", async (Guid id, TriageRouterService service, System.Security.Claims.ClaimsPrincipal user, CancellationToken cancellationToken) =>
{
    var actorIdRaw = user.FindFirst(AuthConstants.ClaimTypes.UserId)?.Value;
    if (!Guid.TryParse(actorIdRaw, out var actorId))
    {
        return Results.Unauthorized();
    }
    return Results.Ok(await service.RecommendAsync(id, actorId, cancellationToken));
}).RequireAuthorization(AuthConstants.Policies.AuthenticatedUser);

api.MapPost("/imports/budget-requests", async (CreateImportBatchRequest request, ImportValidationService service, CancellationToken cancellationToken) =>
{
    var summary = await service.CreateAndValidateBatchAsync(request.FileName, request.UploadedByUserId, request.Rows, cancellationToken);
    return Results.Created($"/api/imports/{summary.Id}", summary);
}).RequireAuthorization(AuthConstants.Policies.CanRunImport);

api.MapGet("/imports/{id:guid}", async (Guid id, ImportValidationService service, CancellationToken cancellationToken) =>
    Results.Ok(await service.GetBatchSummaryAsync(id, cancellationToken)))
    .RequireAuthorization(AuthConstants.Policies.CanRunImport);

api.MapGet("/imports/{id:guid}/errors", async (Guid id, ImportValidationService service, CancellationToken cancellationToken) =>
{
    var summary = await service.GetBatchSummaryAsync(id, cancellationToken);
    return Results.Ok(summary.Rows.Where(row => row.Errors.Any()).ToArray());
}).RequireAuthorization(AuthConstants.Policies.CanRunImport);

api.MapPost("/imports/{id:guid}/validate", async (Guid id, ImportValidationService service, CancellationToken cancellationToken) =>
    Results.Ok(await service.ValidateExistingBatchAsync(id, cancellationToken)))
    .RequireAuthorization(AuthConstants.Policies.CanRunImport);

api.MapPost("/imports/{id:guid}/explain-errors", async (Guid id, ExplainErrorsBody body, ImportErrorExplainerService service, CancellationToken cancellationToken) =>
    Results.Ok(await service.ExplainAsync(id, body.ActorUserId, cancellationToken)))
    .RequireAuthorization(AuthConstants.Policies.CanRunImport);

api.MapPost("/imports/{id:guid}/transform", async (Guid id, TransformImportBody body, ImportValidationService service, CancellationToken cancellationToken) =>
    Results.Ok(await service.TransformBatchAsync(id, body.ActorUserId, cancellationToken)))
    .RequireAuthorization(AuthConstants.Policies.CanRunImport);

api.MapGet("/reference/agencies", async (CivicFlowDbContext db, CancellationToken cancellationToken) =>
    Results.Ok(await db.Agencies.OrderBy(agency => agency.Code).ToArrayAsync(cancellationToken)))
    .RequireAuthorization(AuthConstants.Policies.CanReadReferenceData);

api.MapGet("/reference/funds", async (CivicFlowDbContext db, CancellationToken cancellationToken) =>
    Results.Ok(await db.Funds.OrderBy(fund => fund.Code).ToArrayAsync(cancellationToken)))
    .RequireAuthorization(AuthConstants.Policies.CanReadReferenceData);

api.MapGet("/integrations/legacy-budget/{agencyCode}/{fundCode}", (string agencyCode, string fundCode) =>
{
    var legacyResponse = new LegacyBudgetLookupResponse(agencyCode.ToUpperInvariant(), fundCode.ToUpperInvariant(), "ACTIVE", DateTimeOffset.UtcNow);
    return Results.Ok(legacyResponse);
}).RequireAuthorization(AuthConstants.Policies.AuthenticatedUser);


// ---- Platform endpoints (ServiceNow-shape) ----
api.MapGet("/platform/ui-policies/{formName}", (string formName, UiPolicyCatalog catalog) =>
    Results.Ok(catalog.Policies.Where(p => p.FormName.Equals(formName, StringComparison.OrdinalIgnoreCase)).ToArray()))
    .RequireAuthorization(AuthConstants.Policies.AuthenticatedUser);

api.MapGet("/platform/transform-maps", (IEnumerable<ITransformMap> maps) =>
    Results.Ok(maps.Select(m => new
    {
        m.Name,
        m.SourceTable,
        m.TargetTable,
        FieldMaps = m.FieldMaps
    }).ToArray()))
    .RequireAuthorization(AuthConstants.Policies.AuthenticatedUser);

api.MapGet("/platform/business-rules", (IEnumerable<IBusinessRule> rules) =>
    Results.Ok(rules.Select(r => new { r.Name, Table = r.Table.ToString(), Phase = r.Phase.ToString(), r.Order }).ToArray()))
    .RequireAuthorization(AuthConstants.Policies.AuthenticatedUser);

app.Run();

public sealed record RejectRequestBody(Guid ActorUserId, string Reason);
public sealed record CreateImportBatchRequest(string FileName, Guid UploadedByUserId, IReadOnlyCollection<ImportRowDto> Rows);
public sealed record TransformImportBody(Guid ActorUserId);
public sealed record ExplainErrorsBody(Guid ActorUserId);
public sealed record LegacyBudgetLookupResponse(string AgencyCode, string FundCode, string Status, DateTimeOffset CheckedAt);
public sealed record CurrentUserResponse(string UserId, string DisplayName, string Role);
public sealed record RosterUserResponse(Guid Id, string DisplayName, string Email, string PrimaryRole);

public partial class Program;
