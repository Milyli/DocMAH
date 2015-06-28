using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DocMAH.UnitTests.Data.Sql
{
	[TestFixture]
	public class SqlFirstTimeHelpRepositoryTestFixture : BaseSqlRepositoryTestFixture
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
			var newHelp = Models.CreateFirstTimeHelp();
			Assert.AreEqual(0, newHelp.Id, "The page id should not be set until after data layer Page_Create method is called.");

			FirstTimeHelpRepository.Create(newHelp);
			Assert.AreNotEqual(0, newHelp.Id, "The page id should have been set by the data layer.");

			var oldTitle = newHelp.Title;
			newHelp.Title = "New Page Title";
			FirstTimeHelpRepository.Update(newHelp);

			var existingPage = FirstTimeHelpRepository.Read(newHelp.Id);
			Assert.IsNotNull(existingPage, "The page should exist in the database.");
			Assert.AreNotEqual(oldTitle, existingPage.Title, "The page's title should have been updated.");
			Assert.AreEqual(newHelp.Content, existingPage.Content, "The rest of the page instances' contents should be the same.");

			FirstTimeHelpRepository.Delete(newHelp.Id);

			var deletedPage = FirstTimeHelpRepository.Read(newHelp.Id);
			Assert.That(deletedPage, Is.Null, "The page should have been deleted from the database.");
		}

		[Test]
		[Description("Empty list should not cause method to throw SqlException.")]
		public void DeleteExcept_EmptyList()
		{
			// Arrange

			// Act
			FirstTimeHelpRepository.DeleteExcept(new List<int>());

			// Assert
			// Above method call should not throw a SqlException.

		}

		[Test]
		[Description("Deletes pages and page URLs no included in the parameter list.")]
		public void DeleteExcept_Success()
		{
			// Arrange
			var keptHelp = Models.CreateFirstTimeHelp(matchUrls: "/Pages/KeptPage1 /Pages/KeptPage2");
			FirstTimeHelpRepository.Create(keptHelp);
			var deletedHelp = Models.CreateFirstTimeHelp(matchUrls: "/Pages/DeletedPage1 /Pages/DeletedPage2");
			FirstTimeHelpRepository.Create(deletedHelp);
			var anotherDeletedHelp = Models.CreateFirstTimeHelp(matchUrls: "/Pages/AnotherDeletedPage1 /Pages/AnotherDeletedPage2");
			FirstTimeHelpRepository.Create(anotherDeletedHelp);

			// Act
			FirstTimeHelpRepository.DeleteExcept(new List<int> { keptHelp.Id });

			// Assert
			var pages = FirstTimeHelpRepository.ReadAll().ToList();
			Assert.That(pages.Count, Is.EqualTo(1), "Only one page should be left in the repository.");
			Assert.That(pages[0].Id, Is.EqualTo(keptHelp.Id), "The kept page should remain because its id was in the list.");
		}

		[Test]
		[Description("Updates an existing page in the data store.")]
		public void Import_ExistingHelp()
		{
			// Arrange
			var help = Models.CreateFirstTimeHelp();
			FirstTimeHelpRepository.Create(help);

			help.Title = "Imported existing page title.";

			// Act
			FirstTimeHelpRepository.Import(help);
			var result = FirstTimeHelpRepository.Read(help.Id);

			// Assert
			Assert.That(result, Is.Not.Null, "The page should still exist in the repository.");
			Assert.That(result.Title, Is.EqualTo(help.Title), "The title should have been updated on import.");
		}

		[Test]
		[Description("Imports a new page into the data store.")]
		public void Import_NewHelp()
		{
			// Arrange
			var help = Models.CreateFirstTimeHelp();
			help.Id = 10573;

			// Act
			FirstTimeHelpRepository.Import(help);
			var result = FirstTimeHelpRepository.Read(help.Id);

			// Assert
			Assert.That(result, Is.Not.Null, "The page should have been created with the supplied id.");
			Assert.That(result.Title, Is.EqualTo(help.Title), "The page Title should have been added to the data store.");
		}

		[Test]
		[Description("Reads pages by URLs.")]
		public void ReadByUrl_Success()
		{
			// Arrange
			var targetHelp = Models.CreateFirstTimeHelp(matchUrls: "/Controller/Target /Controller/Target/*");
			FirstTimeHelpRepository.Create(targetHelp);
			var noiseHelp = Models.CreateFirstTimeHelp(matchUrls: "/Controller/Noise /Controller/Target/Exact");
			FirstTimeHelpRepository.Create(noiseHelp);

			// Act
			var targetShortUrlMatch = FirstTimeHelpRepository.ReadByUrl("/Controller/Target");
			var noiseExactMatch = FirstTimeHelpRepository.ReadByUrl("/Controller/Target/Exact");
			var targetWildCardMatch = FirstTimeHelpRepository.ReadByUrl("/Controller/Target/WildCard");

			// Assert
			Assert.IsNotNull(targetShortUrlMatch, "A value should be returned for the target short URL match.");
			Assert.AreEqual(targetHelp.Id, targetShortUrlMatch.Id, "The target page should be returned for the target short URL match.");
			Assert.IsNotNull(noiseExactMatch, "A value should be returned for the noise exact match.");
			Assert.AreEqual(noiseHelp.Id, noiseExactMatch.Id, "The noise page should be returned for the noise exact match.");
			Assert.IsNotNull(targetWildCardMatch, "A value should be returned for the target wild card match.");
			Assert.AreEqual(targetHelp.Id, targetWildCardMatch.Id, "The target page should be returned for the wild card match.");
		}
		


		#endregion
	}
}
