IF EXISTS (SELECT 1 FROM [dbo].[DocmahDocumentationPages] WHERE [Id] = @id) BEGIN 
	UPDATE [dbo].[DocmahDocumentationPages] SET 
		[ParentPageId] = @parentPageId,
		[Order] = @order,
		[Title] = @title,
		[Content] = @content,
		[IsHidden] = @isHidden 
	WHERE [Id] = @id
END ELSE BEGIN
	INSERT [dbo].[DocmahDocumentationPages]	([Id], [ParentPageId], [Order], [Title], [Content], [IsHidden]) 
	VALUES									(@id,  @parentPageId,  @order,  @title,  @content,  @isHidden)
END
