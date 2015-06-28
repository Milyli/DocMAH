DELETE [dbo].[DocmahPageUrls] WHERE [PageId] NOT IN (@pageIds)
DELETE [dbo].[DocmahFirstTimeHelp] WHERE [Id] NOT IN (@pageIds)