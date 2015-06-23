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
			var page = Models.CreatePage();
			PageRepository.Create(page);

			var settings = new UserPageSettings(){
				UserName = userName,
				PageId = page.Id,
				HidePage = false,
			};
			UserPageSettingsRepository.Create(settings);

			settings.HidePage = true;
			UserPageSettingsRepository.Update(settings);

			var existingSettings = UserPageSettingsRepository.Read(settings.UserName, settings.PageId);

			Assert.That(existingSettings.HidePage, Is.True, "The value in the database should have been changed to true.");
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
			var keptPage = Models.CreatePage();
			PageRepository.Create(keptPage);

			var deletedPage = Models.CreatePage();
			PageRepository.Create(deletedPage);

			var anotherDeletedPage = Models.CreatePage();
			PageRepository.Create(anotherDeletedPage);

			var userName1 = "user1@testerson.com";
			var userName2 = "user2@testerson.com";

			var keptPageSetting = new UserPageSettings { PageId = keptPage.Id, UserName = userName1 };
			UserPageSettingsRepository.Create(keptPageSetting);
			var deletedPageSetting1 = new UserPageSettings { PageId = deletedPage.Id, UserName = userName1 };
			UserPageSettingsRepository.Create(deletedPageSetting1);
			var deletedPageSetting2 = new UserPageSettings { PageId = deletedPage.Id, UserName = userName2 };
			UserPageSettingsRepository.Create(deletedPageSetting2);
			var anotherDeletedPageSetting = new UserPageSettings { PageId = anotherDeletedPage.Id, UserName = userName1 };
			UserPageSettingsRepository.Create(anotherDeletedPageSetting);

			// Act
			UserPageSettingsRepository.DeleteExcept(new List<int> { keptPage.Id });

			// Assert
			var keptResult = UserPageSettingsRepository.Read(userName1, keptPage.Id);
			Assert.That(keptResult, Is.Not.Null, "The kept settings should still be in the data store.");
			var deletedResult1 = UserPageSettingsRepository.Read(userName1, deletedPage.Id);
			Assert.That(deletedResult1, Is.Null, "Settings shoudl not exist for the first deleted page and first user.");
			var deletedResult2 = UserPageSettingsRepository.Read(userName2, deletedPage.Id);
			Assert.That(deletedResult2, Is.Null, "Settings should not exist for the first deleted page and second user.");
			var anotherDeletedResult = UserPageSettingsRepository.Read(userName1, anotherDeletedPage.Id);
			Assert.That(anotherDeletedResult, Is.Null, "Settings should not exist for the second deleted page and first user.");
		}

		#endregion
	}
}
