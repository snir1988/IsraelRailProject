IF OBJECT_ID('dbo.Signatures','U')       IS NOT NULL DROP TABLE dbo.Signatures;
IF OBJECT_ID('dbo.WorkFormRiskItems','U') IS NOT NULL DROP TABLE dbo.WorkFormRiskItems;
IF OBJECT_ID('dbo.RiskItems','U')        IS NOT NULL DROP TABLE dbo.RiskItems;
IF OBJECT_ID('dbo.WorkFormEmployees','U') IS NOT NULL DROP TABLE dbo.WorkFormEmployees;
IF OBJECT_ID('dbo.WorkForms','U')        IS NOT NULL DROP TABLE dbo.WorkForms;
IF OBJECT_ID('dbo.Users','U')            IS NOT NULL DROP TABLE dbo.Users;

CREATE TABLE dbo.Users (
  Id        INT IDENTITY PRIMARY KEY,
  FullName  NVARCHAR(100) NOT NULL,
  Email     NVARCHAR(150) NOT NULL UNIQUE,
  Pass      NVARCHAR(100) NOT NULL,
  Role      NVARCHAR(20)  NOT NULL CHECK (Role IN (N'Manager', N'Employee')),
  Phone     NVARCHAR(30)  NULL
);

CREATE TABLE dbo.WorkForms (
  Id             INT IDENTITY PRIMARY KEY,
  ManagerId      INT NOT NULL FOREIGN KEY REFERENCES dbo.Users(Id),
  Site           NVARCHAR(150) NOT NULL,
  WorkDateTime   DATETIME2     NOT NULL,
  WorkType       NVARCHAR(100) NOT NULL,
  Status         NVARCHAR(20)  NOT NULL DEFAULT(N'Draft'), -- Draft/Open/Completed
  Version        INT           NOT NULL DEFAULT(1),
  OriginalFormId INT           NULL,
  CreatedAt      DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME()
);

CREATE TABLE dbo.WorkFormEmployees (
  Id         INT IDENTITY PRIMARY KEY,
  WorkFormId INT NOT NULL FOREIGN KEY REFERENCES dbo.WorkForms(Id) ON DELETE CASCADE,
  EmployeeId INT NOT NULL FOREIGN KEY REFERENCES dbo.Users(Id),
  CONSTRAINT UQ_WorkFormEmployee UNIQUE (WorkFormId, EmployeeId)
);

CREATE TABLE dbo.RiskItems (
  Id   INT IDENTITY PRIMARY KEY,
  Name NVARCHAR(150) NOT NULL
);

CREATE TABLE dbo.WorkFormRiskItems (
  Id          INT IDENTITY PRIMARY KEY,
  WorkFormId  INT NOT NULL FOREIGN KEY REFERENCES dbo.WorkForms(Id) ON DELETE CASCADE,
  RiskItemId  INT NOT NULL FOREIGN KEY REFERENCES dbo.RiskItems(Id),
  CONSTRAINT UQ_WorkFormRisk UNIQUE (WorkFormId, RiskItemId)
);

CREATE TABLE dbo.Signatures (
  Id         INT IDENTITY PRIMARY KEY,
  WorkFormId INT NOT NULL FOREIGN KEY REFERENCES dbo.WorkForms(Id) ON DELETE CASCADE,
  EmployeeId INT NOT NULL FOREIGN KEY REFERENCES dbo.Users(Id),
  SignedAt   DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
  Version    INT       NOT NULL,
  CONSTRAINT UQ_Signature UNIQUE (WorkFormId, EmployeeId, Version)
);

-- Seed למיני-דמו (מנהל + 3 עובדים)
INSERT INTO dbo.Users (FullName, Email, Pass, Role, Phone) VALUES
(N'מנהל הדגמה', 'manager@demo', '1234', N'Manager',  N'050-0000000'),
(N'עובד 1',      'e1@demo',     '1234', N'Employee', N'050-1111111'),
(N'עובד 2',      'e2@demo',     '1234', N'Employee', N'050-2222222'),
(N'עובד 3',      'e3@demo',     '1234', N'Employee', N'050-3333333');

-- כמה פריטי סיכון לדוגמה (לא חובה)
INSERT INTO dbo.RiskItems (Name) VALUES
(N'עבודה ליד מסילה פעילה'),
(N'עבודה בגובה'),
(N'ציוד כבד בתנועה');
