UPDATE DocmahConfiguration
SET [Name] = 'HelpContentVersion'
WHERE [Name] = 'DatabaseHelpVersion'


-- Update database schema version.
UPDATE [DocmahConfiguration] SET [Value] = 2 WHERE [Name] = 'DatabaseSchemaVersion'
