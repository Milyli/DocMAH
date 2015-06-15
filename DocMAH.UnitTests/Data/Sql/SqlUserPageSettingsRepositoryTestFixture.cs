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

		#endregion
	}
}
