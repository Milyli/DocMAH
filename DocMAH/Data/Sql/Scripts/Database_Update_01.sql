SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON


IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DocmahBullets]') AND type in (N'U')) BEGIN
	CREATE TABLE [dbo].[DocmahBullets](
		[Id] [int] NOT NULL,
		[PageId] [int] NOT NULL,
		[Number] [int] NOT NULL,
		[Text] [nvarchar](max) NOT NULL,
		[VerticalOffset] [int] NOT NULL,
		[HorizontalOffset] [int] NOT NULL,
		[OffsetElementId] [nvarchar](50) NOT NULL,
		[DocVerticalOffset] [int] NULL,
		[DocHorizontalOffset] [int] NULL,
	 CONSTRAINT [PK_DocmahBullets] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF)
	)
END

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DocmahConfiguration]') AND type in (N'U')) BEGIN
	CREATE TABLE [dbo].[DocmahConfiguration](
		[Name] [nvarchar](50) NOT NULL,
		[Value] [int] NOT NULL,
	 CONSTRAINT [PK_DocmahConfiguration] PRIMARY KEY CLUSTERED 
	(
		[Name] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) 
	) 
END

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DocmahPages]') AND type in (N'U')) BEGIN
	CREATE TABLE [dbo].[DocmahPages](
		[Id] [int] NOT NULL,
		[PageTypeId] [int] NOT NULL,
		[ParentPageId] [int] NULL,
		[Order] [int] NOT NULL,
		[SourceUrl] [nvarchar](256) NULL,
		[Title] [nvarchar](50) NOT NULL,
		[Content] [nvarchar](max) NOT NULL,
		[VerticalOffset] [int] NULL,
		[HorizontalOffset] [int] NULL,
		[OffsetElementId] [nvarchar](50) NULL,
		[DocImageUrl] [nvarchar](256) NULL,
		[DocVerticalOffset] [int] NULL,
		[DocHorizontalOffset] [int] NULL,
		[IsHidden] [bit],
	 CONSTRAINT [PK_DocmahPages] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF)
	)
END

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DocmahPageTypes]') AND type in (N'U')) BEGIN
	CREATE TABLE [dbo].[DocmahPageTypes](
		[Id] [int] NOT NULL,
		[Name] [nvarchar](50) NOT NULL,
		[Description] [nvarchar](max) NOT NULL,
	 CONSTRAINT [PK_DocmahPageTypes] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF)
	)
END

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DocmahPageUrls]') AND type in (N'U')) BEGIN
	CREATE TABLE [dbo].[DocmahPageUrls](
		[Url] [nvarchar](256) NOT NULL,
		[PageId] [int] NOT NULL,
	 CONSTRAINT [PK_DocmahPageUrls] PRIMARY KEY CLUSTERED 
	(
		[Url] ASC
	)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF)
	)
END

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DocmahUserPageSettings]') AND type in (N'U')) BEGIN
	CREATE TABLE [dbo].[DocmahUserPageSettings](
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[UserName] [nvarchar](256) NOT NULL,
		[PageId] [int] NOT NULL,
		[HidePage] [bit] NOT NULL,
	 CONSTRAINT [PK_DocmahUserPageSettings] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF)
	)
END

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DocmahUserSettings]') AND type in (N'U')) BEGIN
	CREATE TABLE [dbo].[DocmahUserSettings](
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[UserName] [nvarchar](256) NOT NULL,
		[HideHelp] [bit] NOT NULL,
	 CONSTRAINT [PK_DocmahUserSettings] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF)
	)
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[DocmahBullets]') AND name = N'IX_DocmahBullets_PageId_Number') BEGIN
	CREATE NONCLUSTERED INDEX [IX_DocmahBullets_PageId_Number] ON [dbo].[DocmahBullets]
	(
		[PageId] ASC,
		[Number] ASC
	)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF)
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[DocmahPages]') AND name = N'IX_DocmahPages_ParentPageId_Order') BEGIN
	CREATE NONCLUSTERED INDEX [IX_DocmahPages_ParentPageId_Order] ON [dbo].[DocmahPages]
	(
		[ParentPageId] ASC,
		[Order] ASC
	)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF)
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[DocmahPageUrls]') AND name = N'IX_DocmahPageUrls_PageId') BEGIN
	CREATE NONCLUSTERED INDEX [IX_DocmahPageUrls_PageId] ON [dbo].[DocmahPageUrls]
	(
		[PageId] ASC
	)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF)
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[DocmahUserPageSettings]') AND name = N'IX_DocmahUserPageSettings_PageId_UserName') BEGIN
	CREATE NONCLUSTERED INDEX [IX_DocmahUserPageSettings_PageId_UserName] ON [dbo].[DocmahUserPageSettings]
	(
		[PageId] ASC,
		[UserName] ASC
	)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF)
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[DocmahUserSettings]') AND name = N'IX_DocmahUserSettings') BEGIN
	CREATE NONCLUSTERED INDEX [IX_DocmahUserSettings] ON [dbo].[DocmahUserSettings]
	(
		[UserName] ASC
	)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF)
END

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DocmahBullets_DocmahPages]') AND parent_object_id = OBJECT_ID(N'[dbo].[DocmahBullets]')) BEGIN
	ALTER TABLE [dbo].[DocmahBullets]  WITH CHECK ADD  CONSTRAINT [FK_DocmahBullets_DocmahPages] FOREIGN KEY([PageId])
	REFERENCES [dbo].[DocmahPages] ([Id])
END

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DocmahBullets_DocmahPages]') AND parent_object_id = OBJECT_ID(N'[dbo].[DocmahBullets]')) BEGIN
	ALTER TABLE [dbo].[DocmahBullets] CHECK CONSTRAINT [FK_DocmahBullets_DocmahPages]
END

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DocmahPages_DocmahPages]') AND parent_object_id = OBJECT_ID(N'[dbo].[DocmahPages]')) BEGIN
	ALTER TABLE [dbo].[DocmahPages]  WITH CHECK ADD  CONSTRAINT [FK_DocmahPages_DocmahPages] FOREIGN KEY([ParentPageId])
	REFERENCES [dbo].[DocmahPages] ([Id])
END

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DocmahPages_DocmahPages]') AND parent_object_id = OBJECT_ID(N'[dbo].[DocmahPages]')) BEGIN
	ALTER TABLE [dbo].[DocmahPages] CHECK CONSTRAINT [FK_DocmahPages_DocmahPages]
END

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DocmahPages_DocmahPageTypes]') AND parent_object_id = OBJECT_ID(N'[dbo].[DocmahPages]')) BEGIN
	ALTER TABLE [dbo].[DocmahPages]  WITH CHECK ADD  CONSTRAINT [FK_DocmahPages_DocmahPageTypes] FOREIGN KEY([PageTypeId])
	REFERENCES [dbo].[DocmahPageTypes] ([Id])
END
	
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DocmahPages_DocmahPageTypes]') AND parent_object_id = OBJECT_ID(N'[dbo].[DocmahPages]')) BEGIN
	ALTER TABLE [dbo].[DocmahPages] CHECK CONSTRAINT [FK_DocmahPages_DocmahPageTypes]
END
	
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DocmahPageUrls_DocmahPages]') AND parent_object_id = OBJECT_ID(N'[dbo].[DocmahPageUrls]')) BEGIN
	ALTER TABLE [dbo].[DocmahPageUrls]  WITH CHECK ADD  CONSTRAINT [FK_DocmahPageUrls_DocmahPages] FOREIGN KEY([PageId])
	REFERENCES [dbo].[DocmahPages] ([Id])
END
	
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DocmahPageUrls_DocmahPages]') AND parent_object_id = OBJECT_ID(N'[dbo].[DocmahPageUrls]')) BEGIN
	ALTER TABLE [dbo].[DocmahPageUrls] CHECK CONSTRAINT [FK_DocmahPageUrls_DocmahPages]
END
	
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DocmahUserPageSettings_DocmahPages]') AND parent_object_id = OBJECT_ID(N'[dbo].[DocmahUserPageSettings]')) BEGIN
	ALTER TABLE [dbo].[DocmahUserPageSettings]  WITH CHECK ADD  CONSTRAINT [FK_DocmahUserPageSettings_DocmahPages] FOREIGN KEY([PageId])
	REFERENCES [dbo].[DocmahPages] ([Id])
END
	
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DocmahUserPageSettings_DocmahPages]') AND parent_object_id = OBJECT_ID(N'[dbo].[DocmahUserPageSettings]')) BEGIN
	ALTER TABLE [dbo].[DocmahUserPageSettings] CHECK CONSTRAINT [FK_DocmahUserPageSettings_DocmahPages]
END


/* Initialize data */
IF NOT EXISTS (SELECT * FROM [dbo].[DocmahPageTypes] WHERE [Id] = 1) BEGIN
	INSERT [DocmahPageTypes] ([Id], [Name], [Description])
	VALUES (1, 'FirstTimePage', 'Help that show over a live application page the first time the page is shown to the user. The help is also included in documentation.')
END

IF NOT EXISTS (SELECT * FROM [dbo].[DocmahPageTypes] WHERE [Id] = 2) BEGIN
	INSERT [DocmahPageTypes] ([Id], [Name], [Description])
	VALUES (2, 'DocumentationPage', 'Help page that lives in application documentation only.')
END

IF NOT EXISTS (SELECT * FROM [dbo].[DocmahConfiguration] WHERE [Name] = 'DatabaseSchemaVersion') BEGIN
	INSERT [DocmahConfiguration]
	VALUES ('DatabaseSchemaVersion', 1)
END

IF NOT EXISTS (SELECT * FROM [dbo].[DocmahConfiguration] WHERE [Name] = 'DatabaseHelpVersion') BEGIN
	INSERT [DocmahConfiguration]
	VALUES ('DatabaseHelpVersion', 0)
END