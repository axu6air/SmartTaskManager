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
