DECLARE @nextId INT 
SELECT @nextId = ISNULL(MAX([Id]), 0) + 1 FROM [dbo].[DocmahBullets]

INSERT INTO [DocmahBullets] (
	[Id]
	,[PageId]
	,[Number]
	,[Text]
	,[VerticalOffset]
	,[HorizontalOffset]
	,[OffsetElementId]
)
VALUES (
	@nextId
	,@pageId
	,@number
	,@text
	,@verticalOffset
	,@horizontalOffset
	,@offsetElementId
)

SELECT @nextId