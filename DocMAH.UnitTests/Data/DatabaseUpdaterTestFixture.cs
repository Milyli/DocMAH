using System;
using System.IO;
using System.Linq;
using System.Web;
using NUnit.Framework;
using Moq;
using DocMAH.Data;
using DocMAH.Data.Sql;
using DocMAH.Extensions;

namespace DocMAH.UnitTests.Data
{
	[TestFixture]
	public class DatabaseUpdaterTestFixture
	{
		#region Private Fields

		private string _installFileName;
		private string _tempPath;

		#endregion

		#region SetUp / TearDown
		
		[SetUp]
		public void SetUp()
		{
			_tempPath = Path.GetTempPath();
			_installFileName = Path.Combine(_tempPath, "ApplicationHelpInstall.xml");
		}

		[TearDown]
		public void TearDown()
		{
			if (File.Exists(_installFileName))
				File.Delete(_installFileName);
		}

		#endregion

		#region Tests

		[Test]
		[Description("Tests that the schema update is not run if on most recent version.")]
		public void Update_HelpUpdateOnly()
		{
			// Arrange
			var lastVersion = EnumExtensions.GetMaxValue<SqlDataStoreVersions, int>();

			var updateScript = "Update Script";
			using (var tempFile = File.Create(_installFileName))
			{
				var fileContents = string.Format("<{0} {1}='{2}' {3}='{4}'><{5}>{6}</{5}></{0}>",
					XmlNodeNames.UpdateScriptsElement,
					XmlNodeNames.FileSchemaVersionAttribute,
					lastVersion,
					XmlNodeNames.FileHelpVersionAttribute,
					1,
					XmlNodeNames.UpdateScriptElement,
					updateScript);
				var bytes = System.Text.Encoding.UTF8.GetBytes(fileContents);
				tempFile.Write(bytes, 0, bytes.Length);
				tempFile.Close();
			}

			var configurationService = new Mock<IConfigurationService>(MockBehavior.Strict);
			configurationService.SetupGet(c => c.DataStoreSchemaVersion).Returns(lastVersion);
			configurationService.SetupGet(c => c.HelpContentVersion).Returns(0);

			var dataStore = new Mock<IDataStore>(MockBehavior.Strict);
			dataStore.Setup(d => d.DataStore_Update());
			dataStore.Setup(a => a.Database_RunScript(updateScript));

			var httpContext = new Mock<HttpContextBase>();
			httpContext.SetupGet(c => c.Application[ContentFileManager.DocmahInitializedKey]).Returns(null);
			httpContext.Setup(c => c.Server.MapPath("~")).Returns(_tempPath);
			httpContext.SetupSet(c => c.Application[ContentFileManager.DocmahInitializedKey] = true);

			var updater = new ContentFileManager(httpContext.Object, dataStore.Object, configurationService.Object);

			// Act
			updater.Update();

			// Assert
			configurationService.VerifyAll();
			dataStore.VerifyAll();
		}

		[Test]
		[Description("Tests that the help content is updated the first time update is called.")]
		public void Update_FirstTime()
		{
			// Arrange		
			var lastVersion = EnumExtensions.GetMaxValue<SqlDataStoreVersions, int>();

			var updateScript = "Update Script";
			using (var tempFile = File.Create(_installFileName))
			{
				var fileContents = string.Format("<{0} {1}='{2}' {3}='{4}'><{5}>{6}</{5}></{0}>",
					XmlNodeNames.UpdateScriptsElement,
					XmlNodeNames.FileSchemaVersionAttribute,
					lastVersion,
					XmlNodeNames.FileHelpVersionAttribute,
					1,
					XmlNodeNames.UpdateScriptElement,
					updateScript);
				var bytes = System.Text.Encoding.UTF8.GetBytes(fileContents);
				tempFile.Write(bytes, 0, bytes.Length);
				tempFile.Close();
			}

			var databaseConfiguration = new Mock<IConfigurationService>(MockBehavior.Strict);
			databaseConfiguration.SetupGet(c => c.DataStoreSchemaVersion).Returns(lastVersion);
			databaseConfiguration.SetupGet(c => c.HelpContentVersion).Returns(0);

			var databaseAccess = new Mock<IDataStore>(MockBehavior.Strict);
			databaseAccess.Setup(a => a.DataStore_Update());
			databaseAccess.Setup(a => a.Database_RunScript(updateScript));

			var httpContext = new Mock<HttpContextBase>();
			httpContext.SetupGet(c => c.Application[ContentFileManager.DocmahInitializedKey]).Returns(null);
			httpContext.Setup(c => c.Server.MapPath("~")).Returns(_tempPath);
			httpContext.SetupSet(c => c.Application[ContentFileManager.DocmahInitializedKey] = true);

			var updater = new ContentFileManager(httpContext.Object, databaseAccess.Object, databaseConfiguration.Object);

			// Act
			updater.Update();

			// Assert
			databaseConfiguration.VerifyAll();
			databaseAccess.VerifyAll();
		}

		[Test]
		[Description("Tests that the help update is not run if update file is not present.")]
		public void Update_SchemaOnly()
		{
			// Arrange
			var databaseConfiguration = new Mock<IConfigurationService>(MockBehavior.Strict);

			var databaseAccess = new Mock<IDataStore>(MockBehavior.Strict);
			databaseAccess.Setup(a => a.DataStore_Update());

			var httpContext = new Mock<HttpContextBase>();
			httpContext.SetupGet(c => c.Application[ContentFileManager.DocmahInitializedKey]).Returns(null);
			httpContext.Setup(c => c.Server.MapPath("~")).Returns(_tempPath);
			httpContext.SetupSet(c => c.Application[ContentFileManager.DocmahInitializedKey] = true);

			var updater = new ContentFileManager(httpContext.Object, databaseAccess.Object, databaseConfiguration.Object);

			// Act
			updater.Update();

			// Assert
			databaseConfiguration.VerifyAll();
			databaseAccess.VerifyAll();

			File.Delete(_installFileName);

		}

		[Test]
		[Description("Do not run if already initialized.")]
		public void Update_Initialized()
		{
			// Arrange
			var httpContext = new Mock<HttpContextBase>();
			httpContext.SetupGet(c => c.Application[ContentFileManager.DocmahInitializedKey]).Returns(true);

			var updater = new ContentFileManager(httpContext.Object, null, null);

			// Act
			updater.Update();

			// Assert
			// Should not access other dependencies. If the code does so, a null reference exception will be thrown.
		}

		#endregion
	}
}
