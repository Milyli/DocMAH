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
			_updateScripts = new Dictionary<SqlDataStoreVersions, string>();
			_updateScripts.Add(SqlDataStoreVersions.Database_01, SqlScripts.Database_Update_01);
			_updateScripts.Add(SqlDataStoreVersions.Database_02, SqlScripts.Database_Update_02);
		}

		public SqlDataStore()
			: this(new ConfigurationService(), new SqlConnectionFactory())
		{

		}

		public SqlDataStore(IConfigurationService configurationService, ISqlConnectionFactory connectionFactory)
		{
			_configurationService = configurationService;
			_connectionFactory = connectionFactory;
		}

		#endregion

		#region Private Fields

		private static Dictionary<SqlDataStoreVersions, string> _updateScripts;

		private readonly ISqlConnectionFactory _connectionFactory;
		private readonly IConfigurationService _configurationService;

		#endregion

		#region Private Methods

		private static List<UserPageSettings> HydrateUserPageSettings(SqlDataReader reader)
		{
			var idOrdinal = reader.GetOrdinal("Id");
			var pageIdOrdinal = reader.GetOrdinal("PageId");
			var hidePageOrdinal = reader.GetOrdinal("HidePage");
			var userNameOrdinal = reader.GetOrdinal("UserName");

			var result = new List<UserPageSettings>();

			while (reader.Read())
			{
				result.Add(new UserPageSettings
				{
					Id = reader.GetInt32(idOrdinal),
					PageId = reader.GetInt32(pageIdOrdinal),
					HidePage = reader.GetBoolean(hidePageOrdinal),
					UserName = reader.GetString(userNameOrdinal),
				});
			}

			return result;
		}

		private static void UserPageSettings_AddParameters(UserPageSettings userPageSettings, SqlCommand command)
		{
			command.Parameters.AddRange(new SqlParameter[]{
				new SqlParameter("@pageId", userPageSettings.PageId),
				new SqlParameter("@hidePage", userPageSettings.HidePage),
				new SqlParameter("@userName", userPageSettings.UserName),	
			});
		}

		#endregion

		#region Public Methods

		public void DataStore_Create()
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

		public void DataStore_Drop()
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

		public void DataStore_Update()
		{
			var currentDataStoreVersion = _configurationService.DataStoreSchemaVersion;

			var allDataStoreVersions = Enum.GetValues(typeof(SqlDataStoreVersions));
			using (var connection = _connectionFactory.GetConnection())
			{
				connection.Open();
				foreach (SqlDataStoreVersions dataStoreVersion in allDataStoreVersions)
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

		public void Database_RunScript(string sql)
		{
			using (var connection = _connectionFactory.GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = sql;
				connection.Open();
				command.ExecuteNonQuery();
			}
		}

		public void UserPageSettings_Create(UserPageSettings userPageSettings)
		{
			using (var connection = _connectionFactory.GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.UserPageSettings_Create;
				UserPageSettings_AddParameters(userPageSettings, command);

				connection.Open();
				userPageSettings.Id = (int)command.ExecuteScalar();
			}
		}

		public UserPageSettings UserPageSettings_ReadByUserAndPage(string userName, int pageId)
		{
			UserPageSettings result = null;

			using (var connection = _connectionFactory.GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.UserPageSettings_ReadByUserAndPage;
				command.Parameters.AddRange(new SqlParameter[]{
					new SqlParameter("@userName", userName),
					new SqlParameter("@pageId", pageId)
				});

				connection.Open();
				using (var reader = command.ExecuteReader())
				{
					result = HydrateUserPageSettings(reader).FirstOrDefault();
				}
			}

			return result;
		}

		public void UserPageSettings_Update(UserPageSettings userPageSettings)
		{
			using (var connection = _connectionFactory.GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.UserPageSettings_Update;
				UserPageSettings_AddParameters(userPageSettings, command);
				command.Parameters.Add(new SqlParameter("@id", userPageSettings.Id));

				connection.Open();
				command.ExecuteNonQuery();

			}
		}

		#endregion
	}
}
