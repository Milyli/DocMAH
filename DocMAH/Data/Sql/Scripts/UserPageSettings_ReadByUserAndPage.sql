SELECT *
FROM [dbo].[DocmahUserPageSettings]
WHERE 
	[UserName] = @userName 
	AND [PageId] = @pageId