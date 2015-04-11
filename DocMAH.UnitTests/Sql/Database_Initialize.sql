
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
	DROP DATABASE docmahUnitTests
END

CREATE DATABASE docmahUnitTests
