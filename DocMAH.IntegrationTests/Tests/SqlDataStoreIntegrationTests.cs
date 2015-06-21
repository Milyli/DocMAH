﻿using System;
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
using DocMAH.Web.Authorization;
using DocMAH.Web.Requests;

namespace DocMAH.IntegrationTests.Tests
{
	[TestFixture]
	public class SqlDataStoreIntegrationTests : BaseIntegrationTestFixture
	{

		#region Tests

		[Test]
		[Description("Verify that Page ids remain consistent across content updates spanning missing updates.")]
		public void PageIdConsistencyAcrossSchemaUpdates()
		{
			// Create installation file.
			HttpContext.SetMapPath("~", NUnit.Framework.TestContext.CurrentContext.TestDirectory);
			HttpContext.AddApplicationState(HelpContentManager.DocmahInitializedKey, false);
			HttpContext.SetRequestParameter("m", RequestTypes.GenerateInstallScript);
			Container.Register<HttpContextBase>(c => HttpContext.Object);

			var pageRepository = Container.Resolve<IPageRepository>();
			var handler = new HttpHandler(Container);

			var models = new ModelFactory();
			var firstPage = models.CreatePage();
			var deletedPage = models.CreatePage();
			var recreatedPage = models.CreatePage();
			var lastPage = models.CreatePage();

			pageRepository.Create(firstPage);
			handler.ProcessWrappedRequest(HttpContext.Object);
			pageRepository.Create(deletedPage);
			pageRepository.Create(recreatedPage);
			pageRepository.Create(lastPage);
			pageRepository.Delete(recreatedPage.Id);
			pageRepository.Create(recreatedPage);
			pageRepository.Delete(deletedPage.Id);
			HttpContext.SetRequestContent(string.Empty); // Resets stream for next read.
			handler.ProcessWrappedRequest(HttpContext.Object);

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

		#endregion
	}
}
