using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DocMAH.UnitTests.Data.Sql
{
	[TestFixture]
	public class SqlDocumentationPageRepositoryTestFixture : BaseSqlRepositoryTestFixture
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
			var newPage = Models.CreateDocumentationPage();
			Assert.AreEqual(0, newPage.Id, "The page id should not be set until after data layer Page_Create method is called.");

			DocumentationPageRepository.Create(newPage);
			Assert.AreNotEqual(0, newPage.Id, "The page id should have been set by the data layer.");

			var oldTitle = newPage.Title;
			newPage.Title = "New Page Title";
			DocumentationPageRepository.Update(newPage);

			var existingPage = DocumentationPageRepository.Read(newPage.Id);
			Assert.IsNotNull(existingPage, "The page should exist in the database.");
			Assert.AreNotEqual(oldTitle, existingPage.Title, "The page's title should have been updated.");
			Assert.AreEqual(newPage.Content, existingPage.Content, "The rest of the page instances' contents should be the same.");

			DocumentationPageRepository.Delete(newPage.Id);

			var deletedPage = DocumentationPageRepository.Read(newPage.Id);
			Assert.That(deletedPage, Is.Null, "The page should have been deleted from the database.");
		}

		[Test]
		[Description("Empty list should not cause method to throw SqlException.")]
		public void DeleteExcept_EmptyList()
		{
			// Arrange
			
			// Act
			DocumentationPageRepository.DeleteExcept(new List<int>());

			// Assert
			// Above method call should not throw a SqlException.

		}

		[Test]
		[Description("Deletes pages and page URLs no included in the parameter list.")]
		public void DeleteExcept_Success()
		{
			// Arrange
			var keptPage = Models.CreateDocumentationPage();
			DocumentationPageRepository.Create(keptPage);
			var deletedPage = Models.CreateDocumentationPage();
			DocumentationPageRepository.Create(deletedPage);
			var anotherDeletedPage = Models.CreateDocumentationPage();
			DocumentationPageRepository.Create(anotherDeletedPage);

			// Act
			DocumentationPageRepository.DeleteExcept(new List<int> { keptPage.Id });

			// Assert
			var pages = DocumentationPageRepository.ReadAll().ToList();
			Assert.That(pages.Count, Is.EqualTo(1), "Only one page should be left in the repository.");
			Assert.That(pages[0].Id, Is.EqualTo(keptPage.Id), "The kept page should remain because its id was in the list.");
		}

		[Test]
		[Description("Updates an existing page in the data store.")]
		public void Import_ExistingPage()
		{
			// Arrange
			var page = Models.CreateDocumentationPage();
			DocumentationPageRepository.Create(page);

			page.Title = "Imported existing page title.";

			// Act
			DocumentationPageRepository.Import(page);
			var result = DocumentationPageRepository.Read(page.Id);

			// Assert
			Assert.That(result, Is.Not.Null, "The page should still exist in the repository.");
			Assert.That(result.Title, Is.EqualTo(page.Title), "The title should have been updated on import.");
		}

		[Test]
		[Description("Imports a new page into the data store.")]
		public void Import_NewPage()
		{
			// Arrange
			var page = Models.CreateDocumentationPage();
			page.Id = 10573;

			// Act
			DocumentationPageRepository.Import(page);
			var result = DocumentationPageRepository.Read(page.Id);

			// Assert
			Assert.That(result, Is.Not.Null, "The page should have been created with the supplied id.");
			Assert.That(result.Title, Is.EqualTo(page.Title), "The page Title should have been added to the data store.");
		}

		[Test]
		[Description("Reads pages by parent id.")]
		public void ReadByParentId_Success()
		{
			// Arrange
			var parentPage = Models.CreateDocumentationPage();
			DocumentationPageRepository.Create(parentPage);
			var childPage1 = Models.CreateDocumentationPage(parentPageId: parentPage.Id);
			DocumentationPageRepository.Create(childPage1);
			var childPage2 = Models.CreateDocumentationPage(parentPageId: parentPage.Id);
			DocumentationPageRepository.Create(childPage2);
			var noisePage = Models.CreateDocumentationPage();
			DocumentationPageRepository.Create(noisePage);

			// Act
			var results = DocumentationPageRepository.ReadByParentId(parentPage.Id);

			// Assert
			Assert.AreEqual(2, results.Count(), "Two child pages exist and should be returned.");
			Assert.IsNotNull(results.Where(p => p.Id == childPage1.Id).FirstOrDefault(), "The results should contain childPage1.");
			var childPage2Result = results.Where(p => p.Id == childPage2.Id).FirstOrDefault();
			Assert.IsNotNull(childPage2Result, "The results should contain childPage2.");
		}

		[Test]
		[Description("Reads all table of contents as page models.")]
		public void ReadTableOfContents_All()
		{
			// Arrange
			var root_1 = Models.CreateDocumentationPage(order: 1);
			DocumentationPageRepository.Create(root_1);
			var child_1_1 = Models.CreateDocumentationPage(parentPageId: root_1.Id, order: 1);
			DocumentationPageRepository.Create(child_1_1);
			var child_1_1_1 = Models.CreateDocumentationPage(parentPageId: child_1_1.Id, order: 1);
			DocumentationPageRepository.Create(child_1_1_1);
			var child_1_2 = Models.CreateDocumentationPage(parentPageId: root_1.Id, order: 2);
			DocumentationPageRepository.Create(child_1_2);
			var root_2 = Models.CreateDocumentationPage(order: 2);
			DocumentationPageRepository.Create(root_2);
			var child_2_1 = Models.CreateDocumentationPage(parentPageId: root_2.Id, order: 1);
			DocumentationPageRepository.Create(child_2_1);

			// Act
			var toc = DocumentationPageRepository.ReadTableOfContents(true);

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
			var root = Models.CreateDocumentationPage(order: 1, isHidden: false);
			DocumentationPageRepository.Create(root);
			var child = Models.CreateDocumentationPage(order: 1, parentPageId: root.Id, isHidden: true);
			DocumentationPageRepository.Create(child);
			var grandchild = Models.CreateDocumentationPage(order: 1, parentPageId: child.Id, isHidden: false);
			DocumentationPageRepository.Create(grandchild);

			// Act
			var toc = DocumentationPageRepository.ReadTableOfContents(false);

			// Assert
			Assert.AreEqual(1, toc.Count(), "Only one page should be returned because all other pages are hidden or beneath hidden pages.");
			Assert.AreEqual(root.Id, toc[0].Id, "The root page should be the only page included in the table of contents.");
		}

		#endregion
	}
}
