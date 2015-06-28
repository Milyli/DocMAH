-- Need to fake identity column so that Ids generated when writing documentation
-- are used in production so that installation scripts can update help 
-- without invalidating user settings.

DECLARE @nextId INT 
SELECT @nextId = ISNULL(MAX([Id]), 0) + 1 FROM [dbo].[DocmahFirstTimeHelp]

INSERT INTO [dbo].[DocmahFirstTimeHelp] (
	[Id]
	,[SourceUrl]
	,[Title]
	,[Content]
	,[VerticalOffset]
	,[HorizontalOffset]
	,[OffsetElementId]
)
VALUES (
	@nextId
	,@sourceUrl
	,@title
	,@content
	,@verticalOffset
	,@horizontalOffset
	,@offsetElementId
)

SELECT @nextId;