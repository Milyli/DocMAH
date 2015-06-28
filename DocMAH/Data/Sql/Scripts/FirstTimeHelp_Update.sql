UPDATE [dbo].[DocmahFirstTimeHelp]
   SET [SourceUrl] = @sourceUrl
	  ,[Title] = @title
      ,[Content] = @content
	  ,[VerticalOffset] = @verticalOffset
	  ,[HorizontalOffset] = @horizontalOffset
	  ,[OffsetElementId] = @offsetElementId
 WHERE [Id] = @id