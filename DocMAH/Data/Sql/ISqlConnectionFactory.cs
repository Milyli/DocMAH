using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace DocMAH.Data.Sql
{
	public interface ISqlConnectionFactory
	{
		SqlConnection GetConnection(string initialCatalog = null);
	}
}
