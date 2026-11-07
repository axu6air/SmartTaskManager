/* =====================================================================
   SmartTaskManager - full current schema (SQL Server). Source of truth.
   Filtered indexes require QUOTED_IDENTIFIER ON.
   ===================================================================== */
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

/* ---------------------------------------------------------------------
   1. USERS
   --------------------------------------------------------------------- */
CREATE TABLE dbo.Users
(
    Id                 UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Users_Id DEFAULT NEWSEQUENTIALID(),
    FullName           NVARCHAR(150)     NOT NULL,
    Email              NVARCHAR(256)     NOT NULL,
    NormalizedEmail    NVARCHAR(256)     NOT NULL,
    PasswordHash       NVARCHAR(500)     NOT NULL,
    SecurityStamp      UNIQUEIDENTIFIER  NOT NULL CONSTRAINT DF_Users_SecurityStamp DEFAULT NEWID(),
    IsActive           BIT               NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT 1,
    AccessFailedCount  INT               NOT NULL CONSTRAINT DF_Users_AccessFailedCount DEFAULT 0,
    LockoutEnd         DATETIMEOFFSET    NULL,
    LastLoginAt        DATETIMEOFFSET    NULL,
    CreatedBy          UNIQUEIDENTIFIER NULL,
    CreatedAt          DATETIMEOFFSET    NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSDATETIMEOFFSET(),
    UpdatedBy          UNIQUEIDENTIFIER NULL,
    UpdatedAt          DATETIMEOFFSET    NULL,
    RowVersion         ROWVERSION        NOT NULL,
    CONSTRAINT PK_Users PRIMARY KEY (Id),
    CONSTRAINT UQ_Users_NormalizedEmail UNIQUE (NormalizedEmail),
    CONSTRAINT FK_Users_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES dbo.Users (Id),
    CONSTRAINT FK_Users_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES dbo.Users (Id)
);
GO

/* ---------------------------------------------------------------------
   2. ROLES (dynamic - Admin can create/rename/delete non-system roles)
   --------------------------------------------------------------------- */
CREATE TABLE dbo.Roles
(
    Id              UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Roles_Id DEFAULT NEWSEQUENTIALID(),
    Name            NVARCHAR(100)     NOT NULL,
    NormalizedName  NVARCHAR(100)     NOT NULL,
    Description     NVARCHAR(500)     NULL,
    IsSystem        BIT               NOT NULL CONSTRAINT DF_Roles_IsSystem DEFAULT 0,  -- cannot be deleted/edited
    IsDefault       BIT               NOT NULL CONSTRAINT DF_Roles_IsDefault DEFAULT 0, -- assigned on self-registration
    CreatedBy       UNIQUEIDENTIFIER NULL,
    CreatedAt       DATETIMEOFFSET    NOT NULL CONSTRAINT DF_Roles_CreatedAt DEFAULT SYSDATETIMEOFFSET(),
    UpdatedBy       UNIQUEIDENTIFIER NULL,
    UpdatedAt       DATETIMEOFFSET    NULL,
    RowVersion      ROWVERSION        NOT NULL,
    CONSTRAINT PK_Roles PRIMARY KEY (Id),
    CONSTRAINT UQ_Roles_NormalizedName UNIQUE (NormalizedName),
    CONSTRAINT FK_Roles_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES dbo.Users (Id),
    CONSTRAINT FK_Roles_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES dbo.Users (Id)
);

-- At most one default role.
CREATE UNIQUE INDEX UX_Roles_SingleDefault ON dbo.Roles (IsDefault) WHERE IsDefault = 1;
GO

/* ---------------------------------------------------------------------
   3a. PERMISSION MODULES (Project Management, Task Management, ...)
       Named so SSMS lists it directly before dbo.Permissions.
   --------------------------------------------------------------------- */
CREATE TABLE dbo.PermissionModules
(
    Id            UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_PermissionModules_Id DEFAULT NEWSEQUENTIALID(),
    Code          VARCHAR(50)       NOT NULL,   -- e.g. 'projects'
    Name          NVARCHAR(100)     NOT NULL,   -- e.g. 'Project Management'
    Description   NVARCHAR(500)     NULL,
    DisplayOrder  INT               NOT NULL CONSTRAINT DF_PermissionModules_DisplayOrder DEFAULT 0,
    CreatedBy     UNIQUEIDENTIFIER NULL,
    CreatedAt     DATETIMEOFFSET    NOT NULL CONSTRAINT DF_PermissionModules_CreatedAt DEFAULT SYSDATETIMEOFFSET(),
    UpdatedBy     UNIQUEIDENTIFIER NULL,
    UpdatedAt     DATETIMEOFFSET    NULL,
    CONSTRAINT PK_PermissionModules PRIMARY KEY (Id),
    CONSTRAINT UQ_PermissionModules_Code UNIQUE (Code),
    CONSTRAINT UQ_PermissionModules_Name UNIQUE (Name),
    CONSTRAINT FK_PermissionModules_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES dbo.Users (Id),
    CONSTRAINT FK_PermissionModules_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES dbo.Users (Id)
);
GO

/* ---------------------------------------------------------------------
   3b. PERMISSIONS (Create Project, Update Task, ...)
       Catalog is seeded from code, because endpoints reference these
       codes; the dynamic part is which roles hold them (RolePermissions).
   --------------------------------------------------------------------- */
CREATE TABLE dbo.Permissions
(
    Id            UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Permissions_Id DEFAULT NEWSEQUENTIALID(),
    ModuleId      UNIQUEIDENTIFIER NOT NULL,
    Code          VARCHAR(100)      NOT NULL,   -- e.g. 'tasks.update_status' (used in [HasPermission])
    Name          NVARCHAR(100)     NOT NULL,   -- e.g. 'Update Task Status'
    Description   NVARCHAR(500)     NULL,
    IsScopable    BIT               NOT NULL CONSTRAINT DF_Permissions_IsScopable DEFAULT 0,
    DisplayOrder  INT               NOT NULL CONSTRAINT DF_Permissions_DisplayOrder DEFAULT 0,
    CreatedBy     UNIQUEIDENTIFIER NULL,
    CreatedAt     DATETIMEOFFSET    NOT NULL CONSTRAINT DF_Permissions_CreatedAt DEFAULT SYSDATETIMEOFFSET(),
    UpdatedBy     UNIQUEIDENTIFIER NULL,
    UpdatedAt     DATETIMEOFFSET    NULL,
    CONSTRAINT PK_Permissions PRIMARY KEY (Id),
    CONSTRAINT FK_Permissions_Module FOREIGN KEY (ModuleId) REFERENCES dbo.PermissionModules (Id),
    CONSTRAINT UQ_Permissions_Code UNIQUE (Code),
    CONSTRAINT UQ_Permissions_Module_Name UNIQUE (ModuleId, Name),
    CONSTRAINT FK_Permissions_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES dbo.Users (Id),
    CONSTRAINT FK_Permissions_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES dbo.Users (Id)
);

CREATE INDEX IX_Permissions_ModuleId ON dbo.Permissions (ModuleId);
GO

/* ---------------------------------------------------------------------
   4. ROLE <-> PERMISSION grants (with scope)
      CreatedBy/CreatedAt = who granted it and when.
   --------------------------------------------------------------------- */
CREATE TABLE dbo.RolePermissions
(
    RoleId        UNIQUEIDENTIFIER NOT NULL,
    PermissionId  UNIQUEIDENTIFIER NOT NULL,
    Scope         TINYINT         NOT NULL CONSTRAINT DF_RolePermissions_Scope DEFAULT 8,
    CreatedBy     UNIQUEIDENTIFIER NULL,
    CreatedAt     DATETIMEOFFSET  NOT NULL CONSTRAINT DF_RolePermissions_CreatedAt DEFAULT SYSDATETIMEOFFSET(),
    UpdatedBy     UNIQUEIDENTIFIER NULL,
    UpdatedAt     DATETIMEOFFSET  NULL,
    CONSTRAINT PK_RolePermissions PRIMARY KEY (RoleId, PermissionId),
    CONSTRAINT FK_RolePermissions_Role FOREIGN KEY (RoleId) REFERENCES dbo.Roles (Id) ON DELETE CASCADE,
    CONSTRAINT FK_RolePermissions_Permission FOREIGN KEY (PermissionId) REFERENCES dbo.Permissions (Id),
    CONSTRAINT FK_RolePermissions_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES dbo.Users (Id),
    CONSTRAINT FK_RolePermissions_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES dbo.Users (Id),
    CONSTRAINT CK_RolePermissions_Scope CHECK (Scope BETWEEN 1 AND 15)
);

CREATE INDEX IX_RolePermissions_PermissionId ON dbo.RolePermissions (PermissionId);
GO

/* ---------------------------------------------------------------------
   5. USER <-> ROLE (many-to-many)
      No cascade from Roles: a role still assigned to users cannot be deleted.
      CreatedBy is NULL when the default role is assigned on self-registration.
   --------------------------------------------------------------------- */
CREATE TABLE dbo.UserRoles
(
    UserId     UNIQUEIDENTIFIER NOT NULL,
    RoleId     UNIQUEIDENTIFIER NOT NULL,
    CreatedBy  UNIQUEIDENTIFIER NULL,
    CreatedAt  DATETIMEOFFSET  NOT NULL CONSTRAINT DF_UserRoles_CreatedAt DEFAULT SYSDATETIMEOFFSET(),
    UpdatedBy  UNIQUEIDENTIFIER NULL,
    UpdatedAt  DATETIMEOFFSET  NULL,
    CONSTRAINT PK_UserRoles PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_UserRoles_User FOREIGN KEY (UserId) REFERENCES dbo.Users (Id),
    CONSTRAINT FK_UserRoles_Role FOREIGN KEY (RoleId) REFERENCES dbo.Roles (Id),
    CONSTRAINT FK_UserRoles_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES dbo.Users (Id),
    CONSTRAINT FK_UserRoles_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES dbo.Users (Id)
);

CREATE INDEX IX_UserRoles_RoleId ON dbo.UserRoles (RoleId);
GO

/* ---------------------------------------------------------------------
   6. REFRESH TOKENS (hashed, rotated, family-based reuse detection)
      CreatedBy = the token's user; UpdatedBy/UpdatedAt set on revocation.
   --------------------------------------------------------------------- */
CREATE TABLE dbo.RefreshTokens
(
    Id                 UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RefreshTokens_Id DEFAULT NEWSEQUENTIALID(),
    UserId             UNIQUEIDENTIFIER NOT NULL,
    TokenHash          CHAR(64)             NOT NULL,   -- SHA-256 hex of the raw token
    FamilyId           UNIQUEIDENTIFIER     NOT NULL,   -- same for all rotations of one login
    ExpiresAt          DATETIMEOFFSET       NOT NULL,
    CreatedByIp        VARCHAR(45)          NULL,
    RevokedAt          DATETIMEOFFSET       NULL,
    RevokedByIp        VARCHAR(45)          NULL,
    RevokeReason       NVARCHAR(100)        NULL,       -- Rotated / Logout / ReuseDetected / RoleChanged
    ReplacedByTokenId  UNIQUEIDENTIFIER NULL,
    CreatedBy          UNIQUEIDENTIFIER NULL,
    CreatedAt          DATETIMEOFFSET       NOT NULL CONSTRAINT DF_RefreshTokens_CreatedAt DEFAULT SYSDATETIMEOFFSET(),
    UpdatedBy          UNIQUEIDENTIFIER NULL,
    UpdatedAt          DATETIMEOFFSET       NULL,
    CONSTRAINT PK_RefreshTokens PRIMARY KEY (Id),
    CONSTRAINT UQ_RefreshTokens_TokenHash UNIQUE (TokenHash),
    CONSTRAINT FK_RefreshTokens_User FOREIGN KEY (UserId) REFERENCES dbo.Users (Id),
    CONSTRAINT FK_RefreshTokens_ReplacedBy FOREIGN KEY (ReplacedByTokenId) REFERENCES dbo.RefreshTokens (Id),
    CONSTRAINT FK_RefreshTokens_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES dbo.Users (Id),
    CONSTRAINT FK_RefreshTokens_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES dbo.Users (Id),
    CONSTRAINT CK_RefreshTokens_Expiry CHECK (ExpiresAt > CreatedAt)
);

CREATE INDEX IX_RefreshTokens_UserId_Active ON dbo.RefreshTokens (UserId) WHERE RevokedAt IS NULL;
CREATE INDEX IX_RefreshTokens_FamilyId ON dbo.RefreshTokens (FamilyId);
GO

/* ---------------------------------------------------------------------
   7. PROJECTS
      Managers are ProjectMembers rows with IsManager = 1.
   --------------------------------------------------------------------- */
CREATE TABLE dbo.Projects
(
    Id           UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Projects_Id DEFAULT NEWSEQUENTIALID(),
    Name         NVARCHAR(200)     NOT NULL,
    Description  NVARCHAR(2000)    NULL,
    StartDate    DATETIMEOFFSET    NULL,
    EndDate      DATETIMEOFFSET    NULL,
    IsDeleted    BIT               NOT NULL CONSTRAINT DF_Projects_IsDeleted DEFAULT 0,
    DeletedBy    UNIQUEIDENTIFIER NULL,
    DeletedAt    DATETIMEOFFSET    NULL,
    CreatedBy    UNIQUEIDENTIFIER NOT NULL,
    CreatedAt    DATETIMEOFFSET    NOT NULL CONSTRAINT DF_Projects_CreatedAt DEFAULT SYSDATETIMEOFFSET(),
    UpdatedBy    UNIQUEIDENTIFIER NULL,
    UpdatedAt    DATETIMEOFFSET    NULL,
    RowVersion   ROWVERSION        NOT NULL,
    CONSTRAINT PK_Projects PRIMARY KEY (Id),
    CONSTRAINT FK_Projects_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES dbo.Users (Id),
    CONSTRAINT FK_Projects_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES dbo.Users (Id),
    CONSTRAINT FK_Projects_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES dbo.Users (Id),
    CONSTRAINT CK_Projects_Dates CHECK (EndDate IS NULL OR StartDate IS NULL OR EndDate >= StartDate),
    CONSTRAINT CK_Projects_SoftDelete CHECK (
        (IsDeleted = 0 AND DeletedAt IS NULL AND DeletedBy IS NULL) OR
        (IsDeleted = 1 AND DeletedAt IS NOT NULL))
);

CREATE UNIQUE INDEX UX_Projects_Name_Active ON dbo.Projects (Name) WHERE IsDeleted = 0;
GO

/* ---------------------------------------------------------------------
   8. PROJECT MEMBERS (hard delete - membership is current state)
      IsManager = 1 grants "Managed" scope over the project.
      A project can have more than one manager.
   --------------------------------------------------------------------- */
CREATE TABLE dbo.ProjectMembers
(
    ProjectId  UNIQUEIDENTIFIER NOT NULL,
    UserId     UNIQUEIDENTIFIER NOT NULL,
    IsManager  BIT             NOT NULL CONSTRAINT DF_ProjectMembers_IsManager DEFAULT 0,
    CreatedBy  UNIQUEIDENTIFIER NOT NULL,
    CreatedAt  DATETIMEOFFSET  NOT NULL CONSTRAINT DF_ProjectMembers_CreatedAt DEFAULT SYSDATETIMEOFFSET(),
    UpdatedBy  UNIQUEIDENTIFIER NULL,
    UpdatedAt  DATETIMEOFFSET  NULL,
    CONSTRAINT PK_ProjectMembers PRIMARY KEY (ProjectId, UserId),
    CONSTRAINT FK_ProjectMembers_Project   FOREIGN KEY (ProjectId) REFERENCES dbo.Projects (Id),
    CONSTRAINT FK_ProjectMembers_User      FOREIGN KEY (UserId)    REFERENCES dbo.Users (Id),
    CONSTRAINT FK_ProjectMembers_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES dbo.Users (Id),
    CONSTRAINT FK_ProjectMembers_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES dbo.Users (Id)
);

-- "Which projects am I in / do I manage" (scope checks, project lists)
CREATE INDEX IX_ProjectMembers_UserId ON dbo.ProjectMembers (UserId) INCLUDE (IsManager);
GO

/* ---------------------------------------------------------------------
   9. TASKS  (entity name ProjectTask in C# to avoid clashing with
      System.Threading.Tasks.Task)

      Status:   1 = ToDo, 2 = InProgress, 3 = Completed, 4 = Cancelled
      Priority: 1 = Low,  2 = Medium,     3 = High,      4 = Critical
      Values start at 1 so an uninitialized enum (0) is rejected.

      Composite FK (ProjectId, AssigneeId) -> ProjectMembers guarantees the
      assignee is a member of THAT project. When AssigneeId is NULL the FK
      is not checked, so unassigned tasks are allowed.
   --------------------------------------------------------------------- */
CREATE TABLE dbo.ProjectTasks
(
    Id           UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_ProjectTasks_Id DEFAULT NEWSEQUENTIALID(),
    ProjectId    UNIQUEIDENTIFIER NOT NULL,
    Title        NVARCHAR(200)     NOT NULL,
    Description  NVARCHAR(4000)    NULL,
    Status       TINYINT           NOT NULL CONSTRAINT DF_ProjectTasks_Status DEFAULT 1,
    Priority     TINYINT           NOT NULL CONSTRAINT DF_ProjectTasks_Priority DEFAULT 2,
    DueDate      DATETIMEOFFSET    NULL,
    AssigneeId   UNIQUEIDENTIFIER NULL,
    CompletedAt  DATETIMEOFFSET    NULL,
    IsDeleted    BIT               NOT NULL CONSTRAINT DF_ProjectTasks_IsDeleted DEFAULT 0,
    DeletedBy    UNIQUEIDENTIFIER NULL,
    DeletedAt    DATETIMEOFFSET    NULL,
    CreatedBy    UNIQUEIDENTIFIER NOT NULL,
    CreatedAt    DATETIMEOFFSET    NOT NULL CONSTRAINT DF_ProjectTasks_CreatedAt DEFAULT SYSDATETIMEOFFSET(),
    UpdatedBy    UNIQUEIDENTIFIER NULL,
    UpdatedAt    DATETIMEOFFSET    NULL,
    RowVersion   ROWVERSION        NOT NULL,
    CONSTRAINT PK_ProjectTasks PRIMARY KEY (Id),
    CONSTRAINT FK_ProjectTasks_Project  FOREIGN KEY (ProjectId) REFERENCES dbo.Projects (Id),
    CONSTRAINT FK_ProjectTasks_Assignee FOREIGN KEY (ProjectId, AssigneeId)
        REFERENCES dbo.ProjectMembers (ProjectId, UserId),
    CONSTRAINT FK_ProjectTasks_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES dbo.Users (Id),
    CONSTRAINT FK_ProjectTasks_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES dbo.Users (Id),
    CONSTRAINT FK_ProjectTasks_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES dbo.Users (Id),
    CONSTRAINT CK_ProjectTasks_Status   CHECK (Status   BETWEEN 1 AND 4),
    CONSTRAINT CK_ProjectTasks_Priority CHECK (Priority BETWEEN 1 AND 4),
    CONSTRAINT CK_ProjectTasks_Completed CHECK (
        (Status = 3 AND CompletedAt IS NOT NULL) OR (Status <> 3 AND CompletedAt IS NULL)),
    CONSTRAINT CK_ProjectTasks_SoftDelete CHECK (
        (IsDeleted = 0 AND DeletedAt IS NULL AND DeletedBy IS NULL) OR
        (IsDeleted = 1 AND DeletedAt IS NOT NULL))
);

-- Project task lists and dashboard counts by status/priority
CREATE INDEX IX_ProjectTasks_Project_Status
    ON dbo.ProjectTasks (ProjectId, Status)
    INCLUDE (Priority, DueDate, AssigneeId)
    WHERE IsDeleted = 0;

-- "My tasks" and Assigned-scope checks
CREATE INDEX IX_ProjectTasks_Assignee_Status
    ON dbo.ProjectTasks (AssigneeId, Status)
    INCLUDE (ProjectId, Priority, DueDate)
    WHERE IsDeleted = 0 AND AssigneeId IS NOT NULL;

-- Dashboard "upcoming due" (open tasks only)
CREATE INDEX IX_ProjectTasks_DueDate_Open
    ON dbo.ProjectTasks (DueDate)
    INCLUDE (ProjectId, AssigneeId, Priority)
    WHERE IsDeleted = 0 AND Status IN (1, 2);
GO

/* ---------------------------------------------------------------------
   10. AUDIT LOG (important in dynamic RBAC: who granted what, when)
       Append-only: UpdatedBy/UpdatedAt exist for convention and stay NULL.
   --------------------------------------------------------------------- */
CREATE TABLE dbo.AuditLogs
(
    Id          UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_AuditLogs_Id DEFAULT NEWSEQUENTIALID(),
    Action      VARCHAR(100)         NOT NULL,  -- e.g. 'RolePermission.Granted'
    EntityType  VARCHAR(100)         NOT NULL,
    EntityId    VARCHAR(100)         NOT NULL,  -- GUID, or 'guid:guid' for composite keys
    OldValues   NVARCHAR(MAX)        NULL,      -- JSON
    NewValues   NVARCHAR(MAX)        NULL,      -- JSON
    IpAddress   VARCHAR(45)          NULL,
    CreatedBy   UNIQUEIDENTIFIER NULL,      -- the actor; NULL for system actions
    CreatedAt   DATETIMEOFFSET       NOT NULL CONSTRAINT DF_AuditLogs_CreatedAt DEFAULT SYSDATETIMEOFFSET(),
    UpdatedBy   UNIQUEIDENTIFIER NULL,
    UpdatedAt   DATETIMEOFFSET       NULL,
    CONSTRAINT PK_AuditLogs PRIMARY KEY (Id),
    CONSTRAINT FK_AuditLogs_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES dbo.Users (Id),
    CONSTRAINT FK_AuditLogs_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES dbo.Users (Id)
);

CREATE INDEX IX_AuditLogs_Entity ON dbo.AuditLogs (EntityType, EntityId, CreatedAt);
GO
