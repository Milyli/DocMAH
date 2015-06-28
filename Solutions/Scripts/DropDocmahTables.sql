IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[DocmahBullets]') AND type in (N'U'))
DROP TABLE [DocmahBullets]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[DocmahConfiguration]') AND type in (N'U'))
DROP TABLE [DocmahConfiguration]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[DocmahPageUrls]') AND type in (N'U'))
DROP TABLE [DocmahPageUrls]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[DocmahUserPageSettings]') AND type in (N'U'))
DROP TABLE [DocmahUserPageSettings]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[DocmahUserSettings]') AND type in (N'U'))
DROP TABLE [DocmahUserSettings]

-- Old pages table. Just in case.
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[DocmahPages]') AND type in (N'U'))
DROP TABLE [DocmahPages]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[DocmahDocumentationPages]') AND type in (N'U'))
DROP TABLE [DocmahDocumentationPages]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[DocmahPageTypes]') AND type in (N'U'))
DROP TABLE [DocmahPageTypes]

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[DocmahFirstTimeHelp]') AND type in (N'U'))
DROP TABLE [DocmahFirstTimeHelp]


