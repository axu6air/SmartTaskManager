/* ---------------------------------------------------------------------
   002. Add IsAiModified to Projects and ProjectTasks.
        Idempotent: each column is added only if it does not exist.
        The default fills existing rows with 0.
   --------------------------------------------------------------------- */
IF COL_LENGTH('dbo.Projects', 'IsAiModified') IS NULL
    ALTER TABLE dbo.Projects
        ADD IsAiModified BIT NOT NULL CONSTRAINT DF_Projects_IsAiModified DEFAULT 0;
GO

IF COL_LENGTH('dbo.ProjectTasks', 'IsAiModified') IS NULL
    ALTER TABLE dbo.ProjectTasks
        ADD IsAiModified BIT NOT NULL CONSTRAINT DF_ProjectTasks_IsAiModified DEFAULT 0;
GO
