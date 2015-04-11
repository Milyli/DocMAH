using System;
using NUnit.Framework;
using Moq;
using DocMAH.Data;

namespace DocMAH.UnitTests.Data
{
	[TestFixture]
	public class DatabaseConfigurationUnitTests
	{
		#region Tests

		[Test]
		[Description("Reads database help version number")]
		public void DatabaseHelpVersion_Read()
		{
			// Arrange
			var databaseAccess = new Mock<IDatabaseAccess>(MockBehavior.Strict);
			databaseAccess.Setup(d => d.Configuration_Read(DatabaseConfiguration.DatabaseHelpVersionKey)).Returns(int.MaxValue);

			var configuration = new DatabaseConfiguration(databaseAccess.Object);
			
			// Act
			var result = configuration.DatabaseHelpVersion;

			// Assert
			Assert.AreEqual(int.MaxValue, result);
			databaseAccess.VerifyAll();
		}

		[Test]
		[Description("Persist database help version number.")]
		public void DatabaseHelpVersion_Update()
		{
			// Arrange
			var databaseAccess = new Mock<IDatabaseAccess>(MockBehavior.Strict);
			databaseAccess.Setup(d => d.Configuration_Update(DatabaseConfiguration.DatabaseHelpVersionKey, int.MaxValue));

			var configuration = new DatabaseConfiguration(databaseAccess.Object);

			// Act
			configuration.DatabaseHelpVersion = int.MaxValue;

			// Assert
			databaseAccess.VerifyAll();
		}

		[Test]
		[Description("Read database schema version number.")]
		public void DatabaseSchemaVersion_Read()
		{
			// Arrange
			var databaseAccess = new Mock<IDatabaseAccess>(MockBehavior.Strict);
			databaseAccess.Setup(d => d.Configuration_Read(DatabaseConfiguration.DatabaseSchemaVersionKey)).Returns(int.MaxValue);

			var configuration = new DatabaseConfiguration(databaseAccess.Object);

			// Act
			var result = configuration.DatabaseSchemaVersion;

			// Assert
			Assert.AreEqual(int.MaxValue, result);
			databaseAccess.VerifyAll();

		}

		[Test]
		[Description("updates database schema version number.")]
		public void DatabaseSchemaVersion_Update()
		{
			// Arrange
			var databaseAccess = new Mock<IDatabaseAccess>(MockBehavior.Strict);
			databaseAccess.Setup(d => d.Configuration_Update(DatabaseConfiguration.DatabaseSchemaVersionKey, int.MaxValue));

			var configuration = new DatabaseConfiguration(databaseAccess.Object);

			// Act
			configuration.DatabaseSchemaVersion = int.MaxValue;

			// Assert
			databaseAccess.VerifyAll();
		}

		#endregion
	}
}
