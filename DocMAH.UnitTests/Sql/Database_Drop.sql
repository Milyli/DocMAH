
DECLARE @databaseName NVARCHAR(128)
SET @databaseName = 'docmahUnitTests'

IF (
	EXISTS(
		SELECT name 
		FROM master.dbo.sysdatabases 
		WHERE '[' + name + ']' = @databaseName
			OR name = @databaseName
	)
) BEGIN
	ALTER DATABASE docmahUnitTests SET SINGLE_USER WITH ROLLBACK IMMEDIATE

	DROP DATABASE docmahUnitTests
END