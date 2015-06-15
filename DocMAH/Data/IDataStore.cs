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
		/// <summary>
		/// Creates a new DocMAH data store if one does not exist.
		/// </summary>
		void DataStore_Create();

		/// <summary>
		/// Deletes a data store.
		/// Should only be used in unit tests.
		/// </summary>
		void DataStore_Drop();

		/// <summary>
		/// This will be replaced with model specific data store calls when data file is updated.
		/// This will likely be a breaking change.
		/// </summary>
		/// <param name="sql"></param>
		void Database_RunScript(string sql);

		/// <summary>
		/// Updates the data store to the version used by this version of the DocMAH library.
		/// </summary>
		void DataStore_Update();

	}
}
