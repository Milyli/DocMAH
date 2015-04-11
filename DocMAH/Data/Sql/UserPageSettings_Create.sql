INSERT INTO [DocmahUserPageSettings](
	[UserName]
	,[PageId]
	,[HidePage]
)
OUTPUT Inserted.ID
VALUES (
	 @userName
	,@pageId
	,@hidePage
)