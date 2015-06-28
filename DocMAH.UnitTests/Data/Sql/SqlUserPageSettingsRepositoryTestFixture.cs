using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocMAH.Models;
using NUnit.Framework;

namespace DocMAH.UnitTests.Data.Sql
{
	[TestFixture]
	public class SqlUserPageSettingsRepositoryTestFixture : BaseSqlRepositoryTestFixture
	{
		#region Tests

		[Test]
		[Description("Test CRUD for user page settings in SQL data store.")]
		public void Crud_Success()
		{
			var userName = "TestUserName";
			var help = Models.CreateFirstTimeHelp();
			FirstTimeHelpRepository.Create(help);

			var settings = new UserPageSettings(){
				UserName = userName,
				PageId = help.Id,
				HidePage = false,
			};
			UserPageSettingsRepository.Create(settings);

			settings.HidePage = true;
			UserPageSettingsRepository.Update(settings);

			var existingSettings = UserPageSettingsRepository.Read(settings.UserName, settings.PageId);

			Assert.That(existingSettings.HidePage, Is.True, "The value in the database should have been changed to true.");
		}

		[Test]
		[Description("Deletes all settings for a single page.")]
		public void DeleteByPageId_Success()
		{
			// Arrange
			var targetHelp = Models.CreateFirstTimeHelp();
			FirstTimeHelpRepository.Create(targetHelp);

			var noiseHelp = Models.CreateFirstTimeHelp();
			FirstTimeHelpRepository.Create(noiseHelp);

			var userName1 = "Test1@docmah.org";
			var userName2 = "Test2@docmah.org";

			var targetSettings1 = new UserPageSettings { PageId = targetHelp.Id, UserName = userName1 };
			UserPageSettingsRepository.Create(targetSettings1);
			var targetSettings2 = new UserPageSettings { PageId = targetHelp.Id, UserName = userName2 };
			UserPageSettingsRepository.Create(targetSettings2);
			var noiseSettings = new UserPageSettings { PageId = noiseHelp.Id, UserName = userName1 };
			UserPageSettingsRepository.Create(noiseSettings);

			// Act
			UserPageSettingsRepository.DeleteByPageId(targetHelp.Id);

			// Assert
			var deletedSettings1 = UserPageSettingsRepository.Read(userName1, targetHelp.Id);
			Assert.That(deletedSettings1, Is.Null, "First user's settings for target page should have been deleted.");
			var deletedSettings2 = UserPageSettingsRepository.Read(userName2, targetHelp.Id);
			Assert.That(deletedSettings2, Is.Null, "Second user's settings for target page should have been deleted.");
			var existingSettings = UserPageSettingsRepository.Read(userName1, noiseHelp.Id);
			Assert.That(existingSettings, Is.Not.Null, "Settings for other pages should remain.");
		}

		[Test]
		[Description("Method should not throw exception on empty lists.")]
		public void DeleteExcept_EmptyList()
		{
			// Arrange
			
			// Act
			UserPageSettingsRepository.DeleteExcept(new List<int>());

			// Assert
			// Above method call should not throw a SqlException.

		}

		[Test]
		[Description("Deletes user page settings where the page id is not included in the list.")]
		public void DeleteExcept_Success()
		{
			// Arrange
			var keptHelp = Models.CreateFirstTimeHelp();
			FirstTimeHelpRepository.Create(keptHelp);

			var deletedHelp = Models.CreateFirstTimeHelp();
			FirstTimeHelpRepository.Create(deletedHelp);

			var anotherDeletedHelp = Models.CreateFirstTimeHelp();
			FirstTimeHelpRepository.Create(anotherDeletedHelp);

			var userName1 = "user1@testerson.com";
			var userName2 = "user2@testerson.com";

			var keptPageSetting = new UserPageSettings { PageId = keptHelp.Id, UserName = userName1 };
			UserPageSettingsRepository.Create(keptPageSetting);
			var deletedPageSetting1 = new UserPageSettings { PageId = deletedHelp.Id, UserName = userName1 };
			UserPageSettingsRepository.Create(deletedPageSetting1);
			var deletedPageSetting2 = new UserPageSettings { PageId = deletedHelp.Id, UserName = userName2 };
			UserPageSettingsRepository.Create(deletedPageSetting2);
			var anotherDeletedPageSetting = new UserPageSettings { PageId = anotherDeletedHelp.Id, UserName = userName1 };
			UserPageSettingsRepository.Create(anotherDeletedPageSetting);

			// Act
			UserPageSettingsRepository.DeleteExcept(new List<int> { keptHelp.Id });

			// Assert
			var keptResult = UserPageSettingsRepository.Read(userName1, keptHelp.Id);
			Assert.That(keptResult, Is.Not.Null, "The kept settings should still be in the data store.");
			var deletedResult1 = UserPageSettingsRepository.Read(userName1, deletedHelp.Id);
			Assert.That(deletedResult1, Is.Null, "Settings shoudl not exist for the first deleted page and first user.");
			var deletedResult2 = UserPageSettingsRepository.Read(userName2, deletedHelp.Id);
			Assert.That(deletedResult2, Is.Null, "Settings should not exist for the first deleted page and second user.");
			var anotherDeletedResult = UserPageSettingsRepository.Read(userName1, anotherDeletedHelp.Id);
			Assert.That(anotherDeletedResult, Is.Null, "Settings should not exist for the second deleted page and first user.");
		}

		#endregion
	}
}
