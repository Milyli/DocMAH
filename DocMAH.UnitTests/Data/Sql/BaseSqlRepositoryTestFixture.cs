using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
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
		protected SqlPageRepository PageRepository { get; set; }

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
			DataStore = new SqlDataStore();
			Models = new ModelFactory();

			BulletRepository = new SqlBulletRepository();
			PageRepository = new SqlPageRepository();

			_transactionScope = new TransactionScope();
		}

		[TearDown]
		public virtual void TearDown()
		{
			_transactionScope.Dispose();
			_transactionScope = null;

			BulletRepository = null;
			PageRepository = null;
		}
		
		#endregion
	}
}
