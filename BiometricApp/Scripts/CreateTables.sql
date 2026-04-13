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

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Biometrics')
BEGIN
    CREATE TABLE Biometrics (
        BiometricId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,

        OrgId INT NULL,
        PersonId INT NOT NULL,
        UserId INT NULL,

        -- LEFT HAND
        LeftThumb VARBINARY(MAX) NULL,
        LeftThumb_FileName NVARCHAR(200) NULL,

        LeftIndex VARBINARY(MAX) NULL,
        LeftIndex_FileName NVARCHAR(200) NULL,

        LeftMiddle VARBINARY(MAX) NULL,
        LeftMiddle_FileName NVARCHAR(200) NULL,

        LeftRing VARBINARY(MAX) NULL,
        LeftRing_FileName NVARCHAR(200) NULL,

        LeftLittle VARBINARY(MAX) NULL,
        LeftLittle_FileName NVARCHAR(200) NULL,

        -- RIGHT HAND
        RightThumb VARBINARY(MAX) NULL,
        RightThumb_FileName NVARCHAR(200) NULL,

        RightIndex VARBINARY(MAX) NULL,
        RightIndex_FileName NVARCHAR(200) NULL,

        RightMiddle VARBINARY(MAX) NULL,
        RightMiddle_FileName NVARCHAR(200) NULL,

        RightRing VARBINARY(MAX) NULL,
        RightRing_FileName NVARCHAR(200) NULL,

        RightLittle VARBINARY(MAX) NULL,
        RightLittle_FileName NVARCHAR(200) NULL,

        -- IRIS
        LeftIris VARBINARY(MAX) NULL,
        LeftIris_FileName NVARCHAR(200) NULL,

        RightIris VARBINARY(MAX) NULL,
        RightIris_FileName NVARCHAR(200) NULL,

        -- FACE
        Face VARBINARY(MAX) NULL,
        Face_FileName NVARCHAR(200) NULL,

        CreatedOn DATETIME NULL,
        CreatedBy NVARCHAR(100) NULL,
        UpdatedOn DATETIME NULL,
        UpdatedBy NVARCHAR(100) NULL,

        -- FOREIGN KEY
        CONSTRAINT FK_Biometrics_Demographics
            FOREIGN KEY (PersonId)
            REFERENCES Demographics(PersonId)
            ON DELETE CASCADE
    );
END