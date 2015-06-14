using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DocMAH.Models;

namespace DocMAH.Data
{
	public interface IBulletRepository
	{
		/// <summary>
		/// Create a new bullet in configured data store.
		/// </summary>
		/// <param name="bullet">Bullet model to persist.</param>
		void Create(Bullet bullet);

		/// <summary>
		/// Delete the bullet with the given id from the data store.
		/// </summary>
		/// <param name="id"></param>
		void Delete(int id);

		/// <summary>
		/// Delete all bullets associated with the given page id from the data store.
		/// This is a one shot delete instead of separate calls for each bullet.
		/// </summary>
		/// <param name="pageId"></param>
		void DeleteByPageId(int pageId);

		/// <summary>
		/// Streams all bullets from the data store.
		/// </summary>
		/// <returns></returns>
		IEnumerable<Bullet> ReadAll();

		/// <summary>
		/// Read all bullets associated with the give page id from the data store.
		/// </summary>
		/// <returns></returns>
		List<Bullet> ReadByPageId(int pageId);

		/// <summary>
		/// Update the given bullet in the data store. 
		/// </summary>
		/// <param name="bullet"></param>
		void Update(Bullet bullet);
	}
}
