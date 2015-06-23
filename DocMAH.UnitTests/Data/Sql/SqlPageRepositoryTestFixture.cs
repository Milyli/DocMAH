using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DocMAH.UnitTests.Data.Sql
{
	[TestFixture]
	public class SqlPageRepositoryTestFixture : BaseSqlRepositoryTestFixture
	{
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
		public void Crud_Success()
		{
			var newPage = Models.CreatePage();
			Assert.AreEqual(0, newPage.Id, "The page id should not be set until after data layer Page_Create method is called.");

			PageRepository.Create(newPage);
			Assert.AreNotEqual(0, newPage.Id, "The page id should have been set by the data layer.");

			var oldTitle = newPage.Title;
			newPage.Title = "New Page Title";
			PageRepository.Update(newPage);

			var existingPage = PageRepository.Read(newPage.Id);
			Assert.IsNotNull(existingPage, "The page should exist in the database.");
			Assert.AreNotEqual(oldTitle, existingPage.Title, "The page's title should have been updated.");
			Assert.AreEqual(newPage.Content, existingPage.Content, "The rest of the page instances' contents should be the same.");

			PageRepository.Delete(newPage.Id);

			var deletedPage = PageRepository.Read(newPage.Id);
			Assert.That(deletedPage, Is.Null, "The page should have been deleted from the database.");
		}

		[Test]
		[Description("Empty list should not cause method to throw SqlException.")]
		public void DeleteExcept_EmptyList()
		{
			// Arrange
			
			// Act
			PageRepository.DeleteExcept(new List<int>());

			// Assert
			// Above method call should not throw a SqlException.

		}

		[Test]
		[Description("Deletes pages and page URLs no included in the parameter list.")]
		public void DeleteExcept_Success()
		{
			// Arrange
			var keptPage = Models.CreatePage(matchUrls: "/Pages/KeptPage1 /Pages/KeptPage2");
			PageRepository.Create(keptPage);
			var deletedPage = Models.CreatePage(matchUrls: "/Pages/DeletedPage1 /Pages/DeletedPage2");
			PageRepository.Create(deletedPage);
			var anotherDeletedPage = Models.CreatePage(matchUrls: "/Pages/AnotherDeletedPage1 /Pages/AnotherDeletedPage2");
			PageRepository.Create(anotherDeletedPage);

			// Act
			PageRepository.DeleteExcept(new List<int> { keptPage.Id });

			// Assert
			var pages = PageRepository.ReadAll().ToList();
			Assert.That(pages.Count, Is.EqualTo(1), "Only one page should be left in the repository.");
			Assert.That(pages[0].Id, Is.EqualTo(keptPage.Id), "The kept page should remain because its id was in the list.");
		}

		[Test]
		[Description("Updates an existing page in the data store.")]
		public void Import_ExistingPage()
		{
			// Arrange
			var page = Models.CreatePage();
			PageRepository.Create(page);

			page.Title = "Imported existing page title.";

			// Act
			PageRepository.Import(page);
			var result = PageRepository.Read(page.Id);

			// Assert
			Assert.That(result, Is.Not.Null, "The page should still exist in the repository.");
			Assert.That(result.Title, Is.EqualTo(page.Title), "The title should have been updated on import.");
		}

		[Test]
		[Description("Imports a new page into the data store.")]
		public void Import_NewPage()
		{
			// Arrange
			var page = Models.CreatePage();
			page.Id = 10573;

			// Act
			PageRepository.Import(page);
			var result = PageRepository.Read(page.Id);

			// Assert
			Assert.That(result, Is.Not.Null, "The page should have been created with the supplied id.");
			Assert.That(result.Title, Is.EqualTo(page.Title), "The page Title should have been added to the data store.");
		}

		[Test]
		[Description("Reads pages by parent id.")]
		public void ReadByParentId_Success()
		{
			// Arrange
			var parentPage = Models.CreatePage();
			PageRepository.Create(parentPage);
			var childPage1 = Models.CreatePage(parentPage.Id);
			PageRepository.Create(childPage1);
			var childPage2 = Models.CreatePage(parentPage.Id);
			PageRepository.Create(childPage2);
			var noisePage = Models.CreatePage();
			PageRepository.Create(noisePage);

			// Act
			var results = PageRepository.ReadByParentId(parentPage.Id);

			// Assert
			Assert.AreEqual(2, results.Count(), "Two child pages exist and should be returned.");
			Assert.IsNotNull(results.Where(p => p.Id == childPage1.Id).FirstOrDefault(), "The results should contain childPage1.");
			var childPage2Result = results.Where(p => p.Id == childPage2.Id).FirstOrDefault();
			Assert.IsNotNull(childPage2Result, "The results should contain childPage2.");
			Assert.That(childPage2.MatchUrls, Is.EqualTo(childPage2Result.MatchUrls), "The match URLs not being populated is causing issues in the page move algorithm. B36");
		}

		[Test]
		[Description("Reads pages by URLs.")]
		public void ReadByUrl_Success()
		{
			// Arrange
			var targetPage = Models.CreatePage(matchUrls: "/Controller/Target /Controller/Target/*");
			PageRepository.Create(targetPage);
			var noisePage = Models.CreatePage(matchUrls: "/Controller/Noise /Controller/Target/Exact");
			PageRepository.Create(noisePage);

			// Act
			var targetShortUrlMatch = PageRepository.ReadByUrl("/Controller/Target");
			var noiseExactMatch = PageRepository.ReadByUrl("/Controller/Target/Exact");
			var targetWildCardMatch = PageRepository.ReadByUrl("/Controller/Target/WildCard");

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
		public void ReadTableOfContents_All()
		{
			// Arrange
			var root_1 = Models.CreatePage(order: 1);
			PageRepository.Create(root_1);
			var child_1_1 = Models.CreatePage(parentPageId: root_1.Id, order: 1);
			PageRepository.Create(child_1_1);
			var child_1_1_1 = Models.CreatePage(parentPageId: child_1_1.Id, order: 1);
			PageRepository.Create(child_1_1_1);
			var child_1_2 = Models.CreatePage(parentPageId: root_1.Id, order: 2);
			PageRepository.Create(child_1_2);
			var root_2 = Models.CreatePage(order: 2);
			PageRepository.Create(root_2);
			var child_2_1 = Models.CreatePage(parentPageId: root_2.Id, order: 1);
			PageRepository.Create(child_2_1);

			// Act
			var toc = PageRepository.ReadTableOfContents(true);

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
		public void ReadTableOfContents_Visible()
		{
			// Arrange
			var root = Models.CreatePage(order: 1, isHidden: false);
			PageRepository.Create(root);
			var child = Models.CreatePage(order: 1, parentPageId: root.Id, isHidden: true);
			PageRepository.Create(child);
			var grandchild = Models.CreatePage(order: 1, parentPageId: child.Id, isHidden: false);
			PageRepository.Create(grandchild);

			// Act
			var toc = PageRepository.ReadTableOfContents(false);

			// Assert
			Assert.AreEqual(1, toc.Count(), "Only one page should be returned because all other pages are hidden or beneath hidden pages.");
			Assert.AreEqual(root.Id, toc[0].Id, "The root page should be the only page included in the table of contents.");
		}

		#endregion
	}
}
