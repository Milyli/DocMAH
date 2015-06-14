using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocMAH.Data.Sql;
using NUnit.Framework;

namespace DocMAH.UnitTests.Data.Sql
{
	[TestFixture]
	public class SqlBulletRepositoryTestFixture : BaseSqlRepositoryTestFixture
	{
		#region Tests



		// I'm not normally a fan of testing multiple methods in a single test.
		// However, in this case I would otherwise need to write a bunch of
		// SQL just for the unit tests.
		// 
		// As I'm feeling lazy I'm not going to spend time on that right now.
		//
		// This does verify that the data access layer is internally
		// consistent, which is good enough for the time being.
		[Test]
		[Description("Tests bullet create, read, update and delete methods.")]
		public void Crud_Success()
		{
			var page = Models.CreatePage();
			PageRepository.Create(page);
			
			var newBullet = Models.CreateBullet(page.Id);
			Assert.AreEqual(0, newBullet.Id, "The bullet id should not be set until after data layer Bullet_Create method is called.");

			BulletRepository.Create(newBullet);
			Assert.AreNotEqual(0, newBullet.Id, "The bullet id should have been set by the data layer.");

			var oldText = newBullet.Text;
			newBullet.Text = "New Bullet Text.";
			BulletRepository.Update(newBullet);

			var existingBullet = BulletRepository.ReadByPageId(page.Id).FirstOrDefault();
			Assert.IsNotNull(existingBullet, "The bullet should still exist in the database.");
			Assert.AreNotEqual(oldText, existingBullet.Text, "The bullet's text should have been updated.");
			Assert.AreEqual(newBullet.VerticalOffset, existingBullet.VerticalOffset, "The rest of the bullet instances' properties should be the same.");

			BulletRepository.Delete(existingBullet.Id);

			var deletedBullet = BulletRepository.ReadByPageId(existingBullet.Id).FirstOrDefault();
			Assert.That(deletedBullet, Is.Null, "The bullet should no longer exist in the database.");
		}

		[Test]
		[Description("Tests reading and deleting bullets by page id.")]
		public void CrudByPage_Success()
		{
			// Arrange
			var targetPage = Models.CreatePage();
			PageRepository.Create(targetPage);
			var noisePage = Models.CreatePage();
			PageRepository.Create(noisePage);

			var targetBullet1 = Models.CreateBullet(targetPage.Id);
			BulletRepository.Create(targetBullet1);
			var targetBullet2 = Models.CreateBullet(targetPage.Id);
			BulletRepository.Create(targetBullet2);
			var noiseBullet = Models.CreateBullet(noisePage.Id);
			BulletRepository.Create(noiseBullet);

			// Act
			var existingTargetBullets = BulletRepository.ReadByPageId(targetPage.Id);
			BulletRepository.DeleteByPageId(targetPage.Id);
			var deletedTargetBullets = BulletRepository.ReadByPageId(targetPage.Id);
			var noiseBullets = BulletRepository.ReadByPageId(noisePage.Id);

			// Assert
			Assert.AreEqual(2, existingTargetBullets.Count(), "Two bullets should exist before they are deleted.");
			Assert.IsNotNull(existingTargetBullets.Where(b => b.Id == targetBullet1.Id).FirstOrDefault(), "The first target bullet should be included in the bullets read.");
			Assert.IsNotNull(existingTargetBullets.Where(b => b.Id == targetBullet2.Id).FirstOrDefault(), "The second target bullet should be included in the bullets read.");
			Assert.AreEqual(0, deletedTargetBullets.Count(), "Zero bullets should exist after they are deleted.");
			Assert.AreEqual(1, noiseBullets.Count(), "One noise bullet should still exist in the database.");
			Assert.IsNotNull(noiseBullets.Where(b => b.Id == noiseBullet.Id).FirstOrDefault(), "The noise bullet should exist in the noise bullets read.");
		}
		
		#endregion
	}
}
