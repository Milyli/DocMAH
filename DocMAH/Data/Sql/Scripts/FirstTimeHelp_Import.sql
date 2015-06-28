IF EXISTS (SELECT 1 FROM [dbo].[DocmahFirstTimeHelp] WHERE [Id] = @id) BEGIN 
	UPDATE [dbo].[DocmahFirstTimeHelp] SET 
		[SourceUrl] = @sourceUrl,
		[Title] = @title,
		[Content] = @content,
		[VerticalOffset] = @verticalOffset,
		[HorizontalOffset] = @horizontalOffset,
		[OffsetElementId] = @offsetElementId
	WHERE [Id] = @id
END ELSE BEGIN
	INSERT [dbo].[DocmahFirstTimeHelp]	([Id], [SourceUrl], [Title], [Content], [VerticalOffset], [HorizontalOffset], [OffsetElementId]) 
	VALUES								(@id,  @sourceUrl,  @title,  @content,  @verticalOffset,  @horizontalOffset,  @offsetElementId)
END
