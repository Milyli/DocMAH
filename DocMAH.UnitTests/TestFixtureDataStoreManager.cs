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
using DocMAH.Data;
using DocMAH.Data.Sql;
using Moq;
using NUnit.Framework;

namespace DocMAH.UnitTests
{
	public class TestFixtureDataStoreManager
	{
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
			
			// Create data store for unit tests.
			var connectionFactory = new SqlConnectionFactory();
			var configurationRepository = new SqlConfigurationRepository(connectionFactory);
			var configurationService = new ConfigurationService(configurationRepository);
			var dataStore = new SqlDataStore(configurationService, connectionFactory);
			dataStore.DataStore_Create();

			// Bring the data store schema up to date.
			// This serves as the test for this routine as none of the
			//	other tests will work if this doesn't.
			var databaseUpdater = new HelpContentManager(context.Object, dataStore, configurationService);
			databaseUpdater.UpdateDataStoreContent();
		}

		public void TestFixtureDataStoreTearDown()
		{
			// Clean up the data store when tests are done.
			var connectionFactory = new SqlConnectionFactory();
			var configurationRepository = new SqlConfigurationRepository(connectionFactory);
			var configurationService = new ConfigurationService(configurationRepository);
			var dataStore = new SqlDataStore(configurationService, connectionFactory);
			dataStore.DataStore_Drop();
		}
	}
}
