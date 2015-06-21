using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using DocMAH.Data;
using DocMAH.Data.Sql;
using DocMAH.Dependencies;
using NUnit.Framework;

namespace DocMAH.UnitTests.Data.Sql
{
	[TestFixture]
	public class BaseSqlRepositoryTestFixture : BaseTestFixture
	{
		#region Private Fields

		private TransactionScope _transactionScope;

		#endregion

		#region Protected Properties
		
		protected SqlBulletRepository BulletRepository { get; set; }
		protected SqlConfigurationRepository ConfigurationRepository { get; set; }
		protected SqlPageRepository PageRepository { get; set; }
		protected SqlUserPageSettingsRepository UserPageSettingsRepository { get; set; }


		#endregion
		

		#region SetUp / TearDown

		[TestFixtureSetUp]
		public void BaseSqlRepositoryTestFixtureSetUp()
		{
			var dataStoreManager = new TestFixtureDataStoreManager();
			dataStoreManager.TestFixtureDataStoreSetUp();
		}

		[TestFixtureTearDown]
		public void BaseSqlRepositoryTestFixtureTearDown()
		{
			var dataStoreManager = new TestFixtureDataStoreManager();
			dataStoreManager.TestFixtureDataStoreTearDown();
		}

		[SetUp]
		public void BaseSqlRepositorySetUp()
		{
			var container = Registrar.Initialize();
			var connectionFactory = container.Resolve<ISqlConnectionFactory>();

			BulletRepository = new SqlBulletRepository(connectionFactory);
			ConfigurationRepository = new SqlConfigurationRepository(connectionFactory);
			PageRepository = new SqlPageRepository(connectionFactory);
			UserPageSettingsRepository = new SqlUserPageSettingsRepository(connectionFactory);

			_transactionScope = new TransactionScope();
		}

		[TearDown]
		public void BaseSqlRepositoryTearDown()
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
