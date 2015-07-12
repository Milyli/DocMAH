using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using DocMAH.Data;
using Moq;
using NUnit.Framework;
using System.Reflection;
using DocMAH.UnitTests;
using DocMAH.Web;
using DocMAH.Models;
using System.Web.Script.Serialization;
using DocMAH.Data.Sql;
using DocMAH.Dependencies;

using DocMAH.Web.Requests;
using DocMAH.Content;
using DocMAH.Adapters;

namespace DocMAH.IntegrationTests.Tests
{
	[TestFixture]
	public class ContentDeploymentIntegrationTests : BaseIntegrationTestFixture
	{
		#region Private Fields

		private IDocumentationPageRepository _documentationPageRepository;
		private IFirstTimeHelpRepository _firstTimeHelpRepository;

		#endregion

		#region SetUp/TearDown

		[SetUp]
		public void SetUp()
		{

			_documentationPageRepository = Container.Resolve<IDocumentationPageRepository>();
			_firstTimeHelpRepository = Container.Resolve<IFirstTimeHelpRepository>();
		}
		
		#endregion

		#region Tests

		[Test]
		[Description("Verify that page data is consistent when content file upgrades are skipped.")]
		public void InstallFromSkippedFile()
		{
			var dataStoreManager = new TestFixtureDataStoreManager();

			var firstPage = Models.CreateDocumentationPage();
			_documentationPageRepository.Create(firstPage);

			// Generate content file.
			var contentFileName = Path.GetTempFileName();
			dataStoreManager.ExportContent(contentFileName);


			// Change content and recreate installation file.
			var deletedPage = Models.CreateDocumentationPage();
			var recreatedPage = Models.CreateDocumentationPage();
			var lastPage = Models.CreateDocumentationPage();

			_documentationPageRepository.Create(deletedPage);
			_documentationPageRepository.Create(recreatedPage);
			_documentationPageRepository.Create(lastPage);
			_documentationPageRepository.Delete(recreatedPage.Id);
			_documentationPageRepository.Create(recreatedPage);
			_documentationPageRepository.Delete(deletedPage.Id);

			// Regenerate content file.
			dataStoreManager.ExportContent(contentFileName);

			// Reset data store and exercise startup file.
			dataStoreManager.DeleteDataStore();
			dataStoreManager.CreateDataStore();
			dataStoreManager.ImportContent(contentFileName);

			// Validate model ids.
			var newFirstPage = _documentationPageRepository.Read(firstPage.Id);
			var deletedPageResult = _documentationPageRepository.Read(deletedPage.Id);
			Assert.That(deletedPageResult, Is.Null, "The data layer should return null for non-existant pages.");

			var newRecreatedPage = _documentationPageRepository.Read(recreatedPage.Id);
			var newLastPage = _documentationPageRepository.Read(lastPage.Id);

			Assert.That(newFirstPage, Is.Not.Null, "First page should still exist.");
			Assert.That(newFirstPage.Title, Is.EqualTo(firstPage.Title), "Old first page title should match new page with its id.");
			Assert.That(newRecreatedPage, Is.Not.Null, "Recreated page should still exist.");
			Assert.That(newRecreatedPage.Title, Is.EqualTo(recreatedPage.Title), "Old recreated page title should match new recreated page title.");
			Assert.That(newLastPage, Is.Not.Null, "Last page should still exist.");
			Assert.That(newLastPage.Title, Is.EqualTo(lastPage.Title), "Old last page title should match new last page title.");
		}

		[Test]
		[Description("Verify that help URLs that are reused on different help entries upgrade correctly.")]
		public void UpgradeReusedHelpUrls()
		{
			var dataStoreManager = new TestFixtureDataStoreManager();
			var reusedUrl = "/Duplicate/URL";

			// Create starting content data.
			var firstHelp = Models.CreateFirstTimeHelp(matchUrls: "/ /Home /Home/*");
			_firstTimeHelpRepository.Create(firstHelp);
			var originalUrlHelp = Models.CreateFirstTimeHelp(matchUrls: reusedUrl);
			_firstTimeHelpRepository.Create(originalUrlHelp);

			// Generate starting content file.
			var startingContentFileName = Path.GetTempFileName();
			dataStoreManager.ExportContent(startingContentFileName);

			// Create upgrade content data.
			var noiseHelp = Models.CreateFirstTimeHelp();	// Need to bump table id so reusedUrlPage receives a different id.
			_firstTimeHelpRepository.Create(noiseHelp);
			_firstTimeHelpRepository.Delete(originalUrlHelp.Id);
			var reusedUrlHelp = Models.CreateFirstTimeHelp(matchUrls: reusedUrl);
			_firstTimeHelpRepository.Create(reusedUrlHelp);

			// Generate upgrade content file.
			var upgradeContentFileName = Path.GetTempFileName();
			dataStoreManager.ExportContent(upgradeContentFileName);

			// Reset the data store to the starting data.
			dataStoreManager.DeleteDataStore();
			dataStoreManager.CreateDataStore();
			dataStoreManager.ImportContent(startingContentFileName);

			// Try upgrading content using upgrade script.
			dataStoreManager.ImportContent(upgradeContentFileName);

			// Read data from updated data store.
			var newFirstHelp = _firstTimeHelpRepository.Read(firstHelp.Id);
			var newOriginalUrlHelp = _firstTimeHelpRepository.Read(originalUrlHelp.Id);
			var newNoiseHelp = _firstTimeHelpRepository.Read(noiseHelp.Id);
			var newReusedUrlHelp = _firstTimeHelpRepository.Read(reusedUrlHelp.Id);

			// Validate data.
			Assert.That(originalUrlHelp.Id, Is.LessThan(reusedUrlHelp.Id), "The second help must have a higher id.");
			Assert.That(newFirstHelp, Is.Not.Null);
			Assert.That(newNoiseHelp, Is.Not.Null);
			Assert.That(newReusedUrlHelp, Is.Not.Null);
			Assert.That(newOriginalUrlHelp, Is.Null);

		}

		#endregion
	}
}
