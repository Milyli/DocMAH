
IF (
	EXISTS(
		SELECT name 
		FROM master.dbo.sysdatabases 
		WHERE '[' + name + ']' = 'CatalogName'
			OR name = 'CatalogName'
	)
) BEGIN
	ALTER DATABASE CatalogName SET SINGLE_USER WITH ROLLBACK IMMEDIATE

	DROP DATABASE CatalogName
END