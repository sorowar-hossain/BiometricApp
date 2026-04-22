
-- Add TestBy in Demographics
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE Name = 'TestBy' 
    AND Object_ID = Object_ID('Demographics')
)
BEGIN
    ALTER TABLE Demographics ADD TestBy NVARCHAR(100) NULL;
END
