using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using DocMAH.Models;

namespace DocMAH.Data.Sql
{
	public class SqlUserPageSettingsRepository : BaseSqlRepository, IUserPageSettingsRepository
	{
		#region Constructors
		
		public SqlUserPageSettingsRepository(ISqlConnectionFactory sqlConnectionFactory)
			: base(sqlConnectionFactory)
		{

		}	

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

		private static void AddParameters(UserPageSettings userPageSettings, SqlCommand command)
		{
			command.Parameters.AddRange(new SqlParameter[]{
				new SqlParameter("@pageId", userPageSettings.PageId),
				new SqlParameter("@hidePage", userPageSettings.HidePage),
				new SqlParameter("@userName", userPageSettings.UserName),	
			});
		}


		#endregion

		#region Public Methods

		public void Create(UserPageSettings userPageSettings)
		{
			using (var connection = SqlConnectionFactory.GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.UserPageSettings_Create;
				AddParameters(userPageSettings, command);

				connection.Open();
				userPageSettings.Id = (int)command.ExecuteScalar();
			}
		}

		public UserPageSettings Read(string userName, int pageId)
		{
			UserPageSettings result = null;

			using (var connection = SqlConnectionFactory.GetConnection())
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

		public void Update(UserPageSettings userPageSettings)
		{
			using (var connection = SqlConnectionFactory.GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.UserPageSettings_Update;
				AddParameters(userPageSettings, command);
				command.Parameters.Add(new SqlParameter("@id", userPageSettings.Id));

				connection.Open();
				command.ExecuteNonQuery();

			}
		}


		#endregion
	}
}
