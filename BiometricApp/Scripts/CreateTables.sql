
-- DEMOGRAPHICS TABLE

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Demographics')
BEGIN
    CREATE TABLE Demographics (
        PersonId INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NULL,
        OrgId INT NULL,
        FirstName NVARCHAR(100),
        LastName NVARCHAR(100),
        MaritalStatus NVARCHAR(50),
        PlaceOfIssue NVARCHAR(100),
        PlaceOfBirth NVARCHAR(100),
        DateOfBirth DATETIME,
        Gender NVARCHAR(20),
        Address NVARCHAR(255),
        Weight FLOAT,
        FatherName NVARCHAR(100),
        MotherName NVARCHAR(100),
        ExpiryDate DATETIME,
        PersonUniqueId NVARCHAR(100) NOT NULL UNIQUE,
        CreatedOn DATETIME,
        CreatedBy NVARCHAR(100),
        UpdatedOn DATETIME NULL,
        UpdatedBy NVARCHAR(100) NULL
    );
END


-- BIOMETRICS TABLE

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Biometrics')
BEGIN
    CREATE TABLE Biometrics (
        BiometricId INT IDENTITY(1,1) PRIMARY KEY,
        OrgId INT NULL,
        PersonId INT NOT NULL,
        UserId INT NULL,

        LeftThumb VARBINARY(MAX), LeftThumb_FileName NVARCHAR(200),
        LeftIndex VARBINARY(MAX), LeftIndex_FileName NVARCHAR(200),
        LeftMiddle VARBINARY(MAX), LeftMiddle_FileName NVARCHAR(200),
        LeftRing VARBINARY(MAX), LeftRing_FileName NVARCHAR(200),
        LeftLittle VARBINARY(MAX), LeftLittle_FileName NVARCHAR(200),

        RightThumb VARBINARY(MAX), RightThumb_FileName NVARCHAR(200),
        RightIndex VARBINARY(MAX), RightIndex_FileName NVARCHAR(200),
        RightMiddle VARBINARY(MAX), RightMiddle_FileName NVARCHAR(200),
        RightRing VARBINARY(MAX), RightRing_FileName NVARCHAR(200),
        RightLittle VARBINARY(MAX), RightLittle_FileName NVARCHAR(200),

        LeftIris VARBINARY(MAX), LeftIris_FileName NVARCHAR(200),
        RightIris VARBINARY(MAX), RightIris_FileName NVARCHAR(200),

        Face VARBINARY(MAX), Face_FileName NVARCHAR(200),

        CreatedOn DATETIME,
        CreatedBy NVARCHAR(100),
        UpdatedOn DATETIME,
        UpdatedBy NVARCHAR(100)
    );
END

-- Add FK safely
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Biometrics_Demographics')
BEGIN
    ALTER TABLE dbo.Biometrics
    ADD CONSTRAINT FK_Biometrics_Demographics
    FOREIGN KEY (PersonId)
    REFERENCES dbo.Demographics(PersonId)
    ON DELETE CASCADE;
END



-- BIOMETRIC LOGS TABLE

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BiometricLogs')
BEGIN
    CREATE TABLE BiometricLogs (
        BiometricId INT IDENTITY(1,1) PRIMARY KEY,
        OrgId INT NULL,
        PersonId INT NOT NULL,
        UserId INT NULL,

        LeftThumb VARBINARY(MAX), LeftThumb_FileName NVARCHAR(200),
        LeftIndex VARBINARY(MAX), LeftIndex_FileName NVARCHAR(200),
        LeftMiddle VARBINARY(MAX), LeftMiddle_FileName NVARCHAR(200),
        LeftRing VARBINARY(MAX), LeftRing_FileName NVARCHAR(200),
        LeftLittle VARBINARY(MAX), LeftLittle_FileName NVARCHAR(200),

        RightThumb VARBINARY(MAX), RightThumb_FileName NVARCHAR(200),
        RightIndex VARBINARY(MAX), RightIndex_FileName NVARCHAR(200),
        RightMiddle VARBINARY(MAX), RightMiddle_FileName NVARCHAR(200),
        RightRing VARBINARY(MAX), RightRing_FileName NVARCHAR(200),
        RightLittle VARBINARY(MAX), RightLittle_FileName NVARCHAR(200),

        LeftIris VARBINARY(MAX), LeftIris_FileName NVARCHAR(200),
        RightIris VARBINARY(MAX), RightIris_FileName NVARCHAR(200),

        Face VARBINARY(MAX), Face_FileName NVARCHAR(200),

        CreatedOn DATETIME,
        CreatedBy NVARCHAR(100),
        UpdatedOn DATETIME,
        UpdatedBy NVARCHAR(100)
    );
END

-- ✅ FIX: Different FK name
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_BiometricLogs_Demographics')
BEGIN
    ALTER TABLE dbo.BiometricLogs
    ADD CONSTRAINT FK_BiometricLogs_Demographics
    FOREIGN KEY (PersonId)
    REFERENCES dbo.Demographics(PersonId)
    ON DELETE CASCADE;
END



-- TRIGGER

IF OBJECT_ID('dbo.TRG_Biometric_Update_Log', 'TR') IS NOT NULL
    DROP TRIGGER dbo.TRG_Biometric_Update_Log;

EXEC('
CREATE TRIGGER dbo.TRG_Biometric_Update_Log
ON dbo.Biometrics
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO BiometricLogs (
        OrgId, PersonId, UserId,
        LeftThumb, LeftThumb_FileName,
        LeftIndex, LeftIndex_FileName,
        LeftMiddle, LeftMiddle_FileName,
        LeftRing, LeftRing_FileName,
        LeftLittle, LeftLittle_FileName,
        RightThumb, RightThumb_FileName,
        RightIndex, RightIndex_FileName,
        RightMiddle, RightMiddle_FileName,
        RightRing, RightRing_FileName,
        RightLittle, RightLittle_FileName,
        LeftIris, LeftIris_FileName,
        RightIris, RightIris_FileName,
        Face, Face_FileName,
        CreatedOn, CreatedBy, UpdatedOn, UpdatedBy
    )
    SELECT
        d.OrgId, d.PersonId, d.UserId,
        d.LeftThumb, d.LeftThumb_FileName,
        d.LeftIndex, d.LeftIndex_FileName,
        d.LeftMiddle, d.LeftMiddle_FileName,
        d.LeftRing, d.LeftRing_FileName,
        d.LeftLittle, d.LeftLittle_FileName,
        d.RightThumb, d.RightThumb_FileName,
        d.RightIndex, d.RightIndex_FileName,
        d.RightMiddle, d.RightMiddle_FileName,
        d.RightRing, d.RightRing_FileName,
        d.RightLittle, d.RightLittle_FileName,
        d.LeftIris, d.LeftIris_FileName,
        d.RightIris, d.RightIris_FileName,
        d.Face, d.Face_FileName,
        GETDATE(), d.CreatedBy, d.UpdatedOn, d.UpdatedBy
    FROM DELETED d
    INNER JOIN INSERTED i ON d.BiometricId = i.BiometricId
    WHERE
        -- Compare each column (NULL-safe)
        ISNULL(d.LeftThumb, 0x0) <> ISNULL(i.LeftThumb, 0x0) OR
        ISNULL(d.LeftThumb_FileName, '''') <> ISNULL(i.LeftThumb_FileName, '''') OR

        ISNULL(d.LeftIndex, 0x0) <> ISNULL(i.LeftIndex, 0x0) OR
        ISNULL(d.LeftIndex_FileName, '''') <> ISNULL(i.LeftIndex_FileName, '''') OR

        ISNULL(d.LeftMiddle, 0x0) <> ISNULL(i.LeftMiddle, 0x0) OR
        ISNULL(d.LeftMiddle_FileName, '''') <> ISNULL(i.LeftMiddle_FileName, '''') OR

        ISNULL(d.LeftRing, 0x0) <> ISNULL(i.LeftRing, 0x0) OR
        ISNULL(d.LeftRing_FileName, '''') <> ISNULL(i.LeftRing_FileName, '''') OR

        ISNULL(d.LeftLittle, 0x0) <> ISNULL(i.LeftLittle, 0x0) OR
        ISNULL(d.LeftLittle_FileName, '''') <> ISNULL(i.LeftLittle_FileName, '''') OR

        ISNULL(d.RightThumb, 0x0) <> ISNULL(i.RightThumb, 0x0) OR
        ISNULL(d.RightThumb_FileName, '''') <> ISNULL(i.RightThumb_FileName, '''') OR

        ISNULL(d.RightIndex, 0x0) <> ISNULL(i.RightIndex, 0x0) OR
        ISNULL(d.RightIndex_FileName, '''') <> ISNULL(i.RightIndex_FileName, '''') OR

        ISNULL(d.RightMiddle, 0x0) <> ISNULL(i.RightMiddle, 0x0) OR
        ISNULL(d.RightMiddle_FileName, '''') <> ISNULL(i.RightMiddle_FileName, '''') OR

        ISNULL(d.RightRing, 0x0) <> ISNULL(i.RightRing, 0x0) OR
        ISNULL(d.RightRing_FileName, '''') <> ISNULL(i.RightRing_FileName, '''') OR

        ISNULL(d.RightLittle, 0x0) <> ISNULL(i.RightLittle, 0x0) OR
        ISNULL(d.RightLittle_FileName, '''') <> ISNULL(i.RightLittle_FileName, '''') OR

        ISNULL(d.LeftIris, 0x0) <> ISNULL(i.LeftIris, 0x0) OR
        ISNULL(d.LeftIris_FileName, '''') <> ISNULL(i.LeftIris_FileName, '''') OR

        ISNULL(d.RightIris, 0x0) <> ISNULL(i.RightIris, 0x0) OR
        ISNULL(d.RightIris_FileName, '''') <> ISNULL(i.RightIris_FileName, '''') OR

        ISNULL(d.Face, 0x0) <> ISNULL(i.Face, 0x0) OR
        ISNULL(d.Face_FileName, '''') <> ISNULL(i.Face_FileName, '''');
END
');