using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocMAH.Data
{
	using DocMAH.Data.Sql;
	public enum DataStoreSchemaVersions : int
	{
		/// <summary>
		/// Do not use.
		/// </summary>
		None = 0,

		/// <summary>
		/// First published database schema.
		/// </summary>
		Version_01 = 1,

		/// <summary>
		/// Database changes:
		/// 1) Changed name of DatabaseHelpVersion configuration value to HelpContentVersion.
		/// </summary>
		Version_02 = 2, 
	}
}
