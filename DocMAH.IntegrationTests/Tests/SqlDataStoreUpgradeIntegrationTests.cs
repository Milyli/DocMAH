using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using DocMAH.Dependencies;
using NUnit.Framework;
using DocMAH.Configuration;
using System.Data;
using DocMAH.Data.Sql;
namespace DocMAH.IntegrationTests.Tests
{
	[TestFixture]
	public class SqlDataStoreUpgradeIntegrationTests
	{
		#region Protected Properties

		protected IContainer Container { get; set; }
		protected MockHttpContext HttpContext { get; set; }

		#endregion

		#region Private Fields

		private const string DatabaseNameTemplate = "docmah-V{0}";
		private const string IntegrationTestApplicationName = "DocMAH Sql Data Store Integration Tests";
		private const string LocalDBDataSource = @"(LocalDb)\v11.0";

		// Values should match version numbers on files in Test_Data directory.
		private const string Version_01 = "01";

		#endregion

		#region Private Properties

		private string ArchiveDirectory { get { return Path.Combine(TestDirectory, @"..\..\Test_Data"); } }
		private string TestDirectory { get { return TestContext.CurrentContext.TestDirectory; } }

		#endregion

		#region Private Methods

		private string ArchiveDataFile(string version) { return Path.Combine(ArchiveDirectory, string.Format("DocMAH-Archive-V{0}.mdf", version)); }

		private string LiveDataFile(string version) { return Path.Combine(TestDirectory, string.Format("DocMAH-V{0}.{1}", version, "mdf")); }

		private string LiveLogFile(string version) { return Path.Combine(TestDirectory, string.Format("DocMAH-V{0}_log.ldf", version)); }

		private void SetUpDatabase(string version)
		{
			string liveDataFile = LiveDataFile(version);
			string liveLogFile = LiveLogFile(version);

			if (File.Exists(liveDataFile))
				File.Delete(liveDataFile);
			if (File.Exists(liveLogFile))
				File.Delete(liveLogFile);

			File.Copy(ArchiveDataFile(version), liveDataFile);

			var connectionStringBuilder = new SqlConnectionStringBuilder()
			{
				DataSource = LocalDBDataSource,
				AttachDBFilename = liveDataFile,
				InitialCatalog = string.Format(DatabaseNameTemplate, version),
				IntegratedSecurity = true,
				ApplicationName = IntegrationTestApplicationName,
			};
			Configurator.ConnectionString = connectionStringBuilder.ToString();
		}

		private void TearDownDatabase(string version)
		{
			var connectionStringBuilder = new SqlConnectionStringBuilder()
			{
				DataSource = LocalDBDataSource,
				InitialCatalog = "master",
				IntegratedSecurity = true,
				ApplicationName = IntegrationTestApplicationName,
			};
			using (var connection = new SqlConnection(connectionStringBuilder.ConnectionString))
			using (var command = connection.CreateCommand())
			{
				connection.Open();
				command.CommandType = CommandType.Text;
				var databaseName = string.Format(DatabaseNameTemplate, version);

				command.CommandText = string.Format("ALTER DATABASE [{0}] SET OFFLINE WITH ROLLBACK IMMEDIATE", databaseName);
				command.ExecuteNonQuery();

				command.CommandText = string.Format("exec sp_detach_db '{0}'", databaseName);
				command.ExecuteNonQuery();
			}

			string liveDataFile = LiveDataFile(version);
			if (File.Exists(liveDataFile))
				File.Delete(liveDataFile);

			string liveLogFile = LiveLogFile(version);
			if (File.Exists(liveLogFile))
				File.Delete(liveLogFile);
		}

		#endregion

		#region Tests

		[Test]
		[Description("Upgrades a copy of a SQL data store containing data from V01 to V02.")]
		public void Upgrade_V01_To_V02()
		{
			// Arrange
			SetUpDatabase(Version_01);

			// Act
			using (var connection = new SqlConnection(Configurator.ConnectionString))
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.Database_Update_02;

				connection.Open();
				command.ExecuteNonQuery();
			}

			// Assert
			using (var connection = new SqlConnection(Configurator.ConnectionString))
			using (var command = connection.CreateCommand())
			{
				connection.Open();
				command.CommandType = CommandType.Text;

				command.CommandText = "SELECT COUNT(1) FROM [dbo].[DocmahFirstTimeHelp]";
				var result = command.ExecuteScalar();
				Assert.That(result, Is.EqualTo(1), "One first time help page that exists in the database archive of version 1 should have been converted.");

				command.CommandText = "SELECT COUNT(1) FROM [dbo].[DocmahDocumentationPages]";
				result = command.ExecuteScalar();
				Assert.That(result, Is.EqualTo(12), "Twelve documentation pages that exist in the database archive of version 1 should have been converted.");

				command.CommandText = "SELECT COUNT(1) FROM [dbo].[DocmahDocumentationPages] WHERE ParentPageId = 2";
				result = command.ExecuteScalar();
				Assert.That(result, Is.EqualTo(1), "The child of the first time help page should have been promoted to be the only child of the first time help's parent. That parent (id=2) should start and end the upgrade with a single child.");
			}

			// Clean Up
			TearDownDatabase(Version_01);
		}

		#endregion
	}
}
