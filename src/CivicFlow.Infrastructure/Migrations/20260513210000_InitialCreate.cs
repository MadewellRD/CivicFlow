using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CivicFlow.Infrastructure.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
        CREATE TABLE dbo.Users (
            Id uniqueidentifier NOT NULL PRIMARY KEY,
            DisplayName nvarchar(200) NOT NULL,
            Email nvarchar(254) NOT NULL,
            PrimaryRole nvarchar(80) NOT NULL,
            CreatedAt datetimeoffset NOT NULL,
            CreatedByUserId uniqueidentifier NOT NULL,
            UpdatedAt datetimeoffset NULL,
            UpdatedByUserId uniqueidentifier NULL
        );
        CREATE UNIQUE INDEX IX_Users_Email ON dbo.Users(Email);

        CREATE TABLE dbo.Groups (
            Id uniqueidentifier NOT NULL PRIMARY KEY,
            Name nvarchar(120) NOT NULL,
            Description nvarchar(1000) NOT NULL,
            CreatedAt datetimeoffset NOT NULL,
            CreatedByUserId uniqueidentifier NOT NULL,
            UpdatedAt datetimeoffset NULL,
            UpdatedByUserId uniqueidentifier NULL
        );
        CREATE UNIQUE INDEX IX_Groups_Name ON dbo.Groups(Name);

        CREATE TABLE dbo.UserGroups (
            Id uniqueidentifier NOT NULL PRIMARY KEY,
            UserId uniqueidentifier NOT NULL,
            GroupId uniqueidentifier NOT NULL,
            CONSTRAINT FK_UserGroups_Users_UserId FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),
            CONSTRAINT FK_UserGroups_Groups_GroupId FOREIGN KEY (GroupId) REFERENCES dbo.Groups(Id)
        );
        CREATE UNIQUE INDEX IX_UserGroups_UserId_GroupId ON dbo.UserGroups(UserId, GroupId);

        CREATE TABLE dbo.Agencies (
            Id uniqueidentifier NOT NULL PRIMARY KEY,
            Code nvarchar(20) NOT NULL,
            Name nvarchar(200) NOT NULL,
            IsActive bit NOT NULL,
            CreatedAt datetimeoffset NOT NULL,
            CreatedByUserId uniqueidentifier NOT NULL,
            UpdatedAt datetimeoffset NULL,
            UpdatedByUserId uniqueidentifier NULL
        );
        CREATE UNIQUE INDEX IX_Agencies_Code ON dbo.Agencies(Code);

        CREATE TABLE dbo.Funds (
            Id uniqueidentifier NOT NULL PRIMARY KEY,
            Code nvarchar(20) NOT NULL,
            Name nvarchar(200) NOT NULL,
            IsActive bit NOT NULL,
            CreatedAt datetimeoffset NOT NULL,
            CreatedByUserId uniqueidentifier NOT NULL,
            UpdatedAt datetimeoffset NULL,
            UpdatedByUserId uniqueidentifier NULL
        );
        CREATE UNIQUE INDEX IX_Funds_Code ON dbo.Funds(Code);

        CREATE TABLE dbo.BudgetPrograms (
            Id uniqueidentifier NOT NULL PRIMARY KEY,
            Code nvarchar(20) NOT NULL,
            Name nvarchar(200) NOT NULL,
            AgencyId uniqueidentifier NOT NULL,
            IsActive bit NOT NULL,
            CreatedAt datetimeoffset NOT NULL,
            CreatedByUserId uniqueidentifier NOT NULL,
            UpdatedAt datetimeoffset NULL,
            UpdatedByUserId uniqueidentifier NULL,
            CONSTRAINT FK_BudgetPrograms_Agencies_AgencyId FOREIGN KEY (AgencyId) REFERENCES dbo.Agencies(Id)
        );
        CREATE UNIQUE INDEX IX_BudgetPrograms_AgencyId_Code ON dbo.BudgetPrograms(AgencyId, Code);

        CREATE TABLE dbo.CatalogItems (
            Id uniqueidentifier NOT NULL PRIMARY KEY,
            Name nvarchar(200) NOT NULL,
            Category nvarchar(80) NOT NULL,
            Description nvarchar(1000) NOT NULL,
            IsActive bit NOT NULL,
            CreatedAt datetimeoffset NOT NULL,
            CreatedByUserId uniqueidentifier NOT NULL,
            UpdatedAt datetimeoffset NULL,
            UpdatedByUserId uniqueidentifier NULL
        );

        CREATE TABLE dbo.CatalogFieldDefinitions (
            Id uniqueidentifier NOT NULL PRIMARY KEY,
            CatalogItemId uniqueidentifier NOT NULL,
            FieldKey nvarchar(120) NOT NULL,
            Label nvarchar(200) NOT NULL,
            FieldType nvarchar(80) NOT NULL,
            IsRequired bit NOT NULL,
            CONSTRAINT FK_CatalogFields_CatalogItems_CatalogItemId FOREIGN KEY (CatalogItemId) REFERENCES dbo.CatalogItems(Id) ON DELETE CASCADE
        );

        CREATE TABLE dbo.Requests (
            Id uniqueidentifier NOT NULL PRIMARY KEY,
            RequestNumber nvarchar(32) NOT NULL,
            Title nvarchar(200) NOT NULL,
            Category nvarchar(80) NOT NULL,
            AgencyId uniqueidentifier NOT NULL,
            RequesterId uniqueidentifier NOT NULL,
            FundId uniqueidentifier NULL,
            BudgetProgramId uniqueidentifier NULL,
            EstimatedAmount decimal(18,2) NOT NULL,
            BusinessJustification nvarchar(4000) NOT NULL,
            Status nvarchar(40) NOT NULL,
            AssignedGroupId uniqueidentifier NULL,
            SubmittedAt datetimeoffset NULL,
            CreatedAt datetimeoffset NOT NULL,
            CreatedByUserId uniqueidentifier NOT NULL,
            UpdatedAt datetimeoffset NULL,
            UpdatedByUserId uniqueidentifier NULL,
            CONSTRAINT FK_Requests_Agencies_AgencyId FOREIGN KEY (AgencyId) REFERENCES dbo.Agencies(Id),
            CONSTRAINT FK_Requests_Users_RequesterId FOREIGN KEY (RequesterId) REFERENCES dbo.Users(Id),
            CONSTRAINT FK_Requests_Funds_FundId FOREIGN KEY (FundId) REFERENCES dbo.Funds(Id),
            CONSTRAINT FK_Requests_BudgetPrograms_BudgetProgramId FOREIGN KEY (BudgetProgramId) REFERENCES dbo.BudgetPrograms(Id),
            CONSTRAINT FK_Requests_Groups_AssignedGroupId FOREIGN KEY (AssignedGroupId) REFERENCES dbo.Groups(Id)
        );
        CREATE UNIQUE INDEX IX_Requests_RequestNumber ON dbo.Requests(RequestNumber);

        CREATE TABLE dbo.RequestStatusHistory (
            Id uniqueidentifier NOT NULL PRIMARY KEY,
            RequestId uniqueidentifier NOT NULL,
            Status nvarchar(40) NOT NULL,
            ActorUserId uniqueidentifier NOT NULL,
            Reason nvarchar(1000) NOT NULL,
            OccurredAt datetimeoffset NOT NULL,
            CONSTRAINT FK_RequestStatusHistory_Requests_RequestId FOREIGN KEY (RequestId) REFERENCES dbo.Requests(Id) ON DELETE CASCADE,
            CONSTRAINT FK_RequestStatusHistory_Users_ActorUserId FOREIGN KEY (ActorUserId) REFERENCES dbo.Users(Id)
        );

        CREATE TABLE dbo.RequestComments (
            Id uniqueidentifier NOT NULL PRIMARY KEY,
            RequestId uniqueidentifier NOT NULL,
            AuthorUserId uniqueidentifier NOT NULL,
            Body nvarchar(4000) NOT NULL,
            CreatedAt datetimeoffset NOT NULL,
            CONSTRAINT FK_RequestComments_Requests_RequestId FOREIGN KEY (RequestId) REFERENCES dbo.Requests(Id) ON DELETE CASCADE,
            CONSTRAINT FK_RequestComments_Users_AuthorUserId FOREIGN KEY (AuthorUserId) REFERENCES dbo.Users(Id)
        );

        CREATE TABLE dbo.ImportBatches (
            Id uniqueidentifier NOT NULL PRIMARY KEY,
            FileName nvarchar(260) NOT NULL,
            UploadedByUserId uniqueidentifier NOT NULL,
            Status nvarchar(40) NOT NULL,
            CreatedAt datetimeoffset NOT NULL,
            CreatedByUserId uniqueidentifier NOT NULL,
            UpdatedAt datetimeoffset NULL,
            UpdatedByUserId uniqueidentifier NULL,
            CONSTRAINT FK_ImportBatches_Users_UploadedByUserId FOREIGN KEY (UploadedByUserId) REFERENCES dbo.Users(Id)
        );

        CREATE TABLE dbo.ImportStagingRows (
            Id uniqueidentifier NOT NULL PRIMARY KEY,
            ImportBatchId uniqueidentifier NOT NULL,
            RowNumber int NOT NULL,
            RequestNumber nvarchar(40) NOT NULL,
            AgencyCode nvarchar(20) NOT NULL,
            FundCode nvarchar(20) NOT NULL,
            ProgramCode nvarchar(20) NOT NULL,
            FiscalYear int NOT NULL,
            Amount decimal(18,2) NOT NULL,
            Title nvarchar(200) NOT NULL,
            EffectiveDateText nvarchar(40) NOT NULL,
            RowStatus nvarchar(40) NOT NULL,
            CONSTRAINT FK_ImportStagingRows_ImportBatches_ImportBatchId FOREIGN KEY (ImportBatchId) REFERENCES dbo.ImportBatches(Id) ON DELETE CASCADE
        );

        CREATE TABLE dbo.ImportValidationErrors (
            Id uniqueidentifier NOT NULL PRIMARY KEY,
            ImportStagingRowId uniqueidentifier NOT NULL,
            FieldName nvarchar(80) NOT NULL,
            Message nvarchar(500) NOT NULL,
            CONSTRAINT FK_ImportValidationErrors_ImportStagingRows_ImportStagingRowId FOREIGN KEY (ImportStagingRowId) REFERENCES dbo.ImportStagingRows(Id) ON DELETE CASCADE
        );

        CREATE TABLE dbo.AuditLogs (
            Id uniqueidentifier NOT NULL PRIMARY KEY,
            ActorUserId uniqueidentifier NOT NULL,
            ActionType nvarchar(80) NOT NULL,
            EntityName nvarchar(120) NOT NULL,
            EntityId uniqueidentifier NOT NULL,
            Summary nvarchar(1000) NOT NULL,
            BeforeJson nvarchar(max) NULL,
            AfterJson nvarchar(max) NULL,
            OccurredAt datetimeoffset NOT NULL,
            CONSTRAINT FK_AuditLogs_Users_ActorUserId FOREIGN KEY (ActorUserId) REFERENCES dbo.Users(Id)
        );
        CREATE INDEX IX_AuditLogs_EntityName_EntityId_OccurredAt ON dbo.AuditLogs(EntityName, EntityId, OccurredAt);

        CREATE TABLE dbo.IncidentReports (
            Id uniqueidentifier NOT NULL PRIMARY KEY,
            Title nvarchar(200) NOT NULL,
            Symptom nvarchar(2000) NOT NULL,
            RootCause nvarchar(2000) NOT NULL,
            CorrectiveAction nvarchar(2000) NOT NULL,
            CreatedAt datetimeoffset NOT NULL,
            CreatedByUserId uniqueidentifier NOT NULL,
            UpdatedAt datetimeoffset NULL,
            UpdatedByUserId uniqueidentifier NULL
        );

        CREATE TABLE dbo.NotificationMessages (
            Id uniqueidentifier NOT NULL PRIMARY KEY,
            RecipientUserId uniqueidentifier NOT NULL,
            Subject nvarchar(200) NOT NULL,
            Body nvarchar(2000) NOT NULL,
            CreatedAt datetimeoffset NOT NULL,
            IsSent bit NOT NULL,
            CONSTRAINT FK_NotificationMessages_Users_RecipientUserId FOREIGN KEY (RecipientUserId) REFERENCES dbo.Users(Id)
        );
        """);

        migrationBuilder.Sql("""
        INSERT INTO dbo.Users (Id, DisplayName, Email, PrimaryRole, CreatedAt, CreatedByUserId)
        VALUES
        ('10000000-0000-0000-0000-000000000001', 'Riley Requester', 'requester@example.gov', 'Requester', '1970-01-01T00:00:00+00:00', '10000000-0000-0000-0000-000000000001'),
        ('10000000-0000-0000-0000-000000000002', 'Bailey Analyst', 'analyst@example.gov', 'BudgetAnalyst', '1970-01-01T00:00:00+00:00', '10000000-0000-0000-0000-000000000002'),
        ('10000000-0000-0000-0000-000000000003', 'Casey Developer', 'developer@example.gov', 'ApplicationDeveloper', '1970-01-01T00:00:00+00:00', '10000000-0000-0000-0000-000000000003'),
        ('10000000-0000-0000-0000-000000000004', 'Avery Approver', 'approver@example.gov', 'Approver', '1970-01-01T00:00:00+00:00', '10000000-0000-0000-0000-000000000004');

        INSERT INTO dbo.Agencies (Id, Code, Name, IsActive, CreatedAt, CreatedByUserId)
        VALUES ('20000000-0000-0000-0000-000000000001', 'OFM', 'Office of Financial Management', 1, '1970-01-01T00:00:00+00:00', '10000000-0000-0000-0000-000000000004');

        INSERT INTO dbo.Funds (Id, Code, Name, IsActive, CreatedAt, CreatedByUserId)
        VALUES ('30000000-0000-0000-0000-000000000001', 'GF-S', 'General Fund-State', 1, '1970-01-01T00:00:00+00:00', '10000000-0000-0000-0000-000000000004');

        INSERT INTO dbo.BudgetPrograms (Id, Code, Name, AgencyId, IsActive, CreatedAt, CreatedByUserId)
        VALUES ('40000000-0000-0000-0000-000000000001', 'BUD', 'Budget Operations', '20000000-0000-0000-0000-000000000001', 1, '1970-01-01T00:00:00+00:00', '10000000-0000-0000-0000-000000000004');
        """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
        DROP TABLE IF EXISTS dbo.NotificationMessages;
        DROP TABLE IF EXISTS dbo.IncidentReports;
        DROP TABLE IF EXISTS dbo.AuditLogs;
        DROP TABLE IF EXISTS dbo.ImportValidationErrors;
        DROP TABLE IF EXISTS dbo.ImportStagingRows;
        DROP TABLE IF EXISTS dbo.ImportBatches;
        DROP TABLE IF EXISTS dbo.RequestComments;
        DROP TABLE IF EXISTS dbo.RequestStatusHistory;
        DROP TABLE IF EXISTS dbo.Requests;
        DROP TABLE IF EXISTS dbo.CatalogFieldDefinitions;
        DROP TABLE IF EXISTS dbo.CatalogItems;
        DROP TABLE IF EXISTS dbo.BudgetPrograms;
        DROP TABLE IF EXISTS dbo.Funds;
        DROP TABLE IF EXISTS dbo.Agencies;
        DROP TABLE IF EXISTS dbo.UserGroups;
        DROP TABLE IF EXISTS dbo.Groups;
        DROP TABLE IF EXISTS dbo.Users;
        """);
    }
}
