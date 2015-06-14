using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using DocMAH.Models;

namespace DocMAH.Data.Sql
{
	public class SqlPageRepository : BaseSqlRepository, IPageRepository
	{
		#region Constructors
		
		public SqlPageRepository()
		{

		}
		public SqlPageRepository(ISqlConnectionFactory sqlConnectionFactory)
			: base(sqlConnectionFactory)
		{

		}			
		
		#endregion

		#region Private Methods
		
		private IEnumerable<Page> HydratePages(SqlDataReader reader)
		{
			var idOrdinal = reader.GetOrdinal("Id");
			var pageTypeIdOrdinal = reader.GetOrdinal("PageTypeId");
			var parentPageIdOrdinal = reader.GetOrdinal("ParentPageId");
			var orderOrdinal = reader.GetOrdinal("Order");
			var sourceUrlOrdinal = reader.GetOrdinal("SourceUrl");
			var titleOrdinal = reader.GetOrdinal("Title");
			var contentOrdinal = reader.GetOrdinal("Content");
			var verticalOffsetOrdinal = reader.GetOrdinal("VerticalOffset");
			var horizontalOffsetOrdinal = reader.GetOrdinal("HorizontalOffset");
			var offsetElementIdOrdinal = reader.GetOrdinal("OffsetElementId");
			var docImageUrlOrdinal = reader.GetOrdinal("DocImageUrl");
			var docVerticalOffsetOrdinal = reader.GetOrdinal("DocVerticalOffset");
			var docHorizontalOffsetOrdinal = reader.GetOrdinal("DocHorizontalOffset");
			var isHiddenOrdinal = reader.GetOrdinal("IsHidden");

			while (reader.Read())
			{
				var result = new Page
				{
					Id = reader.GetInt32(idOrdinal),
					PageType = (PageTypes)reader.GetInt32(pageTypeIdOrdinal),
					ParentPageId = reader.IsDBNull(parentPageIdOrdinal) ? (int?)null : reader.GetInt32(parentPageIdOrdinal),
					Order = reader.GetInt32(orderOrdinal),
					SourceUrl = reader.IsDBNull(sourceUrlOrdinal) ? (string)null : reader.GetString(sourceUrlOrdinal),
					Title = reader.GetString(titleOrdinal),
					Content = reader.GetString(contentOrdinal),
					VerticalOffset = reader.IsDBNull(verticalOffsetOrdinal) ? (int?)null : reader.GetInt32(verticalOffsetOrdinal),
					HorizontalOffset = reader.IsDBNull(horizontalOffsetOrdinal) ? (int?)null : reader.GetInt32(horizontalOffsetOrdinal),
					OffsetElementId = reader.IsDBNull(offsetElementIdOrdinal) ? (string)null : reader.GetString(offsetElementIdOrdinal),
					DocImageUrl = reader.IsDBNull(docImageUrlOrdinal) ? (string)null : reader.GetString(docImageUrlOrdinal),
					DocVerticalOffset = reader.IsDBNull(docVerticalOffsetOrdinal) ? (int?)null : reader.GetInt32(docVerticalOffsetOrdinal),
					DocHorizontalOffset = reader.IsDBNull(docHorizontalOffsetOrdinal) ? (int?)null : reader.GetInt32(docHorizontalOffsetOrdinal),
					IsHidden = reader.GetBoolean(isHiddenOrdinal)
				};

				result.MatchUrls = MatchUrls_ReadByPageId(result.Id);
				yield return result;
			}
		}

		private void MatchUrls_CreateByPageId(int pageId, string matchUrls)
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
							command.Parameters.Add(new SqlParameter("@pageId", pageId));

							command.ExecuteNonQuery();
						}
					}
				}
			}
		}

		private void MatchUrls_DeleteByPageId(int pageId)
		{
			using (var connection = SqlConnectionFactory.GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.PageUrl_DeleteByPageId;
				command.Parameters.Add(new SqlParameter("@pageId", pageId));

				connection.Open();
				command.ExecuteNonQuery();
			}
		}

		private string MatchUrls_ReadByPageId(int pageId)
		{
			var result = new StringBuilder();

			using (var connection = SqlConnectionFactory.GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.PageUrl_ReadByPageId;
				command.Parameters.Add(new SqlParameter("@pageId", pageId));

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

		private static void Page_AddParameters(Page page, SqlCommand command)
		{
			command.Parameters.AddRange(new SqlParameter[] {
				new SqlParameter("@pageTypeId", page.PageType),
				new SqlParameter("@parentPageId", (object)page.ParentPageId ?? DBNull.Value),
				new SqlParameter("@order", (object)page.Order ?? DBNull.Value),
				new SqlParameter("@sourceUrl", (object)page.SourceUrl ??DBNull.Value),
				new SqlParameter("@title", page.Title),
				new SqlParameter("@content", page.Content),
				new SqlParameter("@verticalOffset", (object)page.VerticalOffset ?? DBNull.Value),
				new SqlParameter("@horizontalOffset", (object)page.HorizontalOffset ?? DBNull.Value),
				new SqlParameter("@offsetElementId", (object)page.OffsetElementId ?? DBNull.Value),
				new SqlParameter("@docImageUrl", (object)page.DocImageUrl ?? DBNull.Value),
				new SqlParameter("@docVerticalOffset", (object)page.DocVerticalOffset ?? DBNull.Value),
				new SqlParameter("@docHorizontalOffset", (object)page.DocHorizontalOffset ?? DBNull.Value),
				new SqlParameter("@isHidden", page.IsHidden),
			});
		}

		#endregion

		#region Public Methods
		
		public void Create(Page page)
		{
			using (var connection = SqlConnectionFactory.GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.Page_Create;
				Page_AddParameters(page, command);

				connection.Open();
				page.Id = (int)command.ExecuteScalar();
			}

			MatchUrls_CreateByPageId(page.Id, page.MatchUrls);
		}

		public void Delete(int id)
		{
			MatchUrls_DeleteByPageId(id);

			using (var connection = SqlConnectionFactory.GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.Page_Delete;
				command.Parameters.Add(new SqlParameter("@id", id));

				connection.Open();
				command.ExecuteNonQuery();
			}
		}

		public Page Read(int id)
		{
			Page result = null;

			using (var connection = SqlConnectionFactory.GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.Page_ReadById;
				command.Parameters.Add(new SqlParameter("@id", id));

				connection.Open();
				using (var reader = command.ExecuteReader())
				{
					result = HydratePages(reader).FirstOrDefault();
				}
			}

			return result;
		}

		public IEnumerable<Page> ReadAll()
		{
			using (var connection = SqlConnectionFactory.GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.Page_ReadAll;

				connection.Open();
				var reader = command.ExecuteReader();
				foreach (var page in HydratePages(reader))
				{
					yield return page;
				}
			}
		}

		public List<Page> ReadByParentId(int? parentId)
		{
			List<Page> result = null;

			using (var connection = SqlConnectionFactory.GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.Page_ReadByParentId;
				command.Parameters.Add(new SqlParameter("@parentId", (object)parentId ?? DBNull.Value));

				connection.Open();
				using (var reader = command.ExecuteReader())
				{
					result = new List<Page>(HydratePages(reader));
				}
			}

			return result ?? new List<Page>();
		}

		public Page ReadByUrl(string url)
		{
			Page result = null;

			using (var connection = SqlConnectionFactory.GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.Page_ReadByUrl;
				command.Parameters.Add(new SqlParameter("@url", url));

				connection.Open();
				using (var reader = command.ExecuteReader())
				{
					result = HydratePages(reader).FirstOrDefault();
				}
			}

			return result;
		}

		public List<Page> ReadTableOfContents(bool includeHidden)
		{
			var result = new List<Page>();

			using (var connection = SqlConnectionFactory.GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.Page_ReadTableOfContents;
				command.Parameters.Add(new SqlParameter("@includeHidden", includeHidden));

				connection.Open();
				using (var reader = command.ExecuteReader())
				{
					var idOrdinal = reader.GetOrdinal("Id");
					var parentPageIdOrdinal = reader.GetOrdinal("ParentPageId");
					var orderOrdinal = reader.GetOrdinal("Order");
					var titleOrdinal = reader.GetOrdinal("Title");
					var isHiddenOrdinal = reader.GetOrdinal("IsHidden");

					while (reader.Read())
					{
						result.Add(new Page
						{
							Id = reader.GetInt32(idOrdinal),
							ParentPageId = reader.IsDBNull(parentPageIdOrdinal) ? (int?)null : reader.GetInt32(parentPageIdOrdinal),
							Order = reader.GetInt32(orderOrdinal),
							Title = reader.GetString(titleOrdinal),
							IsHidden = reader.GetBoolean(isHiddenOrdinal),
						});
					}
				}
			}

			return result;
		}

		public void Update(Page page)
		{
			using (var connection = SqlConnectionFactory.GetConnection())
			{
				using (var command = connection.CreateCommand())
				{
					command.CommandType = CommandType.Text;
					command.CommandText = SqlScripts.Page_Update;
					Page_AddParameters(page, command);
					command.Parameters.Add(new SqlParameter("@id", page.Id));

					connection.Open();
					command.ExecuteNonQuery();
				}
			}

			MatchUrls_DeleteByPageId(page.Id);
			MatchUrls_CreateByPageId(page.Id, page.MatchUrls);
		}

		#endregion
	}
}
