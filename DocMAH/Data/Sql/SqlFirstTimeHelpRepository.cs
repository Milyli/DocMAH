using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using DocMAH.Extensions;
using DocMAH.Models;

namespace DocMAH.Data.Sql
{
	public class SqlFirstTimeHelpRepository : BaseSqlRepository, IFirstTimeHelpRepository
	{
		#region Constructors

		public SqlFirstTimeHelpRepository(ISqlConnectionFactory sqlConnectionFactory)
			: base(sqlConnectionFactory)
		{

		}

		#endregion

		#region Private Methods

		private static void AddParameters(FirstTimeHelp help, SqlCommand command)
		{
			command.Parameters.AddRange(new SqlParameter[] {
				new SqlParameter("@sourceUrl", help.SourceUrl),
				new SqlParameter("@title", help.Title),
				new SqlParameter("@content", help.Content),
				new SqlParameter("@verticalOffset", help.VerticalOffset),
				new SqlParameter("@horizontalOffset", help.HorizontalOffset),
				new SqlParameter("@offsetElementId", help.OffsetElementId),
			});
		}

		private IEnumerable<FirstTimeHelp> HydratePages(SqlDataReader reader)
		{
			var idOrdinal = reader.GetOrdinal("Id");
			var sourceUrlOrdinal = reader.GetOrdinal("SourceUrl");
			var titleOrdinal = reader.GetOrdinal("Title");
			var contentOrdinal = reader.GetOrdinal("Content");
			var verticalOffsetOrdinal = reader.GetOrdinal("VerticalOffset");
			var horizontalOffsetOrdinal = reader.GetOrdinal("HorizontalOffset");
			var offsetElementIdOrdinal = reader.GetOrdinal("OffsetElementId");

			while (reader.Read())
			{
				var result = new FirstTimeHelp
				{
					Id = reader.GetInt32(idOrdinal),
					SourceUrl = reader.GetString(sourceUrlOrdinal),
					Title = reader.GetString(titleOrdinal),
					Content = reader.GetString(contentOrdinal),
					VerticalOffset = reader.GetInt32(verticalOffsetOrdinal),
					HorizontalOffset = reader.GetInt32(horizontalOffsetOrdinal),
					OffsetElementId = reader.GetString(offsetElementIdOrdinal),
				};

				result.MatchUrls = MatchUrls_ReadByHelpId(result.Id);
				yield return result;
			}
		}

		private void MatchUrls_CreateByHelpId(int helpId, string matchUrls)
		{
			if (!string.IsNullOrEmpty(matchUrls))
			{
				var pageUrls = matchUrls.Trim().Split(' ');

				using (var connection = SqlConnectionFactory.GetConnection())
				{
					connection.Open();
					foreach (var pageUrl in pageUrls)
					{
						using (var command = connection.CreateCommand())
						{
							command.CommandType = CommandType.Text;
							command.CommandText = SqlScripts.PageUrl_Create;
							command.Parameters.Add(new SqlParameter("@url", pageUrl.Replace('*', '%')));
							command.Parameters.Add(new SqlParameter("@pageId", helpId));

							command.ExecuteNonQuery();
						}
					}
				}
			}
		}

		private void MatchUrls_DeleteByHelpId(int helpId)
		{
			using (var connection = SqlConnectionFactory.GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.PageUrl_DeleteByPageId;
				command.Parameters.Add(new SqlParameter("@pageId", helpId));

				connection.Open();
				command.ExecuteNonQuery();
			}
		}

		private string MatchUrls_ReadByHelpId(int helpId)
		{
			var result = new StringBuilder();

			using (var connection = SqlConnectionFactory.GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.PageUrl_ReadByPageId;
				command.Parameters.Add(new SqlParameter("@pageId", helpId));

				connection.Open();
				using (var reader = command.ExecuteReader())
				{
					var urlOrdinal = reader.GetOrdinal("Url");

					while (reader.Read())
					{
						result.Append(reader.GetString(urlOrdinal));
						result.Append(' ');
					}
				}
			}

			return result.ToString().Trim().Replace('%', '*');
		}

		#endregion

		#region Public Methods

		public void Create(FirstTimeHelp help)
		{
			using (var connection = SqlConnectionFactory.GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.FirstTimeHelp_Create;
				AddParameters(help, command);

				connection.Open();
				help.Id = (int)command.ExecuteScalar();
			}

			MatchUrls_CreateByHelpId(help.Id, help.MatchUrls);
		}

		public void Delete(int id)
		{
			MatchUrls_DeleteByHelpId(id);

			using (var connection = SqlConnectionFactory.GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.FirstTimeHelp_Delete;
				command.Parameters.Add(new SqlParameter("@id", id));

				connection.Open();
				command.ExecuteNonQuery();
			}
		}

		public void DeleteExcept(List<int> helpIds)
		{
			if (null == helpIds || helpIds.Count == 0)
				return;

			using (var connection = SqlConnectionFactory.GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.FirstTimeHelp_DeleteExcept.Replace("@pageIds", helpIds.ToCsv());

				connection.Open();
				command.ExecuteNonQuery();
			}
		}

		public void Import(FirstTimeHelp help)
		{
			using (var connection = SqlConnectionFactory.GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.FirstTimeHelp_Import;
				AddParameters(help, command);
				command.Parameters.Add(new SqlParameter("@id", help.Id));

				connection.Open();
				command.ExecuteNonQuery();
			}

			MatchUrls_DeleteByHelpId(help.Id);
			MatchUrls_CreateByHelpId(help.Id, help.MatchUrls);
		}

		public FirstTimeHelp Read(int id)
		{
			FirstTimeHelp result = null;

			using (var connection = SqlConnectionFactory.GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.FirstTimeHelp_ReadById;
				command.Parameters.Add(new SqlParameter("@id", id));

				connection.Open();
				using (var reader = command.ExecuteReader())
				{
					result = HydratePages(reader).FirstOrDefault();
				}
			}

			return result;
		}

		public IEnumerable<FirstTimeHelp> ReadAll()
		{
			using (var connection = SqlConnectionFactory.GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.FirstTimeHelp_ReadAll;

				connection.Open();
				var reader = command.ExecuteReader();
				foreach (var help in HydratePages(reader))
				{
					yield return help;
				}
			}
		}

		public FirstTimeHelp ReadByUrl(string url)
		{
			FirstTimeHelp result = null;

			using (var connection = SqlConnectionFactory.GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.FirstTimeHelp_ReadByUrl;
				command.Parameters.Add(new SqlParameter("@url", url));

				connection.Open();
				using (var reader = command.ExecuteReader())
				{
					result = HydratePages(reader).FirstOrDefault();
				}
			}

			return result;
		}

		public void Update(FirstTimeHelp help)
		{
			using (var connection = SqlConnectionFactory.GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.FirstTimeHelp_Update;
				AddParameters(help, command);
				command.Parameters.Add(new SqlParameter("@id", help.Id));

				connection.Open();
				command.ExecuteNonQuery();
			}

			MatchUrls_DeleteByHelpId(help.Id);
			MatchUrls_CreateByHelpId(help.Id, help.MatchUrls);
		}

		#endregion
	}
}
