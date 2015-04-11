DECLARE @nextId INT 
SELECT @nextId = ISNULL(MAX(Id), 0) + 1 FROM DocmahBullets

INSERT INTO [DocmahBullets] (
	Id
	,[PageId]
	,[Number]
	,[Text]
	,[VerticalOffset]
	,[HorizontalOffset]
	,[OffsetElementId]
	,[DocVerticalOffset]
	,[DocHorizontalOffset]
)
VALUES (
	@nextId
	,@pageId
	,@number
	,@text
	,@verticalOffset
	,@horizontalOffset
	,@offsetElementId
	,@docVerticalOffset
	,@docHorizontalOffset
)

SELECT @nextId