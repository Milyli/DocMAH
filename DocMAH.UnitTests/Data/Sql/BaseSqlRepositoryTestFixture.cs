using System;
using System.Collections.Generic;
using System.IO;
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
		protected SqlDocumentationPageRepository DocumentationPageRepository { get; set; }
		protected SqlFirstTimeHelpRepository FirstTimeHelpRepository { get; set; }
		protected SqlUserPageSettingsRepository UserPageSettingsRepository { get; set; }


		#endregion		

		#region SetUp / TearDown

		[TestFixtureSetUp]
		public void BaseSqlRepositoryTestFixtureSetUp()
		{
			var dataStoreManager = new TestFixtureDataStoreManager();
			dataStoreManager.CreateDataStore();
		}

		[TestFixtureTearDown]
		public void BaseSqlRepositoryTestFixtureTearDown()
		{
			var dataStoreManager = new TestFixtureDataStoreManager();
			dataStoreManager.DeleteDataStore();
		}

		[SetUp]
		public void BaseSqlRepositorySetUp()
		{
			var container = Registrar.Initialize();
			var connectionFactory = container.Resolve<ISqlConnectionFactory>();

			BulletRepository = new SqlBulletRepository(connectionFactory);
			ConfigurationRepository = new SqlConfigurationRepository(connectionFactory);
			DocumentationPageRepository = new SqlDocumentationPageRepository(connectionFactory);
			FirstTimeHelpRepository = new SqlFirstTimeHelpRepository(connectionFactory);
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
			DocumentationPageRepository = null;
		}
		
		#endregion
	}
}
