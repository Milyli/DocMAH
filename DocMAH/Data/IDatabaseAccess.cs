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
	public interface IDatabaseAccess
	{
		void Bullet_Create(Bullet bullet);
		void Bullet_Delete(int id);
		IEnumerable<Bullet> Bullet_ReadAll();
		void Bullet_DeleteByPageId(int pageId);
		List<Bullet> Bullet_ReadByPageId(int pageId);
		void Bullet_Update(Bullet bullet);
		int Configuration_Read(string name);
		void Configuration_Update(string name, int value);
		void Database_RunScript(string sql);
		void Database_Update(DatabaseVersions toVersion);
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
