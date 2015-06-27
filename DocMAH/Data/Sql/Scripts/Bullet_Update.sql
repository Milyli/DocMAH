UPDATE [dbo].[DocmahBullets]
   SET [PageId] = @pageId
      ,[Number] = @number
      ,[Text] = @text
      ,[VerticalOffset] = @verticalOffset
      ,[HorizontalOffset] = @horizontalOffset
      ,[OffsetElementId] = @offsetElementId
	  ,[DocVerticalOffset] = @docVerticalOffset
	  ,[DocHorizontalOffset] = @docHorizontalOffset
 WHERE [Id] = @id