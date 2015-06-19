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

namespace DocMAH.IntegrationTests
{
	[TestFixture]
	public class SqlDataStoreIntegrationTests
	{
		#region SetUp / TearDown

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			var dataStoreManager = new TestFixtureDataStoreManager();
			dataStoreManager.DeleteInstallFile();
			dataStoreManager.TestFixtureDataStoreSetUp();
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			var dataStoreManager = new TestFixtureDataStoreManager();
			dataStoreManager.DeleteInstallFile();
			dataStoreManager.TestFixtureDataStoreTearDown();
		}

		#endregion

		#region Tests

		[Test]
		[Description("Verify that Page ids remain consistent across content updates spanning missing updates.")]
		public void PageIdConsistencyAcrossSchemaUpdates()
		{
			// Create installation file.
			var cachePolicy = new Mock<HttpCachePolicyBase>();
			var httpContext = new Mock<HttpContextBase>();
			httpContext.SetupGet(c => c.Application[HelpContentManager.DocmahInitializedKey]).Returns(false);
			httpContext.Setup(c => c.Server.MapPath("~")).Returns(NUnit.Framework.TestContext.CurrentContext.TestDirectory);
			httpContext.SetupGet(c => c.Response.Cache).Returns(cachePolicy.Object);

			var pageRepository = new SqlPageRepository();
			var handler = new HttpHandler();

			var models = new ModelFactory();
			var firstPage = models.CreatePage();
			var deletedPage = models.CreatePage();
			var recreatedPage = models.CreatePage();
			var lastPage = models.CreatePage();

			pageRepository.Create(firstPage);
			handler.ProcessWrappedRequest(httpContext.Object);
			pageRepository.Create(deletedPage);
			pageRepository.Create(recreatedPage);
			pageRepository.Create(lastPage);
			pageRepository.Delete(recreatedPage.Id);
			pageRepository.Create(recreatedPage);
			pageRepository.Delete(deletedPage.Id);
			handler.ProcessWrappedRequest(httpContext.Object);

			// Reset data store and exercise startup file.
			var dataStoreManager = new TestFixtureDataStoreManager();
			dataStoreManager.TestFixtureDataStoreTearDown();
			dataStoreManager.TestFixtureDataStoreSetUp();

			// Validate model ids.
			var newFirstPage = pageRepository.Read(firstPage.Id);
			var deletedPageResult= pageRepository.Read(deletedPage.Id);
			Assert.That(deletedPageResult, Is.Null, "The data layer should return null for non-existant pages.");

			var newRecreatedPage = pageRepository.Read(recreatedPage.Id);
			var newLastPage = pageRepository.Read(lastPage.Id);

			Assert.That(newFirstPage, Is.Not.Null, "First page should still exist.");
			Assert.That(newFirstPage.Title, Is.EqualTo(firstPage.Title), "Old first page title should match new page with its id.");
			Assert.That(newRecreatedPage, Is.Not.Null, "Recreated page should still exist.");
			Assert.That(newRecreatedPage.Title, Is.EqualTo(recreatedPage.Title), "Old recreated page title should match new recreated page title.");
			Assert.That(newLastPage, Is.Not.Null, "Last page should still exist.");
			Assert.That(newLastPage.Title, Is.EqualTo(lastPage.Title), "Old last page title should match new last page title.");
		}

		[Test]
		[Category("Bug Reproduction")]
		[Description("Recreates bug where page URLs are deleted when pages are reordered.")]
		public void B36_PageUrlsDeletedOnPageReorder()
		{
			// Arrange
			var pageRepository = new SqlPageRepository();

			var childMatchUrl = "/Pages/Child";
			var movedMatchUrl = "/Pages/Moved";

			var models = new ModelFactory();
			var parentPage = models.CreatePage();
			pageRepository.Create(parentPage);
			var childPage = models.CreatePage(parentPageId: parentPage.Id, matchUrls: childMatchUrl);
			pageRepository.Create(childPage);
			var pageToMove = models.CreatePage(matchUrls: movedMatchUrl);
			pageRepository.Create(pageToMove);
			
			var moveRequestModel = new MoveTocRequest()
			{
				NewParentId = parentPage.Id,
				NewPosition = 0,
				PageId = pageToMove.Id
			};
			var serializer = new JavaScriptSerializer();
			var moveRequestData = serializer.Serialize(moveRequestModel);
			var moveRequestStream = new MemoryStream(Encoding.UTF8.GetBytes(moveRequestData));

			var httpContext = new Mock<HttpContextBase>();
			httpContext.SetupGet(c => c.Request.InputStream).Returns(moveRequestStream);

			var handler = new HttpHandler();

			// Act
			handler.ProcessWrappedRequest(httpContext.Object);

			// Assert
			var movedPage = pageRepository.Read(pageToMove.Id);
			var existingChildPage = pageRepository.Read(childPage.Id);

			Assert.That(existingChildPage.MatchUrls, Is.EqualTo(childMatchUrl), "The original child page's match URL should remain the same."); // This is the bug.
			Assert.That(movedPage.MatchUrls, Is.EqualTo(movedMatchUrl), "The moved page's match URL should remain the same.");					// This should not change.
		}

		#endregion
	}
}
