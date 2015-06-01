
IF (
	EXISTS(
		SELECT name 
		FROM master.dbo.sysdatabases 
		WHERE '[' + name + ']' = 'CatalogName'
			OR name = 'CatalogName'
	)
) BEGIN
	DROP DATABASE CatalogName
END

CREATE DATABASE CatalogName
