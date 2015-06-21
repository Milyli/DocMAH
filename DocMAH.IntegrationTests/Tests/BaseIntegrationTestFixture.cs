using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using DocMAH.Dependencies;
using DocMAH.UnitTests;
using NUnit.Framework;

namespace DocMAH.IntegrationTests.Tests
{
	[TestFixture]
	public class BaseIntegrationTestFixture : BaseTestFixture
	{
		#region Protected Properties

		protected IContainer Container { get; set; }
		protected TestFixtureDataStoreManager DataStoreManager { get; set; }
		protected MockHttpContext HttpContext { get; set; }

		#endregion

		#region SetUp / TearDown

		[SetUp]
		public void BaseIntegrationSetUp()
		{
			HttpContext = new MockHttpContext();
			Container.Register<HttpContextBase>(c => HttpContext.Object);
		}

		
		[TestFixtureSetUp]
		public void BaseIntegrationTestFixtureSetUp()
		{
			Container = Registrar.Initialize();

			DataStoreManager = new TestFixtureDataStoreManager();
			DataStoreManager.DeleteInstallFile();
			DataStoreManager.TestFixtureDataStoreSetUp();
		}

		[TestFixtureTearDown]
		public void BaseIntegrationTestFixtureTearDown()
		{
			DataStoreManager.DeleteInstallFile();
			DataStoreManager.TestFixtureDataStoreTearDown();
		}

		#endregion
		
	}
}
