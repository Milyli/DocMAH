WITH Pages_CTE
AS (
	SELECT 
		1 AS [Level],
		[Id],[ParentPageId],[Order],[Title],[IsHidden]
	FROM [dbo].[DocmahPages]
	WHERE ParentPageId IS NULL
		AND (@includeHidden = 1 OR [IsHidden] = 0)

	UNION ALL

	SELECT 
		[Parent].[Level] + 1 AS [Level],
		[Page].[Id],[Page].[ParentPageId],[Page].[Order],[Page].[Title],[Page].[IsHidden]
	FROM [dbo].[DocmahPages] AS [Page]
		JOIN Pages_CTE AS Parent ON [Page].[ParentPageId] = Parent.[Id]
	WHERE @includeHidden = 1 OR [Page].[IsHidden] = 0
)
SELECT [Id],[ParentPageId],[Order],[Title],[IsHidden]
FROM Pages_CTE
ORDER BY [Level],[Order],[Title]