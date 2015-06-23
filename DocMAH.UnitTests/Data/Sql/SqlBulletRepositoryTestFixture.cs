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

		[Test]
		[Description("Prevent SQL exceptions when list is empty.")]
		public void DeleteExcept_EmptyList()
		{
			// Arrange			

			// Act
			BulletRepository.DeleteExcept(new List<int>());

			// Assert
			// Above call should not blow up with an empty list.
		}

		[Test]
		[Description("Deletes bullets except for those with listed ids.")]
		public void DeleteExcept_Success()
		{
			// Arrange
			var keptPage = Models.CreatePage();
			PageRepository.Create(keptPage);
			var keptBullet = Models.CreateBullet(keptPage.Id);
			BulletRepository.Create(keptBullet);

			var deletedPage = Models.CreatePage();
			PageRepository.Create(deletedPage);
			var deletedBullet = Models.CreateBullet(deletedPage.Id);
			BulletRepository.Create(deletedBullet);
			var anotherDeletedBullet = Models.CreateBullet(deletedPage.Id);
			BulletRepository.Create(anotherDeletedBullet);

			// Act
			BulletRepository.DeleteExcept(new List<int> { keptBullet.Id });

			// Assert
			var bullets = BulletRepository.ReadAll().ToList();
			Assert.That(bullets.Count, Is.EqualTo(1), "Exactly one bullet should be left in the repository.");
			Assert.That(bullets[0].Id, Is.EqualTo(keptBullet.Id), "The remaining bullet should be the one associated with the undeleted page.");
		}

		[Test]
		[Description("Imports an existing bullet to the Sql data store.")]
		public void Import_ExistingBullet()
		{
			// Arrange
			var page = Models.CreatePage();
			PageRepository.Create(page);
			var bullet = Models.CreateBullet(page.Id);
			BulletRepository.Create(bullet);
			
			// Modify the bullet informatio nto verify the values in the data store are overwritten.
			bullet.Number = 10;
			bullet.Text = "Import unit test bullet text.";
			bullet.VerticalOffset = 1089;


			// Act
			BulletRepository.Import(bullet);
			var results = BulletRepository.ReadByPageId(page.Id);

			// Assert
			Assert.That(results, Is.Not.Null, "The bullet should still exist in the data store.");
			Assert.That(results.Count, Is.EqualTo(1), "There should be exactly one bullet associated with the page.");
			var result = results.First();
			Assert.That(result.Id, Is.EqualTo(bullet.Id), "The id should remain the same.");
			Assert.That(result.Text, Is.EqualTo(bullet.Text), "The bullet text should have been updated in the data store.");
			Assert.That(result.VerticalOffset, Is.EqualTo(bullet.VerticalOffset), "The vertical offset should have been updated in the data store.");
		}

		[Test]
		[Description("Imports a new bullet to the SQL data store.")]
		public void Import_NewBullet()
		{
			// Arrange
			var page = Models.CreatePage();
			PageRepository.Create(page);
			var bullet = Models.CreateBullet(page.Id);
			bullet.Id = 42098;			

			// Act
			BulletRepository.Import(bullet);
			var results = BulletRepository.ReadByPageId(page.Id);

			// Assert
			Assert.That(results, Is.Not.Null, "The bullet should have been added to the data store.");
			Assert.That(results.Count, Is.EqualTo(1), "There should be exactly one bullet associated with the page.");
			var result = results.First();
			Assert.That(result.Id, Is.EqualTo(bullet.Id), "The id should have been created using the input id.");
		}
		
		#endregion
	}
}
