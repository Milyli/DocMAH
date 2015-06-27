
DECLARE @hitCount INT

SELECT @hitCount = COUNT(*)
FROM [dbo].[DocmahPageUrls]
WHERE @url = [Url]

IF @hitCount > 0 BEGIN	
	SELECT P.*
	FROM [dbo].[DocmahPages] AS P
		JOIN [dbo].[DocmahPageUrls] AS U ON U.[PageId] = P.[Id]
	WHERE @url = U.[Url]
END ELSE BEGIN
	SELECT P.*
	FROM [dbo].[DocmahPages] AS P
		JOIN [dbo].[DocmahPageUrls] AS U ON U.[PageId] = P.[Id]
	WHERE @url LIKE U.[Url]
END