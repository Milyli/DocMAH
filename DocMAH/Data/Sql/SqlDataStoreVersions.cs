using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocMAH.Data.Sql
{
	public enum SqlDataStoreVersions : int
	{
		/// <summary>
		/// First published database schema.
		/// </summary>
		Database_01 = 1,

		/// <summary>
		/// Database changes:
		/// 1) Changed name of DatabaseHelpVersion configuration value to HelpContentVersion.
		/// </summary>
		Database_02 = 2, 
	}
}
