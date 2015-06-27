SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON

-- Rename DatabaseHelpVersion setting to HelpContentVersion
IF EXISTS (SELECT * FROM [dbo].[DocmahConfiguration] WHERE [Name] = 'DatabaseHelpVersion') BEGIN
	UPDATE DocmahConfiguration
	SET [Name] = 'HelpContentVersion'
	WHERE [Name] = 'DatabaseHelpVersion'
END

/***** BEGIN SPLIT DocmahPages TABLE INTO DocmahFirstTimeHelp AND DocmahDocumentationPages *****/

-- Create new tables.
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DocmahFirstTimeHelp]') AND type in (N'U')) BEGIN
	CREATE TABLE [dbo].[DocmahFirstTimeHelp](
		[Id] [int] NOT NULL,
		[SourceUrl] [nvarchar](256) NULL,
		[Title] [nvarchar](50) NOT NULL,
		[Content] [nvarchar](max) NOT NULL,
		[VerticalOffset] [int] NULL,
		[HorizontalOffset] [int] NULL,
		[OffsetElementId] [nvarchar](50) NULL,
	 CONSTRAINT [PK_DocmahFirstTimeHelp] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF)
	)

	INSERT INTO [dbo].[DocmahFirstTimeHelp]
	SELECT [Id], [SourceUrl], [Title], [Content], [VerticalOffset], [HorizontalOffset], [OffsetElementId]
	FROM [dbo].[DocmahPages]
	WHERE [PageTypeId] = 1 -- First Time Help

END

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DocmahDocumentationPages]') AND type in (N'U')) BEGIN
	CREATE TABLE [dbo].[DocmahDocumentationPages](
		[Id] [int] NOT NULL,
		[ParentPageId] [int] NULL,
		[Order] [int] NOT NULL,
		[Title] [nvarchar](50) NOT NULL,
		[Content] [nvarchar](max) NOT NULL,
		[IsHidden] [bit],
	 CONSTRAINT [PK_DocmahDocumentationPages] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF)
	)

	-- It is possible that someone created a first time help page with child documentation pages.
	-- Promote such documentation pages to the next parent documentation page up the chain as its last child.
	DECLARE 
		@PagesCursor CURSOR,
		@id INT,
		@parentPageId INT,
		@order INT,
		@title NVARCHAR(50),
		@isHidden BIT,
		@content NVARCHAR(MAX),
		@pageTypeId INT,
		@parentPageTypeId INT

	SET @PagesCursor = CURSOR FOR
	WITH Pages_CTE
	AS (
		SELECT 
			1 AS [Level],
			[Id],[ParentPageId],[Order],[Title],[IsHidden],[Content],[PageTypeId],NULL AS 'ParentPageTypeId'
		FROM [dbo].[DocmahPages]
		WHERE ParentPageId IS NULL

		UNION ALL

		SELECT 
			[Parent].[Level] + 1 AS [Level],
			[Page].[Id],[Page].[ParentPageId],[Page].[Order],[Page].[Title],[Page].[IsHidden],[Page].[Content],[Page].[PageTypeId],[Parent].[PageTypeId] AS 'ParentPageTypeId'
		FROM [dbo].[DocmahPages] AS [Page]
			JOIN Pages_CTE AS Parent ON [Page].[ParentPageId] = Parent.[Id]
	)
	SELECT [Id],[ParentPageId],[Order],[Title],[IsHidden],[Content],[PageTypeId],[ParentPageTypeId]
	FROM Pages_CTE
	ORDER BY [Level],[Order],[Title]

	OPEN @PagesCursor

	FETCH NEXT FROM @PagesCursor 
	INTO @id, @parentPageId, @order, @title, @isHidden, @content, @pageTypeId, @parentPageTypeId

	WHILE @@FETCH_STATUS = 0
	BEGIN
		IF @pageTypeId = 2 BEGIN -- Only insert documentation pages into new DocmahDocumenationPages table
		
			WHILE @parentPageTypeId = 1 BEGIN -- If the parent page is first time help, find the first time help's parent and use that for this documentation page.
				SELECT 
					@parentPageId = Ancestor.[Id],
					@parentPageTypeId = Ancestor.[PageTypeId]
				FROM DocmahPages AS Parent
					JOIN DocmahPages AS Ancestor ON Parent.ParentPageId = Ancestor.Id
				WHERE Parent.Id = @parentPageId
			END -- While @parentPageTypeId = 1

			-- Pages are selected by their level then order so 
			-- we can calculate new, zero based order by couting the number of documetnation pages
			--for the parent page already inserted into the new table.
			SELECT @order = COUNT(1)
			FROM [dbo].[DocmahDocumentationPages]
			WHERE ISNULL([ParentPageId], -1) = ISNULL(@parentPageId, -1)

			INSERT INTO [dbo].[DocmahDocumentationPages]
			VALUES (@id, @parentPageId, @order, @title, @content, @isHidden)

		END -- IF @pageTypeId = 2
		
		FETCH NEXT FROM @PagesCursor 
		INTO @id, @parentPageId, @order, @title, @isHidden, @content, @pageTypeId, @parentPageTypeId

	END -- @@FETCH_STATUS = 0

END -- IF NOT EXISTS DocmahDocumentationPages


-- Create new indexes.
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[DocmahDocumentationPages]') AND name = N'IX_DocmahDocumentationPages_ParentPageId_Order') BEGIN
	CREATE NONCLUSTERED INDEX [IX_DocmahDocumentationPages_ParentPageId_Order] ON [dbo].[DocmahDocumentationPages]
	(
		[ParentPageId] ASC,
		[Order] ASC
	)WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF)
END

-- Create new foreign keys.
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DocmahBullets_DocmahFirstTimeHelp]') AND parent_object_id = OBJECT_ID(N'[dbo].[DocmahBullets]')) BEGIN
	ALTER TABLE [dbo].[DocmahBullets]  WITH CHECK ADD  CONSTRAINT [FK_DocmahBullets_DocmahFirstTimeHelp] FOREIGN KEY([PageId])
	REFERENCES [dbo].[DocmahFirstTimeHelp] ([Id])
END

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DocmahBullets_DocmahFirstTimeHelp]') AND parent_object_id = OBJECT_ID(N'[dbo].[DocmahBullets]')) BEGIN
	ALTER TABLE [dbo].[DocmahBullets] CHECK CONSTRAINT [FK_DocmahBullets_DocmahFirstTimeHelp]
END

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DocmahDocumentationPages_DocmahDocumentationPages]') AND parent_object_id = OBJECT_ID(N'[dbo].[DocmahDocumentationPages]')) BEGIN
	ALTER TABLE [dbo].[DocmahDocumentationPages]  WITH CHECK ADD CONSTRAINT [FK_DocmahDocumentationPages_DocmahDocumentationPages] FOREIGN KEY([ParentPageId])
	REFERENCES [dbo].[DocmahDocumentationPages] ([Id])
END

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DocmahDocumentationPages_DocmahDocumentationPages]') AND parent_object_id = OBJECT_ID(N'[dbo].[DocmahDocumentationPages]')) BEGIN
	ALTER TABLE [dbo].[DocmahDocumentationPages] CHECK CONSTRAINT [FK_DocmahDocumentationPages_DocmahDocumentationPages]
END

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DocmahPageUrls_DocmahFirstTimeHelp]') AND parent_object_id = OBJECT_ID(N'[dbo].[DocmahPageUrls]')) BEGIN
	ALTER TABLE [dbo].[DocmahPageUrls]  WITH CHECK ADD CONSTRAINT [FK_DocmahPageUrls_DocmahFirstTimeHelp] FOREIGN KEY([PageId])
	REFERENCES [dbo].[DocmahFirstTimeHelp] ([Id])
END
	
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DocmahPageUrls_DocmahFirstTimeHelp]') AND parent_object_id = OBJECT_ID(N'[dbo].[DocmahPageUrls]')) BEGIN
	ALTER TABLE [dbo].[DocmahPageUrls] CHECK CONSTRAINT [FK_DocmahPageUrls_DocmahFirstTimeHelp]
END

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DocmahUserPageSettings_DocmahFirstTimeHelp]') AND parent_object_id = OBJECT_ID(N'[dbo].[DocmahUserPageSettings]')) BEGIN
	ALTER TABLE [dbo].[DocmahUserPageSettings]  WITH CHECK ADD CONSTRAINT [FK_DocmahUserPageSettings_DocmahFirstTimeHelp] FOREIGN KEY([PageId])
	REFERENCES [dbo].[DocmahFirstTimeHelp] ([Id])
END

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DocmahUserPageSettings_DocmahFirstTimeHelp]') AND parent_object_id = OBJECT_ID(N'[dbo].[DocmahUserPageSettings]')) BEGIN
	ALTER TABLE [dbo].[DocmahUserPageSettings] CHECK CONSTRAINT [FK_DocmahUserPageSettings_DocmahFirstTimeHelp]
END

-- Drop old foreign keys.
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DocmahBullets_DocmahPages]') AND parent_object_id = OBJECT_ID(N'[dbo].[DocmahBullets]')) BEGIN
	ALTER TABLE [dbo].[DocmahBullets] DROP CONSTRAINT [FK_DocmahBullets_DocmahPages]
END

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DocmahPages_DocmahPages]') AND parent_object_id = OBJECT_ID(N'[dbo].[DocmahPages]')) BEGIN
	ALTER TABLE [dbo].[DocmahPages] DROP CONSTRAINT [FK_DocmahPages_DocmahPages]
END

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DocmahPages_DocmahPageTypes]') AND parent_object_id = OBJECT_ID(N'[dbo].[DocmahPages]')) BEGIN
	ALTER TABLE [dbo].[DocmahPages]  DROP CONSTRAINT [FK_DocmahPages_DocmahPageTypes]
END

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DocmahPageUrls_DocmahPages]') AND parent_object_id = OBJECT_ID(N'[dbo].[DocmahPageUrls]')) BEGIN
	ALTER TABLE [dbo].[DocmahPageUrls]  DROP CONSTRAINT [FK_DocmahPageUrls_DocmahPages]
END
	
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_DocmahUserPageSettings_DocmahPages]') AND parent_object_id = OBJECT_ID(N'[dbo].[DocmahUserPageSettings]')) BEGIN
	ALTER TABLE [dbo].[DocmahUserPageSettings] DROP CONSTRAINT [FK_DocmahUserPageSettings_DocmahPages]
END

-- Drop old tables.
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DocmahPageTypes]') AND type in (N'U')) BEGIN
	DROP TABLE [dbo].[DocmahPageTypes]
END

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DocmahPages]') AND type in (N'U')) BEGIN
	DROP TABLE [dbo].[DocmahPages]
END
/***** END SPLIT DocmahPages TABLE INTO DocmahFirstTimeHelp AND DocmahDocumentationPages *****/

-- Update database schema version.
IF EXISTS (SELECT * FROM [dbo].[DocmahConfiguration] WHERE [Name] = 'DatabaseSchemaVersion') BEGIN
	UPDATE [dbo].[DocmahConfiguration] SET [Value] = 2 WHERE [Name] = 'DatabaseSchemaVersion'
END
ELSE BEGIN
	INSERT [DocmahConfiguration] VALUES ('DatabaseSchemaVersion', 2)
END
