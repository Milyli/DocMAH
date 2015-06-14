using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocMAH.Configuration;
using System.Data;
using System.Configuration;
using DocMAH.Models;
using System.Web.Routing;
using System.Web;
using DocMAH.Properties;
using System.IO;

namespace DocMAH.Data
{
	public interface IDataStore
	{
		int Configuration_Read(string name);
		void Configuration_Update(string name, int value);

		/// <summary>
		/// Creates a new DocMAH data store if one does not exist.
		/// </summary>
		void DataStore_Create();

		/// <summary>
		/// Deletes a data store.
		/// Should only be used in unit tests.
		/// </summary>
		void DataStore_Drop();

		
		//void DataStore_Update();
		void Database_RunScript(string sql);
		void Database_Update();
		void Page_Create(Page page);
		void Page_Delete(int id);
		IEnumerable<Page> Page_ReadAll();
		Page Page_ReadById(int id);
		List<Page> Page_ReadByParentId(int? parentId);
		Page Page_ReadByUrl(string url);
		List<Page> Page_ReadTableOfContents(bool includeHidden);
		void Page_Update(Page page);
		void UserPageSettings_Create(UserPageSettings userPageSettings);
		UserPageSettings UserPageSettings_ReadByUserAndPage(string userName, int pageId);
		void UserPageSettings_Update(UserPageSettings userPageSettings);
	}
}
