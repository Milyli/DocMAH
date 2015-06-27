IF( (SELECT COUNT(*) FROM [dbo].[DocmahBullets] WHERE [Id] = @id) = 1)BEGIN
	UPDATE [dbo].[DocmahBullets] SET 
		[PageId] = @pageId,
		[Number] = @number,
		[Text] = @text,
		[VerticalOffset] = @verticalOffset,
		[HorizontalOffset] = @horizontalOffset,
		[OffsetElementId] = @offsetElementId,
		[DocVerticalOffset] = @docVerticalOffset,
		[DocHorizontalOffset] = @docHorizontalOffset 
	WHERE [Id] = @id
END ELSE BEGIN
	INSERT [dbo].[DocmahBullets](	[Id], [PageId], [Number], [Text], [VerticalOffset], [HorizontalOffset], [OffsetElementId], [DocVerticalOffset], [DocHorizontalOffset]) 
	VALUES						(	@id,  @pageId,  @number,  @text,  @verticalOffset,  @horizontalOffset,  @offsetElementId,  @docVerticalOffset,  @docHorizontalOffset)
END
