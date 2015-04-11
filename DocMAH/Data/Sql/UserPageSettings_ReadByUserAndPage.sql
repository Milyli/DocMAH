SELECT *
FROM DocmahUserPageSettings
WHERE 
	UserName = @userName 
	AND PageId = @pageId