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
using DocMAH.Data.Sql;

namespace DocMAH.Data.Sql
{
	public class SqlDataStore : IDataStore
	{
		#region Constructors

		static SqlDataStore()
		{
			_updateScripts = new Dictionary<DocMAH.Data.DataStoreSchemaVersions, string>();
			_updateScripts.Add(DocMAH.Data.DataStoreSchemaVersions.Version_01, SqlScripts.Database_Update_01);
			_updateScripts.Add(DocMAH.Data.DataStoreSchemaVersions.Version_02, SqlScripts.Database_Update_02);
		}

		public SqlDataStore(IDataStoreConfiguration dataStoreConfiguration, ISqlConnectionFactory connectionFactory)
		{
			_dataStoreConfiguration = dataStoreConfiguration;
			_connectionFactory = connectionFactory;
		}

		#endregion

		#region Private Fields

		private static Dictionary<DocMAH.Data.DataStoreSchemaVersions, string> _updateScripts;

		private readonly ISqlConnectionFactory _connectionFactory;
		private readonly IDataStoreConfiguration _dataStoreConfiguration;

		#endregion

		#region Private Methods


		#endregion

		#region Public Methods

		public void Create()
		{
			using (var connection = _connectionFactory.GetConnection(initialCatalog: "master"))
			using (var command = connection.CreateCommand())
			{
				var builder = new SqlConnectionStringBuilder { ConnectionString = _connectionFactory.GetConnection().ConnectionString };
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.Database_Create.Replace("CatalogName", builder.InitialCatalog);
				connection.Open();
				command.ExecuteNonQuery();
			}
		}

		public void Delete()
		{
			using (var connection = _connectionFactory.GetConnection(initialCatalog: "master"))
			using (var command = connection.CreateCommand())
			{
				var builder = new SqlConnectionStringBuilder { ConnectionString = _connectionFactory.GetConnection().ConnectionString };
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.Database_Drop.Replace("CatalogName", builder.InitialCatalog);
				connection.Open();
				command.ExecuteNonQuery();
			}
		}

		public void Update()
		{
			var currentDataStoreVersion = _dataStoreConfiguration.DataStoreSchemaVersion;

			var allDataStoreVersions = Enum.GetValues(typeof(DocMAH.Data.DataStoreSchemaVersions));
			using (var connection = _connectionFactory.GetConnection())
			{
				connection.Open();
				foreach (DocMAH.Data.DataStoreSchemaVersions dataStoreVersion in allDataStoreVersions)
				{
					if (currentDataStoreVersion < (int)dataStoreVersion)
					{
						using (var command = connection.CreateCommand())
						{
							command.CommandType = CommandType.Text;
							command.CommandText = _updateScripts[dataStoreVersion];
							command.ExecuteNonQuery();
						}
					}
				}
			}
		}

		#endregion
	}
}
