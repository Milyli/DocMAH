using System;
using NUnit.Framework;
using Moq;
using DocMAH.Data;
using DocMAH.Configuration;
namespace DocMAH.UnitTests.Data
{
	[TestFixture]
	public class DataStoreConfigurationTestFixture
	{
		#region Tests

		[Test]
		[Description("Read database schema version number.")]
		public void DatabaseSchemaVersion_Read()
		{
			// Arrange
			var configurationRepository = new Mock<IConfigurationRepository>(MockBehavior.Strict);
			configurationRepository.Setup(r => r.Read(DataStoreConfiguration.DataStoreSchemaVersionKey)).Returns(int.MaxValue);

			var configuration = new DataStoreConfiguration(configurationRepository.Object);

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
			configurationRepository.Setup(r => r.Update(DataStoreConfiguration.DataStoreSchemaVersionKey, int.MaxValue));

			var configuration = new DataStoreConfiguration(configurationRepository.Object);

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
			configurationRepository.Setup(r => r.Read(DataStoreConfiguration.HelpContentVersionKey)).Returns(int.MaxValue);

			var configuration = new DataStoreConfiguration(configurationRepository.Object);

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
			configurationRepository.Setup(r => r.Update(DataStoreConfiguration.HelpContentVersionKey, int.MaxValue));

			var configuration = new DataStoreConfiguration(configurationRepository.Object);

			// Act
			configuration.HelpContentVersion = int.MaxValue;

			// Assert
			configurationRepository.VerifyAll();
		}

		#endregion
	}
}
