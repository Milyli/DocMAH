﻿using System;
using System.Configuration;
using System.Data.SqlClient;
using NUnit.Framework;
using System.Data;
using DocMAH.Data;
using Moq;
using System.Web;
using System.Linq;
using System.Collections.Generic;
using DocMAH.Models;
using DocMAH.Data.Sql;

namespace DocMAH.UnitTests.Data.Sql
{
	[TestFixture]
	public class SqlDataStoreTestFixture
	{
		#region Private Methods
		
		private int CountPages()
		{
			var pageCount = 0;
			foreach (var page in _database.Page_ReadAll())
				pageCount++;
			return pageCount;
		}

		#endregion

		#region Private Fields

		private SqlDataStore _database;
		private ModelFactory _models;

		#endregion

		#region SetUp / TearDown

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			var dataStoreManager = new TestFixtureDataStoreManager();
			dataStoreManager.TestFixtureDataStoreSetUp();
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			var dataStoreManager = new TestFixtureDataStoreManager();
			dataStoreManager.TestFixtureDataStoreTearDown();
		}

		[SetUp]
		public void TestInitialize()
		{
			_database = new SqlDataStore();
			_models = new ModelFactory();
		}

		[TearDown]
		public void TestCleanup()
		{
			// Pages must be removed in reverse order because of self referencing foreign key.
			var pages = new List<Page>();
			foreach (var page in _database.Page_ReadAll())
				pages.Insert(0, page);
			foreach(var page in pages)
				_database.Page_Delete(page.Id);

			_models = null;
			_database = null;
		}

		#endregion

		#region Tests

		// I'm not normally a fan of testing multiple methods in a single test.
		// However, in this case I would otherwise need to write a bunch of
		// SQL just for the unit tests.
		// 
		// As I'm feeling lazy I'm not going to spend time on that right now.
		//
		// This does verify that the data access layer is internally
		// consistent, which is good enough for the time being.
		[Test]
		[Description("Tests page create, read, update and delete methods.")]
		public void Page_Crud()
		{
			Assert.AreEqual(0, CountPages(), "All tests should start with an empty database.");

			var newPage = _models.CreatePage();
			Assert.AreEqual(0, newPage.Id, "The page id should not be set until after data layer Page_Create method is called.");

			_database.Page_Create(newPage);
			Assert.AreNotEqual(0, newPage.Id, "The page id should have been set by the data layer.");
			Assert.AreEqual(1, CountPages(), "One page should now exist in the database.");

			var oldTitle = newPage.Title;
			newPage.Title = "New Page Title";
			_database.Page_Update(newPage);

			var existingPage = _database.Page_ReadById(newPage.Id);
			Assert.IsNotNull(existingPage, "The page should exist in the database.");
			Assert.AreNotEqual(oldTitle, existingPage.Title, "The page's title should have been updated.");
			Assert.AreEqual(newPage.Content, existingPage.Content, "The rest of the page instances' contents should be the same.");

			_database.Page_Delete(newPage.Id);
			Assert.AreEqual(0, CountPages(), "No pages should exist after the page is deleted.");
		}

		[Test]
		[Description("Reads pages by parent id.")]
		public void Page_ReadByParentId()
		{
			// Arrange
			var parentPage = _models.CreatePage();
			_database.Page_Create(parentPage);
			var childPage1 = _models.CreatePage(parentPage.Id);
			_database.Page_Create(childPage1);
			var childPage2 = _models.CreatePage(parentPage.Id);
			_database.Page_Create(childPage2);
			var noisePage = _models.CreatePage();
			_database.Page_Create(noisePage);

			// Act
			var results = _database.Page_ReadByParentId(parentPage.Id);

			// Assert
			Assert.AreEqual(2, results.Count(), "Two child pages exist and should be returned.");
			Assert.IsNotNull(results.Where(p => p.Id == childPage1.Id).FirstOrDefault(), "The results should contain childPage1.");
			var childPage2Result = results.Where(p => p.Id == childPage2.Id).FirstOrDefault();
			Assert.IsNotNull(childPage2Result, "The results should contain childPage2.");
			Assert.That(childPage2.MatchUrls, Is.EqualTo(childPage2Result.MatchUrls), "The match URLs not being populated is causing issues in the page move algorithm. B36");
		}

		[Test]
		[Description("Reads pages by URLs.")]
		public void Page_ReadByUrl()
		{
			// Arrange
			var targetPage = _models.CreatePage(matchUrls: "/Controller/Target /Controller/Target/*");
			_database.Page_Create(targetPage);
			var noisePage = _models.CreatePage(matchUrls: "/Controller/Noise /Controller/Target/Exact");
			_database.Page_Create(noisePage);

			// Act
			var targetShortUrlMatch = _database.Page_ReadByUrl("/Controller/Target");
			var noiseExactMatch = _database.Page_ReadByUrl("/Controller/Target/Exact");
			var targetWildCardMatch = _database.Page_ReadByUrl("/Controller/Target/WildCard");

			// Assert
			Assert.IsNotNull(targetShortUrlMatch, "A value should be returned for the target short URL match.");
			Assert.AreEqual(targetPage.Id, targetShortUrlMatch.Id, "The target page should be returned for the target short URL match.");
			Assert.IsNotNull(noiseExactMatch, "A value should be returned for the noise exact match.");
			Assert.AreEqual(noisePage.Id, noiseExactMatch.Id, "The noise page should be returned for the noise exact match.");
			Assert.IsNotNull(targetWildCardMatch, "A value should be returned for the target wild card match.");
			Assert.AreEqual(targetPage.Id, targetWildCardMatch.Id, "The target page should be returned for the wild card match.");
		}

		[Test]
		[Description("Reads all table of contents as page models.")]
		public void Page_ReadTableOfContents_All()
		{
			// Arrange
			var root_1 = _models.CreatePage(order: 1);
			_database.Page_Create(root_1);
			var child_1_1 = _models.CreatePage(parentPageId: root_1.Id, order: 1);
			_database.Page_Create(child_1_1);
			var child_1_1_1 = _models.CreatePage(parentPageId: child_1_1.Id, order: 1);
			_database.Page_Create(child_1_1_1);
			var child_1_2 = _models.CreatePage(parentPageId: root_1.Id, order: 2);
			_database.Page_Create(child_1_2);
			var root_2 = _models.CreatePage(order: 2);
			_database.Page_Create(root_2);
			var child_2_1 = _models.CreatePage(parentPageId: root_2.Id, order: 1);
			_database.Page_Create(child_2_1);

			// Act
			var toc = _database.Page_ReadTableOfContents(true);

			// Assert
			Assert.AreEqual(6, toc.Count(), "Six pages should be returned in the table of contents.");
			Assert.IsNull(toc[0].Content, "Content should not be returned in the table of contents.");

			// Nodes are returned first by their depth starting with root nodes, then by their order, then by name.
			Assert.AreEqual(root_1.Id, toc[0].Id, "The first root node should be first in the table of contents.");
			Assert.AreEqual(root_2.Id, toc[1].Id, "The second root node should be returned second in the table of contents.");
			Assert.AreEqual(child_1_1.Id, toc[2].Id, "The first child node of the first root node should be returned third in the table of contents.");
			Assert.AreEqual(child_2_1.Id, toc[3].Id, "The child node of the second root node should be returned fourth in the table of contents.");
			Assert.AreEqual(child_1_2.Id, toc[4].Id, "The second child node of the first root node should be returned fifth in the table of contents.");
			Assert.AreEqual(child_1_1_1.Id, toc[5].Id, "The grandchild node should be returned sixth in the table of contents.");

		}

		[Test]
		[Description("Reads only visible table of contents page models.")]
		public void Page_ReadTableOfContents_Visible()
		{
			// Arrange
			var root = _models.CreatePage(order: 1, isHidden: false);
			_database.Page_Create(root);
			var child = _models.CreatePage(order: 1, parentPageId: root.Id, isHidden: true);
			_database.Page_Create(child);
			var grandchild = _models.CreatePage(order: 1, parentPageId: child.Id, isHidden: false);
			_database.Page_Create(grandchild);

			// Act
			var toc = _database.Page_ReadTableOfContents(false);

			// Assert
			Assert.AreEqual(1, toc.Count(), "Only one page should be returned because all other pages are hidden or beneath hidden pages.");
			Assert.AreEqual(root.Id, toc[0].Id, "The root page should be the only page included in the table of contents.");
		}

		#endregion
	}
}