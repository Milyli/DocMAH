DELETE [dbo].[DocmahPageUrls] WHERE [PageId] NOT IN (@pageIds)
DELETE [dbo].[DocmahPages] WHERE [Id] NOT IN (@pageIds)