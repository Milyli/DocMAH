SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

IF EXISTS (SELECT * FROM [dbo].[DocmahConfiguration] WHERE [Name] = 'DatabaseHelpVersion') BEGIN
	UPDATE DocmahConfiguration
	SET [Name] = 'HelpContentVersion'
	WHERE [Name] = 'DatabaseHelpVersion'
END



-- Update database schema version.
IF EXISTS (SELECT * FROM [dbo].[DocmahConfiguration] WHERE [Name] = 'DatabaseSchemaVersion') BEGIN
	UPDATE [dbo].[DocmahConfiguration] SET [Value] = 2 WHERE [Name] = 'DatabaseSchemaVersion'
END
ELSE BEGIN
	INSERT [DocmahConfiguration] VALUES ('DatabaseSchemaVersion', 2)
END
