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

namespace DocMAH.IntegrationTests
{
	[TestFixture]
	public class SqlContentInstallationUnitTests
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
		public void PageIdConsistencyAcrossOccasionalUpdates()
		{
			// Create installation file.
			var cachePolicy = new Mock<HttpCachePolicyBase>();
			var context = new Mock<HttpContextBase>();
			context.SetupGet(c => c.Application["DMH.Initialized"]).Returns(false);
			context.Setup(c => c.Server.MapPath("~")).Returns(NUnit.Framework.TestContext.CurrentContext.TestDirectory);
			context.SetupGet(c => c.Response.Cache).Returns(cachePolicy.Object);

			// TODO: switch to using data store factory when one is available.
			var sqlAccess = new SqlDataStore();
			var requestProcessor = new RequestProcessor();

			var models = new ModelFactory();
			var firstPage = models.CreatePage();
			var deletedPage = models.CreatePage();
			var recreatedPage = models.CreatePage();
			var lastPage = models.CreatePage();

			sqlAccess.Page_Create(firstPage);
			requestProcessor.ProcessGenerateInstallScriptRequest(context.Object);
			sqlAccess.Page_Create(deletedPage);
			sqlAccess.Page_Create(recreatedPage);
			sqlAccess.Page_Create(lastPage);
			sqlAccess.Page_Delete(recreatedPage.Id);
			sqlAccess.Page_Create(recreatedPage);
			sqlAccess.Page_Delete(deletedPage.Id);
			requestProcessor.ProcessGenerateInstallScriptRequest(context.Object);

			// Reset data store and exercise startup file.
			var dataStoreManager = new TestFixtureDataStoreManager();
			dataStoreManager.TestFixtureDataStoreTearDown();
			dataStoreManager.TestFixtureDataStoreSetUp();

			// Validate model ids.
			var newFirstPage = sqlAccess.Page_ReadById(firstPage.Id);
			try
			{
				sqlAccess.Page_ReadById(deletedPage.Id);
				Assert.Fail("A null reference should be thrown by reading an id that does not exist.");
			}
			catch (NullReferenceException) { }
			var newRecreatedPage = sqlAccess.Page_ReadById(recreatedPage.Id);
			var newLastPage = sqlAccess.Page_ReadById(lastPage.Id);

			Assert.That(newFirstPage, Is.Not.Null, "First page should still exist.");
			Assert.That(newFirstPage.Title, Is.EqualTo(firstPage.Title), "Old first page title should match new page with its id.");
			Assert.That(newRecreatedPage, Is.Not.Null, "Recreated page should still exist.");
			Assert.That(newRecreatedPage.Title, Is.EqualTo(recreatedPage.Title), "Old recreated page title should match new recreated page title.");
			Assert.That(newLastPage, Is.Not.Null, "Last page should still exist.");
			Assert.That(newLastPage.Title, Is.EqualTo(lastPage.Title), "Old last page title should match new last page title.");
		}

		#endregion
	}
}
