using System;
using System.IO;
using System.Linq;
using System.Web;
using NUnit.Framework;
using Moq;
using DocMAH.Data;
using DocMAH.Data.Sql;
using DocMAH.Extensions;
using DocMAH.Configuration;
using DocMAH.Content;
using System.Collections.Generic;
namespace DocMAH.UnitTests.Data
{
	[TestFixture]
	public class HelpContentManagerTestFixture : BaseTestFixture
	{
		#region Private Fields

		private string _contentFileName;
		private string _tempPath;

		#endregion

		#region Private Methods

		private void WriteMinimalContentFile(
			DataStoreSchemaVersions? dataStoreSchemaVersion = null,
			ContentFileSchemaVersions? contentFileSchemaVersion = null,
			int contentFileContentVersion = 1)
		{
			dataStoreSchemaVersion = dataStoreSchemaVersion ?? EnumExtensions.GetMaxValue<DataStoreSchemaVersions>();
			contentFileSchemaVersion = contentFileSchemaVersion ?? EnumExtensions.GetMaxValue<ContentFileSchemaVersions>();

			using (var tempFile = File.Create(_contentFileName))
			{
				var fileContents = string.Format("<{0} {1}='{2}' {3}='{4}' {5}='{6}'></{0}>",
					ContentFileConstants.RootNode,
					ContentFileConstants.DataStoreSchemaVersionAttribute,
					(int)dataStoreSchemaVersion,
					ContentFileConstants.FileSchemaVersionAttribute,
					(int)contentFileSchemaVersion,
					ContentFileConstants.FileContentVersionAttribute,
					contentFileContentVersion);
				var bytes = System.Text.Encoding.UTF8.GetBytes(fileContents);
				tempFile.Write(bytes, 0, bytes.Length);
				tempFile.Close();
			}
		}		

		#endregion

		#region SetUp / TearDown
		
		[SetUp]
		public void SetUp()
		{
			_tempPath = Path.GetTempPath();
			_contentFileName = Path.Combine(_tempPath, ContentFileConstants.ContentFileName);
		}

		[TearDown]
		public void TearDown()
		{
			if (File.Exists(_contentFileName))
				File.Delete(_contentFileName);
		}

		#endregion

		#region Tests

		[Test]
		[Description("Does not update content if file content version is equal to data store content version.")]
		public void Import_ContentUpToDate()
		{
			// Arrange
			WriteMinimalContentFile();

			var lastDataStoreVersion = EnumExtensions.GetMaxValue<DataStoreSchemaVersions>();
			var dataStoreConfiguration = Mocks.Create<IDataStoreConfiguration>(MockBehavior.Strict);
			dataStoreConfiguration.SetupGet(c => c.DataStoreSchemaVersion).Returns((int)lastDataStoreVersion);
			dataStoreConfiguration.SetupGet(c => c.HelpContentVersion).Returns(1);

			var helpContentManager = new HelpContentManager(null, dataStoreConfiguration.Object, null, null);

			// Act
			helpContentManager.ImportContent(_contentFileName);

			// Assert
			Mocks.VerifyAll();
		}

		[Test]
		[Description("Tests that the schema update is not run if on most recent version.")]
		public void Import_UpdateContent()
		{
			// Arrange
			WriteMinimalContentFile();

			var bulletRepository = Mocks.Create<IBulletRepository>();
			bulletRepository.Setup(r => r.DeleteExcept(It.IsAny<List<int>>()));

			var lastDataStoreVersion = EnumExtensions.GetMaxValue<DataStoreSchemaVersions>();
			var dataStoreConfiguration = Mocks.Create<IDataStoreConfiguration>(MockBehavior.Strict);
			dataStoreConfiguration.SetupGet(c => c.DataStoreSchemaVersion).Returns((int)lastDataStoreVersion);
			dataStoreConfiguration.SetupGet(c => c.HelpContentVersion).Returns(0);
			dataStoreConfiguration.SetupSet(c => c.HelpContentVersion = 1);

			var pageRepository = Mocks.Create<IPageRepository>();
			pageRepository.Setup(r => r.DeleteExcept(It.IsAny<List<int>>()));

			var userPageSettingsRepository = Mocks.Create<IUserPageSettingsRepository>();
			userPageSettingsRepository.Setup(r => r.DeleteExcept(It.IsAny<List<int>>()));

			var updater = new HelpContentManager(
 				bulletRepository.Object,
				dataStoreConfiguration.Object,
				pageRepository.Object,
				userPageSettingsRepository.Object);

			// Act
			updater.ImportContent(_contentFileName);

			// Assert
			Mocks.VerifyAll();
		}

		[Test]
		[Description("Tests that the help update is not run if update file is not present.")]
		public void Import_FileNotPresent()
		{
			// Arrange
			// No methods should be called on the repositories.
			// Default behavior strict and the verify all assure this.
			var bulletRepository = Mocks.Create<IBulletRepository>();
			var dataStoreConfiguration = Mocks.Create<IDataStoreConfiguration>();
			var pageRepository = Mocks.Create<IPageRepository>();
			var userPageSettingsRepository = Mocks.Create<IUserPageSettingsRepository>();
			
			var updater = new HelpContentManager(
				bulletRepository.Object, 
				dataStoreConfiguration.Object, 
				pageRepository.Object,
				userPageSettingsRepository.Object);

			// Act
			updater.ImportContent(_contentFileName);

			// Assert
			Mocks.VerifyAll();
		}

		#endregion
	}
}
