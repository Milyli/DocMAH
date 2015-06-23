DELETE [DocmahPageUrls] WHERE PageId NOT IN (@pageIds)
DELETE [DocmahPages] WHERE Id NOT IN (@pageIds)