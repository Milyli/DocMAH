
DECLARE @hitCount INT

SELECT @hitCount = COUNT(*)
FROM DocmahPageUrls
WHERE @url = Url

IF @hitCount > 0 BEGIN	
	SELECT P.*
	FROM DocmahPages AS P
		JOIN DocmahPageUrls AS U ON U.PageId = P.Id
	WHERE @url = U.Url
END ELSE BEGIN
	SELECT P.*
	FROM DocmahPages AS P
		JOIN DocmahPageUrls AS U ON U.PageId = P.Id
	WHERE @url LIKE U.Url
END