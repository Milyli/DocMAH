using System;
using System.IO;
using System.Linq;
using System.Web;
using NUnit.Framework;
using Moq;
using DocMAH.Data;

namespace DocMAH.UnitTests.Data
{
	[TestFixture]
	public class DatabaseUpdaterUnitTests
	{
		#region Tests

		[Test]
		[Description("Tests that the schema update is not run if on most recent version.")]
		public void Update_HelpUpdateOnly()
		{
			// Arrange
			var databaseVersions = Enum.GetValues(typeof(DatabaseVersions));
			var lastVersion = databaseVersions.Length;

			var path = Path.GetTempPath();
			string fileName = Path.Combine(path, "ApplicationHelpInstall.xml");
			var updateScript = "Update Script";
			using (var tempFile = File.Create(fileName))
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

			var databaseConfiguration = new Mock<IDatabaseConfiguration>(MockBehavior.Strict);
			databaseConfiguration.SetupGet(c => c.DatabaseSchemaVersion).Returns(lastVersion);
			databaseConfiguration.SetupGet(c => c.DatabaseHelpVersion).Returns(0);

			var databaseAccess = new Mock<IDatabaseAccess>(MockBehavior.Strict);
			databaseAccess.Setup(a => a.Database_RunScript(updateScript));

			var httpContext = new Mock<HttpContextBase>();
			httpContext.SetupGet(c => c.Application["DMH.Initialized"]).Returns(null);
			httpContext.Setup(c => c.Server.MapPath("~")).Returns(path);
			httpContext.SetupSet(c => c.Application["DMH.Initialized"] = true);

			var updater = new DatabaseUpdater(httpContext.Object, databaseAccess.Object, databaseConfiguration.Object);

			// Act
			updater.Update();

			// Assert
			databaseConfiguration.VerifyAll();
			databaseAccess.VerifyAll();

			File.Delete(fileName);
		}

		[Test]
		[Description("Tests that the help content is updated the first time update is called.")]
		public void Update_FirstTime()
		{
			// Arrange			
			var databaseVersions = Enum.GetValues(typeof(DatabaseVersions));
			var lastVersion = databaseVersions.Length;

			var path = Path.GetTempPath();
			string fileName = Path.Combine(path, "ApplicationHelpInstall.xml");
			var updateScript = "Update Script";
			using (var tempFile = File.Create(fileName))
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

			var beforeUpdate = true;
			var lastIndex = lastVersion - 1;
			var databaseConfiguration = new Mock<IDatabaseConfiguration>(MockBehavior.Strict);
			databaseConfiguration.SetupGet(c => c.DatabaseSchemaVersion).Returns(() => {
				if (beforeUpdate)
				{
					beforeUpdate = false;
					return lastVersion - 1;
				}
				return lastVersion;
			});
			databaseConfiguration.SetupGet(c => c.DatabaseHelpVersion).Returns(0);

			var databaseAccess = new Mock<IDatabaseAccess>(MockBehavior.Strict);
			databaseAccess.Setup(a => a.Database_Update((DatabaseVersions)databaseVersions.GetValue(lastIndex)));
			databaseAccess.Setup(a => a.Database_RunScript(updateScript));

			var httpContext = new Mock<HttpContextBase>();
			httpContext.SetupGet(c => c.Application["DMH.Initialized"]).Returns(null);
			httpContext.Setup(c => c.Server.MapPath("~")).Returns(path);
			httpContext.SetupSet(c => c.Application["DMH.Initialized"] = true);

			var updater = new DatabaseUpdater(httpContext.Object, databaseAccess.Object, databaseConfiguration.Object);

			// Act
			updater.Update();

			// Assert
			databaseConfiguration.VerifyAll();
			databaseAccess.VerifyAll();

			File.Delete(fileName);
		}

		[Test]
		[Description("Tests that the help update is not run if update file is not present.")]
		public void Update_SchemaOnly()
		{
			// Arrange
			var path = Path.GetTempPath();
			string fileName = Path.Combine(path, "ApplicationHelpInstall.xml");
			if (File.Exists(fileName))
				File.Delete(fileName);

			var databaseVersions = Enum.GetValues(typeof(DatabaseVersions));
			var lastVersion = databaseVersions.Length;
			var lastIndex = lastVersion - 1;
			var databaseConfiguration = new Mock<IDatabaseConfiguration>(MockBehavior.Strict);
			databaseConfiguration.SetupGet(c => c.DatabaseSchemaVersion).Returns(lastVersion - 1);

			var databaseAccess = new Mock<IDatabaseAccess>(MockBehavior.Strict);
			databaseAccess.Setup(a => a.Database_Update((DatabaseVersions)databaseVersions.GetValue(lastIndex)));

			var httpContext = new Mock<HttpContextBase>();
			httpContext.SetupGet(c => c.Application["DMH.Initialized"]).Returns(null);
			httpContext.Setup(c => c.Server.MapPath("~")).Returns(path);
			httpContext.SetupSet(c => c.Application["DMH.Initialized"] = true);

			var updater = new DatabaseUpdater(httpContext.Object, databaseAccess.Object, databaseConfiguration.Object);

			// Act
			updater.Update();

			// Assert
			databaseConfiguration.VerifyAll();
			databaseAccess.VerifyAll();

			File.Delete(fileName);

		}

		[Test]
		[Description("Do not run if already initialized.")]
		public void Update_Initialized()
		{
			// Arrange
			var httpContext = new Mock<HttpContextBase>();
			httpContext.SetupGet(c => c.Application["DMH.Initialized"]).Returns(true);

			var updater = new DatabaseUpdater(httpContext.Object, null, null);

			// Act
			updater.Update();

			// Assert
			// Should not access other dependencies. If the code does so, a null reference exception will be thrown.
		}

		#endregion
	}
}
