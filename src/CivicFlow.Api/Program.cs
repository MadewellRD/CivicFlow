using CivicFlow.Api;
using CivicFlow.Application.Dtos;
using CivicFlow.Application.Services;
using CivicFlow.Infrastructure;
using CivicFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks().AddDbContextCheck<CivicFlowDbContext>("sqlserver");
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevelopmentUi", policy => policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:4200"));
});

var app = builder.Build();

app.UseCors("DevelopmentUi");
app.MapHealthChecks("/health");

var api = app.MapGroup("/api");

api.MapGet("/requests", async (RequestWorkflowService service, CancellationToken cancellationToken) =>
{
    return Results.Ok(await service.ListAsync(cancellationToken));
});

api.MapGet("/requests/{id:guid}", async (Guid id, RequestWorkflowService service, CancellationToken cancellationToken) =>
{
    return Results.Ok(await service.GetAsync(id, cancellationToken));
});

api.MapPost("/requests", async (CreateRequestDto dto, RequestWorkflowService service, CancellationToken cancellationToken) =>
{
    var created = await service.CreateAsync(dto, cancellationToken);
    return Results.Created($"/api/requests/{created.Id}", created);
});

api.MapPost("/requests/{id:guid}/submit", async (Guid id, Guid actorUserId, RequestWorkflowService service, CancellationToken cancellationToken) =>
{
    return Results.Ok(await service.SubmitAsync(id, actorUserId, cancellationToken));
});

api.MapPost("/requests/{id:guid}/triage", async (Guid id, Guid actorUserId, RequestWorkflowService service, CancellationToken cancellationToken) =>
{
    return Results.Ok(await service.MoveToTriageAsync(id, actorUserId, cancellationToken));
});

api.MapPost("/requests/{id:guid}/analyst-review", async (Guid id, Guid actorUserId, RequestWorkflowService service, CancellationToken cancellationToken) =>
{
    return Results.Ok(await service.SendToAnalystReviewAsync(id, actorUserId, cancellationToken));
});

api.MapPost("/requests/{id:guid}/technical-review", async (Guid id, Guid actorUserId, RequestWorkflowService service, CancellationToken cancellationToken) =>
{
    return Results.Ok(await service.SendToTechnicalReviewAsync(id, actorUserId, cancellationToken));
});

api.MapPost("/requests/{id:guid}/approve", async (Guid id, Guid actorUserId, RequestWorkflowService service, CancellationToken cancellationToken) =>
{
    return Results.Ok(await service.ApproveAsync(id, actorUserId, cancellationToken));
});

api.MapPost("/requests/{id:guid}/implemented", async (Guid id, Guid actorUserId, RequestWorkflowService service, CancellationToken cancellationToken) =>
{
    return Results.Ok(await service.MarkImplementedAsync(id, actorUserId, cancellationToken));
});

api.MapPost("/requests/{id:guid}/close", async (Guid id, Guid actorUserId, RequestWorkflowService service, CancellationToken cancellationToken) =>
{
    return Results.Ok(await service.CloseAsync(id, actorUserId, cancellationToken));
});

api.MapPost("/requests/{id:guid}/reject", async (Guid id, RejectRequestBody body, RequestWorkflowService service, CancellationToken cancellationToken) =>
{
    return Results.Ok(await service.RejectAsync(id, body.ActorUserId, body.Reason, cancellationToken));
});

api.MapPost("/imports/budget-requests", async (CreateImportBatchRequest request, ImportValidationService service, CancellationToken cancellationToken) =>
{
    var summary = await service.CreateAndValidateBatchAsync(request.FileName, request.UploadedByUserId, request.Rows, cancellationToken);
    return Results.Created($"/api/imports/{summary.Id}", summary);
});

api.MapPost("/imports/{id:guid}/validate", async (Guid id, ImportValidationService service, CancellationToken cancellationToken) =>
{
    return Results.Ok(await service.ValidateExistingBatchAsync(id, cancellationToken));
});

api.MapGet("/reference/agencies", async (CivicFlowDbContext db, CancellationToken cancellationToken) =>
{
    return Results.Ok(await db.Agencies.OrderBy(agency => agency.Code).ToArrayAsync(cancellationToken));
});

api.MapGet("/reference/funds", async (CivicFlowDbContext db, CancellationToken cancellationToken) =>
{
    return Results.Ok(await db.Funds.OrderBy(fund => fund.Code).ToArrayAsync(cancellationToken));
});

api.MapGet("/integrations/legacy-budget/{agencyCode}/{fundCode}", (string agencyCode, string fundCode) =>
{
    var legacyResponse = new LegacyBudgetLookupResponse(agencyCode.ToUpperInvariant(), fundCode.ToUpperInvariant(), "ACTIVE", DateTimeOffset.UtcNow);
    return Results.Ok(legacyResponse);
});

app.Run();

public sealed record RejectRequestBody(Guid ActorUserId, string Reason);
public sealed record CreateImportBatchRequest(string FileName, Guid UploadedByUserId, IReadOnlyCollection<ImportRowDto> Rows);
public sealed record LegacyBudgetLookupResponse(string AgencyCode, string FundCode, string Status, DateTimeOffset CheckedAt);
