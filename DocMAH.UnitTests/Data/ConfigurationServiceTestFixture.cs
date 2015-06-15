using System;
using NUnit.Framework;
using Moq;
using DocMAH.Data;

namespace DocMAH.UnitTests.Data
{
	[TestFixture]
	public class ConfigurationServiceTestFixture
	{
		#region Tests

		[Test]
		[Description("Read database schema version number.")]
		public void DatabaseSchemaVersion_Read()
		{
			// Arrange
			var configurationRepository = new Mock<IConfigurationRepository>(MockBehavior.Strict);
			configurationRepository.Setup(r => r.Read(DocMAH.Data.ConfigurationService.DataStoreSchemaVersionKey)).Returns(int.MaxValue);

			var configuration = new ConfigurationService(configurationRepository.Object);

			// Act
			var result = configuration.DataStoreSchemaVersion;

			// Assert
			Assert.AreEqual(int.MaxValue, result);
			configurationRepository.VerifyAll();

		}

		[Test]
		[Description("updates database schema version number.")]
		public void DatabaseSchemaVersion_Update()
		{
			// Arrange
			var configurationRepository = new Mock<IConfigurationRepository>(MockBehavior.Strict);
			configurationRepository.Setup(r => r.Update(ConfigurationService.DataStoreSchemaVersionKey, int.MaxValue));

			var configuration = new ConfigurationService(configurationRepository.Object);

			// Act
			configuration.DataStoreSchemaVersion = int.MaxValue;

			// Assert
			configurationRepository.VerifyAll();
		}

		[Test]
		[Description("Reads database help version number")]
		public void HelpContentVersion_Read()
		{
			// Arrange
			var configurationRepository = new Mock<IConfigurationRepository>(MockBehavior.Strict);
			configurationRepository.Setup(r => r.Read(DocMAH.Data.ConfigurationService.HelpContentVersionKey)).Returns(int.MaxValue);

			var configuration = new ConfigurationService(configurationRepository.Object);
			
			// Act
			var result = configuration.HelpContentVersion;

			// Assert
			Assert.AreEqual(int.MaxValue, result);
			configurationRepository.VerifyAll();
		}

		[Test]
		[Description("Persist database help version number.")]
		public void HelpContentVersion_Update()
		{
			// Arrange
			var configurationRepository = new Mock<IConfigurationRepository>(MockBehavior.Strict);
			configurationRepository.Setup(r => r.Update(ConfigurationService.HelpContentVersionKey, int.MaxValue));

			var configuration = new ConfigurationService(configurationRepository.Object);

			// Act
			configuration.HelpContentVersion = int.MaxValue;

			// Assert
			configurationRepository.VerifyAll();
		}

		#endregion
	}
}
