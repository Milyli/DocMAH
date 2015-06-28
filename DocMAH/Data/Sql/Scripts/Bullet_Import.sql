IF( (SELECT COUNT(*) FROM [dbo].[DocmahBullets] WHERE [Id] = @id) = 1)BEGIN
	UPDATE [dbo].[DocmahBullets] SET 
		[PageId] = @pageId,
		[Number] = @number,
		[Text] = @text,
		[VerticalOffset] = @verticalOffset,
		[HorizontalOffset] = @horizontalOffset,
		[OffsetElementId] = @offsetElementId
	WHERE [Id] = @id
END ELSE BEGIN
	INSERT [dbo].[DocmahBullets](	[Id], [PageId], [Number], [Text], [VerticalOffset], [HorizontalOffset], [OffsetElementId]) 
	VALUES						(	@id,  @pageId,  @number,  @text,  @verticalOffset,  @horizontalOffset,  @offsetElementId)
END
