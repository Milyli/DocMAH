﻿UPDATE [dbo].[DocmahUserPageSettings]
   SET [UserName] = @userName
      ,[PageId] = @pageId
      ,[HidePage] = @hidePage
 WHERE [Id] = @id