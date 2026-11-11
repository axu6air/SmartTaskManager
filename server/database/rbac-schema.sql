/* =====================================================================
   SmartTaskManager - full current schema (SQL Server). Source of truth.
   Filtered indexes require QUOTED_IDENTIFIER ON.
   ===================================================================== */

/* ---------------------------------------------------------------------
   0. DATABASE
      Idempotent: created only if missing. Development startup
      (DatabaseCreator) runs this script too, so the connection string
      must name this same database.
   --------------------------------------------------------------------- */
IF DB_ID(N'SmartTaskManagement') IS NULL
    CREATE DATABASE SmartTaskManagement;
GO

USE SmartTaskManagement;
GO

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
    IsAiModified BIT               NOT NULL CONSTRAINT DF_Projects_IsAiModified DEFAULT 0,
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
    IsAiModified BIT               NOT NULL CONSTRAINT DF_ProjectTasks_IsAiModified DEFAULT 0,
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

/* ---------------------------------------------------------------------
   11. REFERENCE SEED DATA (roles, permission modules, permissions, grants)
       Idempotent: every insert skips rows that already exist.
       Scope values: Assigned = 1, Managed = 2, Member = 4, All = 8.
       Permission codes must match Application/Common/Security/Permissions.cs.
   --------------------------------------------------------------------- */
INSERT INTO dbo.Roles (Name, NormalizedName, Description, IsSystem, IsDefault)
SELECT v.Name, v.NormalizedName, v.Description, v.IsSystem, v.IsDefault
FROM (VALUES
    (N'Admin',           N'ADMIN',           N'Full system access',                                   1, 0),
    (N'Project Manager', N'PROJECT MANAGER', N'Manages own projects, their members and tasks',        0, 0),
    (N'Team Member',     N'TEAM MEMBER',     N'Works on tasks in projects they belong to',            0, 1)
) AS v (Name, NormalizedName, Description, IsSystem, IsDefault)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Roles r WHERE r.NormalizedName = v.NormalizedName);

INSERT INTO dbo.PermissionModules (Code, Name, DisplayOrder)
SELECT v.Code, v.Name, v.DisplayOrder
FROM (VALUES
    ('users',     N'User Management',    1),
    ('roles',     N'Role Management',    2),
    ('projects',  N'Project Management', 3),
    ('tasks',     N'Task Management',    4),
    ('dashboard', N'Dashboard',          5),
    ('ai',        N'AI Assistant',       6)
) AS v (Code, Name, DisplayOrder)
WHERE NOT EXISTS (SELECT 1 FROM dbo.PermissionModules m WHERE m.Code = v.Code);

INSERT INTO dbo.Permissions (ModuleId, Code, Name, IsScopable, DisplayOrder)
SELECT m.Id, v.Code, v.Name, v.IsScopable, v.DisplayOrder
FROM (VALUES
    ('users',     'users.view',               N'View Users',                 0, 1),
    ('users',     'users.manage',             N'Manage Users',               0, 2),
    ('roles',     'roles.view',               N'View Roles',                 0, 1),
    ('roles',     'roles.manage',             N'Manage Roles',               0, 2),
    ('projects',  'projects.view',            N'View Project',               1, 1),
    ('projects',  'projects.create',          N'Create Project',             0, 2),
    ('projects',  'projects.update',          N'Update Project',             1, 3),
    ('projects',  'projects.delete',          N'Delete Project',             1, 4),
    ('projects',  'projects.manage_members',  N'Manage Project Members',     1, 5),
    ('tasks',     'tasks.view',               N'View Task',                  1, 1),
    ('tasks',     'tasks.create',             N'Create Task',                1, 2),
    ('tasks',     'tasks.update',             N'Update Task',                1, 3),
    ('tasks',     'tasks.delete',             N'Delete Task',                1, 4),
    ('tasks',     'tasks.assign',             N'Assign Task',                1, 5),
    ('tasks',     'tasks.update_status',      N'Update Task Status',         1, 6),
    ('dashboard', 'dashboard.view',           N'View Dashboard',             0, 1),
    ('ai',        'ai.enhance_description',   N'Enhance Task Description',   0, 1)
) AS v (ModuleCode, Code, Name, IsScopable, DisplayOrder)
JOIN dbo.PermissionModules m ON m.Code = v.ModuleCode
WHERE NOT EXISTS (SELECT 1 FROM dbo.Permissions p WHERE p.Code = v.Code);

-- Admin holds every permission with scope All.
INSERT INTO dbo.RolePermissions (RoleId, PermissionId, Scope)
SELECT r.Id, p.Id, 8
FROM dbo.Roles r
CROSS JOIN dbo.Permissions p
WHERE r.NormalizedName = N'ADMIN'
  AND NOT EXISTS (SELECT 1 FROM dbo.RolePermissions rp WHERE rp.RoleId = r.Id AND rp.PermissionId = p.Id);

INSERT INTO dbo.RolePermissions (RoleId, PermissionId, Scope)
SELECT r.Id, p.Id, g.Scope
FROM (VALUES
    (N'PROJECT MANAGER', 'ai.enhance_description',  8),
    (N'PROJECT MANAGER', 'dashboard.view',          8),
    (N'PROJECT MANAGER', 'projects.create',         8),
    (N'PROJECT MANAGER', 'projects.delete',         2),
    (N'PROJECT MANAGER', 'projects.manage_members', 2),
    (N'PROJECT MANAGER', 'projects.update',         2),
    (N'PROJECT MANAGER', 'projects.view',           4),
    (N'PROJECT MANAGER', 'tasks.assign',            2),
    (N'PROJECT MANAGER', 'tasks.create',            2),
    (N'PROJECT MANAGER', 'tasks.delete',            2),
    (N'PROJECT MANAGER', 'tasks.update',            2),
    (N'PROJECT MANAGER', 'tasks.update_status',     3),
    (N'PROJECT MANAGER', 'tasks.view',              4),
    (N'PROJECT MANAGER', 'users.view',              8),
    (N'TEAM MEMBER',     'ai.enhance_description',  8),
    (N'TEAM MEMBER',     'dashboard.view',          8),
    (N'TEAM MEMBER',     'projects.view',           4),
    (N'TEAM MEMBER',     'tasks.update_status',     1),
    (N'TEAM MEMBER',     'tasks.view',              4)
) AS g (NormalizedRoleName, PermissionCode, Scope)
JOIN dbo.Roles r ON r.NormalizedName = g.NormalizedRoleName
JOIN dbo.Permissions p ON p.Code = g.PermissionCode
WHERE NOT EXISTS (SELECT 1 FROM dbo.RolePermissions rp WHERE rp.RoleId = r.Id AND rp.PermissionId = p.Id);
GO
