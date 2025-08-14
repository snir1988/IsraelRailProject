-- צרף פעם אחת במסד:
IF OBJECT_ID('dbo.WorkFormRiskItems','U') IS NULL
BEGIN
  CREATE TABLE dbo.WorkFormRiskItems(
    WorkFormId INT NOT NULL,
    RiskItemId INT NOT NULL,
    CONSTRAINT PK_WorkFormRiskItems PRIMARY KEY (WorkFormId, RiskItemId),
    CONSTRAINT FK_WorkFormRiskItems_WorkForms  FOREIGN KEY (WorkFormId) REFERENCES dbo.WorkForms(Id)  ON DELETE CASCADE,
    CONSTRAINT FK_WorkFormRiskItems_RiskItems  FOREIGN KEY (RiskItemId) REFERENCES dbo.RiskItems(Id)  ON DELETE CASCADE
  );
END
