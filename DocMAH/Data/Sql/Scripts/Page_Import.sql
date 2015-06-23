﻿IF EXISTS (SELECT 1 FROM DocmahPages WHERE Id = @id) BEGIN 
	UPDATE DocmahPages SET 
		PageTypeId = @pageTypeId, 
		ParentPageId = @parentPageId,
		[Order] = @order,
		SourceUrl = @sourceUrl,
		Title = @title,
		Content = @content,
		VerticalOffset = @verticalOffset,
		HorizontalOffset = @horizontalOffset,
		OffsetElementId = @offsetElementId,
		DocImageUrl = @docImageUrl,
		DocVerticalOffset = @docVerticalOffset,
		DocHorizontalOffset = @docHorizontalOffset,
		IsHidden = @isHidden 
	WHERE Id = @id
END ELSE BEGIN
	INSERT DocmahPages(	 Id,  PageTypeId,  ParentPageId, [Order], SourceUrl,  Title,  Content,  VerticalOffset,  HorizontalOffset,  OffsetElementId,  DocImageUrl,  DocVerticalOffset,  DocHorizontalOffset,  IsHidden) 
	VALUES			(	@id, @pageTypeId, @parentPageId, @order, @sourceUrl, @title, @content, @verticalOffset, @horizontalOffset, @offsetElementId, @docImageUrl, @docVerticalOffset, @docHorizontalOffset, @isHidden)
END
