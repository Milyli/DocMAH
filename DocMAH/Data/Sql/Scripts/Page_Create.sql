-- Need to fake identity column so that Ids generated when writing documentation
-- are used in production so that installation scripts can update help 
-- without invalidating user settings.

DECLARE @nextId INT 
SELECT @nextId = ISNULL(MAX([Id]), 0) + 1 FROM [dbo].[DocmahPages]

INSERT INTO [dbo].[DocmahPages] (
	[Id]
	,[PageTypeId]
	,[ParentPageId]
	,[Order]
	,[SourceUrl]
	,[Title]
	,[Content]
	,[VerticalOffset]
	,[HorizontalOffset]
	,[OffsetElementId]
	,[DocImageUrl]
	,[DocVerticalOffset]
	,[DocHorizontalOffset]
	,[IsHidden]
)
VALUES (
	@nextId
	,@pageTypeId
	,@parentPageId
	,@order
	,@sourceUrl
	,@title
	,@content
	,@verticalOffset
	,@horizontalOffset
	,@offsetElementId
	,@docImageUrl
	,@docVerticalOffset
	,@docHorizontalOffset
	,@isHidden
)

SELECT @nextId;