-- Need to fake identity column so that Ids generated when writing documentation
-- are used in production so that installation scripts can update help 
-- without invalidating user settings.

DECLARE @nextId INT 
SELECT @nextId = ISNULL(MAX([Id]), 0) + 1 FROM [dbo].[DocmahDocumentationPages]

INSERT INTO [dbo].[DocmahDocumentationPages] (
	[Id]
	,[ParentPageId]
	,[Order]
	,[Title]
	,[Content]
	,[IsHidden]
)
VALUES (
	@nextId
	,@parentPageId
	,@order
	,@title
	,@content
	,@isHidden
)

SELECT @nextId;