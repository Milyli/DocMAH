UPDATE [dbo].[DocmahDocumentationPages]
   SET [ParentPageId] = @parentPageId
      ,[Order] = @order
      ,[Title] = @title
      ,[Content] = @content
	  ,[IsHidden] = @isHidden
 WHERE [Id] = @id