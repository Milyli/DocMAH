-- If the configuration table doesn't exist, return 0.
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[DocmahConfiguration]') AND type in (N'U')) BEGIN
	SELECT 0;
END

SELECT [Value] FROM [dbo].[DocmahConfiguration] WHERE [Name] = @name;
