WITH Pages_CTE
AS (
	SELECT 
		1 AS [Level],
		[Id],
		[ParentPageId],
		[Order]
	FROM [dbo].[DocmahPages]
	WHERE [ParentPageId] IS NULL

	UNION ALL

	SELECT 
		[Parent].[Level] + 1 AS [Level],
		[Page].[Id],
		[Page].[ParentPageId],
		[Page].[Order]
	FROM [dbo].[DocmahPages] AS [Page]
		JOIN Pages_CTE AS Parent ON [Page].[ParentPageId] = Parent.[Id]
)
SELECT Pages.*
FROM [dbo].[DocmahPages] AS Pages
	JOIN Pages_CTE AS Cte ON Pages.Id = Cte.Id
ORDER BY Cte.[Level],Cte.[Order]