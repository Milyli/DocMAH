﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DocMAH.Models;

namespace DocMAH.Data
{
	/// <summary>
	/// Manages UserPageSettings entries in the data store.
	/// </summary>
	public interface IUserPageSettingsRepository
	{
		/// <summary>
		/// Creates a new user setting for the specified page in the data store.
		/// </summary>
		/// <param name="userPageSettings"></param>
		void Create(UserPageSettings userPageSettings);

		/// <summary>
		/// Deletes all user page settings for a give page.
		/// Used when deleting pages in the system.
		/// </summary>
		/// <param name="pageId"></param>
		void DeleteByPageId(int pageId);

		/// <summary>
		/// Deletes user page settings for all related pages not included in the list of ids.
		/// </summary>
		/// <param name="pageIds"></param>
		void DeleteExcept(List<int> pageIds);

		/// <summary>
		/// Reads a setting for the given user and page in the data store.
		/// </summary>
		/// <param name="userName"></param>
		/// <param name="pageId"></param>
		/// <returns></returns>
		UserPageSettings Read(string userName, int pageId);

		/// <summary>
		/// Update the given page setting in the data store.
		/// </summary>
		/// <param name="userPageSettings"></param>
		void Update(UserPageSettings userPageSettings);
	}
}
