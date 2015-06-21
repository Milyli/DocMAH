using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using DocMAH.Configuration;
using DocMAH.Data;
using DocMAH.Data.Sql;
using DocMAH.Dependencies;
using Moq;
using NUnit.Framework;

namespace DocMAH.UnitTests
{
	public class TestFixtureDataStoreManager
	{
		#region Constructors

		public TestFixtureDataStoreManager()
		{
			// Initialize a private container so that modifications do not interfere with tests.
			_container = Registrar.Initialize();
		}

		#endregion

		#region Private Fields

		private readonly IContainer _container;

		#endregion

		#region Public Methods

		public void DeleteInstallFile()
		{
			// Ensure clean environment.
			var installFile = Path.Combine(TestContext.CurrentContext.TestDirectory, "ApplicationHelpInstall.xml");
			if (File.Exists(installFile))
			{
				File.Delete(installFile);
			}
		}

		public void TestFixtureDataStoreSetUp()
		{
			// Fake the HttpContext to indicate the database needs to be updated.
			var context = new Mock<HttpContextBase>();
			context.SetupGet(c => c.Application[HelpContentManager.DocmahInitializedKey]).Returns(false);
			context.Setup(c => c.Server.MapPath("~")).Returns(NUnit.Framework.TestContext.CurrentContext.TestDirectory);

			// Create custom container to use mock http context.
			_container.RegisterResolver<HttpContextBase>(c => context.Object);
			
			// Create data store for unit tests.
			var dataStore = _container.ResolveInstance<IDataStore>();
			dataStore.DataStore_Create();

			// Bring the data store schema up to date.
			// This serves as the test for this routine as none of the
			//	other tests will work if this doesn't.
			var helpContentManager = _container.ResolveInstance<IHelpContentManager>();
			helpContentManager.UpdateDataStoreContent();
		}

		public void TestFixtureDataStoreTearDown()
		{
			// Clean up the data store when tests are done.
			var dataStore = _container.ResolveInstance<IDataStore>();
			dataStore.DataStore_Drop();
		}		

		#endregion
	}
}
