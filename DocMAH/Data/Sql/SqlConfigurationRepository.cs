using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace DocMAH.Data.Sql
{
	public class SqlConfigurationRepository : BaseSqlRepository, IConfigurationRepository
	{
		#region Public Methods
		
		public int Read(string name)
		{
			using (var connection = SqlConnectionFactory.GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.Configuration_Read;
				command.Parameters.Add(new SqlParameter("@name", name));
				connection.Open();
				var result = command.ExecuteScalar();
				return (int)result;
			}
		}

		public void Update(string name, int value)
		{
			using (var connection = SqlConnectionFactory.GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.Configuration_Update;
				command.Parameters.Add(new SqlParameter("@name", name));
				command.Parameters.Add(new SqlParameter("@value", value));
				connection.Open();
				command.ExecuteNonQuery();
			}
		}

		#endregion
	}
}
