using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using DocMAH.Data;
using DocMAH.Data.Sql;
using NUnit.Framework;

namespace DocMAH.UnitTests.Data.Sql
{
	[TestFixture]
	public class BaseSqlRepositoryTestFixture
	{
		#region Private Fields

		private TransactionScope _transactionScope;

		#endregion

		#region Protected Properties

		protected SqlDataStore DataStore { get; set; } // To be deleted... eventually.
		protected ModelFactory Models { get; set; }

		protected SqlBulletRepository BulletRepository { get; set; }
		protected SqlConfigurationRepository ConfigurationRepository { get; set; }
		protected SqlPageRepository PageRepository { get; set; }
		protected SqlUserPageSettingsRepository UserPageSettingsRepository { get; set; }


		#endregion
		

		#region SetUp / TearDown

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			var dataStoreManager = new TestFixtureDataStoreManager();
			dataStoreManager.TestFixtureDataStoreSetUp();
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			var dataStoreManager = new TestFixtureDataStoreManager();
			dataStoreManager.TestFixtureDataStoreTearDown();
		}

		[SetUp]
		public virtual void SetUp()
		{

			Models = new ModelFactory();

			var connectionFactory = new SqlConnectionFactory();
			BulletRepository = new SqlBulletRepository(connectionFactory);
			ConfigurationRepository = new SqlConfigurationRepository(connectionFactory);
			PageRepository = new SqlPageRepository(connectionFactory);
			UserPageSettingsRepository = new SqlUserPageSettingsRepository(connectionFactory);

			var configurationService = new ConfigurationService(ConfigurationRepository);

			DataStore = new SqlDataStore(configurationService, connectionFactory);

			_transactionScope = new TransactionScope();
		}

		[TearDown]
		public virtual void TearDown()
		{
			_transactionScope.Dispose();
			_transactionScope = null;

			BulletRepository = null;
			ConfigurationRepository = null;
			PageRepository = null;
		}
		
		#endregion
	}
}
