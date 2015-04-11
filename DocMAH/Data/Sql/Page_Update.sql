UPDATE [DocmahPages]
   SET [PageTypeId] = @pageTypeId
      ,[ParentPageId] = @parentPageId
      ,[Order] = @order
      ,[SourceUrl] = @sourceUrl
      ,[Title] = @title
      ,[Content] = @content
      ,[VerticalOffset] = @verticalOffset
      ,[HorizontalOffset] = @horizontalOffset
      ,[OffsetElementId] = @offsetElementId
	  ,[DocImageUrl] = @docImageUrl
	  ,[DocVerticalOffset] = @docVerticalOffset
	  ,[DocHorizontalOffset] = @docHorizontalOffset
	  ,[IsHidden] = @isHidden
 WHERE Id = @id