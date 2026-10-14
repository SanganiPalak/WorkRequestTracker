-- Run in SSMS against a database you want to use for the assignment.
--CREATE DATABASE FlexGccWorkRequests;
--GO
--USE FlexGccWorkRequests;
--GO

CREATE TABLE dbo.WorkRequests
(
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_WorkRequests PRIMARY KEY,
    Title NVARCHAR(150) NOT NULL,
    ClientName NVARCHAR(120) NOT NULL,
    Description NVARCHAR(2000) NOT NULL,
    Priority NVARCHAR(20) NOT NULL,
    Status NVARCHAR(20) NOT NULL,
    DueDate DATETIME2 NOT NULL,
    CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_WorkRequests_CreatedDate DEFAULT SYSUTCDATETIME(),
    UpdatedDate DATETIME2 NOT NULL CONSTRAINT DF_WorkRequests_UpdatedDate DEFAULT SYSUTCDATETIME(),
    CONSTRAINT CK_WorkRequests_Priority CHECK (Priority IN ('Low', 'Medium', 'High')),
    CONSTRAINT CK_WorkRequests_Status CHECK (Status IN ('New', 'InProgress', 'Blocked', 'Completed'))
);
GO

CREATE TABLE dbo.WorkRequestNotes
(
    Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_WorkRequestNotes PRIMARY KEY,
    WorkRequestId INT NOT NULL,
    [Text] NVARCHAR(1000) NOT NULL,
    CreatedDate DATETIME2 NOT NULL CONSTRAINT DF_WorkRequestNotes_CreatedDate DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_WorkRequestNotes_WorkRequests FOREIGN KEY (WorkRequestId)
        REFERENCES dbo.WorkRequests(Id) ON DELETE CASCADE
);
GO

-- Helps status-filtered list queries.
CREATE INDEX IX_WorkRequests_Status ON dbo.WorkRequests(Status);
GO

-- Supports searches/queries starting with client/title values and is useful for
-- sorting/filtering in common list screens. Contains/substring search may still
-- require scanning; full-text search can be considered if the dataset grows.
CREATE INDEX IX_WorkRequests_ClientName_Title ON dbo.WorkRequests(ClientName, Title);
GO

CREATE INDEX IX_WorkRequestNotes_WorkRequestId_CreatedDate
ON dbo.WorkRequestNotes(WorkRequestId, CreatedDate DESC);
GO
