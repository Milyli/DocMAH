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
		void Create();

		/// <summary>
		/// Deletes a data store.
		/// Should only be used in unit tests.
		/// </summary>
		void Delete();

		/// <summary>
		/// Updates the data store to the version used by this version of the DocMAH library.
		/// </summary>
		void Update();

	}
}
