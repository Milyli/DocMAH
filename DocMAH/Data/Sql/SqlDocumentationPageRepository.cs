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
	public class SqlDocumentationPageRepository : BaseSqlRepository, IDocumentationPageRepository
	{
		#region Constructors

		public SqlDocumentationPageRepository(ISqlConnectionFactory sqlConnectionFactory)
			: base(sqlConnectionFactory)
		{

		}

		#endregion

		#region Private Methods

		private static void AddParameters(DocumentationPage page, SqlCommand command)
		{
			command.Parameters.AddRange(new SqlParameter[] {
				new SqlParameter("@parentPageId", (object)page.ParentPageId ?? DBNull.Value),
				new SqlParameter("@order", (object)page.Order ?? DBNull.Value),
				new SqlParameter("@title", page.Title),
				new SqlParameter("@content", page.Content),
				new SqlParameter("@isHidden", page.IsHidden),
			});
		}

		private IEnumerable<DocumentationPage> HydratePages(SqlDataReader reader)
		{
			var idOrdinal = reader.GetOrdinal("Id");
			var parentPageIdOrdinal = reader.GetOrdinal("ParentPageId");
			var orderOrdinal = reader.GetOrdinal("Order");
			var titleOrdinal = reader.GetOrdinal("Title");
			var contentOrdinal = reader.GetOrdinal("Content");
			var isHiddenOrdinal = reader.GetOrdinal("IsHidden");

			while (reader.Read())
			{
				var result = new DocumentationPage
				{
					Id = reader.GetInt32(idOrdinal),
					ParentPageId = reader.IsDBNull(parentPageIdOrdinal) ? (int?)null : reader.GetInt32(parentPageIdOrdinal),
					Order = reader.GetInt32(orderOrdinal),
					Title = reader.GetString(titleOrdinal),
					Content = reader.GetString(contentOrdinal),
					IsHidden = reader.GetBoolean(isHiddenOrdinal)
				};

				yield return result;
			}
		}

		#endregion

		#region Public Methods

		public void Create(DocumentationPage page)
		{
			using (var connection = SqlConnectionFactory.GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.DocumentationPage_Create;
				AddParameters(page, command);

				connection.Open();
				page.Id = (int)command.ExecuteScalar();
			}
		}

		public void Delete(int id)
		{
			using (var connection = SqlConnectionFactory.GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.DocumentationPage_Delete;
				command.Parameters.Add(new SqlParameter("@id", id));

				connection.Open();
				command.ExecuteNonQuery();
			}
		}

		public void DeleteExcept(List<int> pageIds)
		{
			if (null == pageIds || pageIds.Count == 0)
				return;

			using (var connection = SqlConnectionFactory.GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.DocumentationPage_DeleteExcept.Replace("@pageIds", pageIds.ToCsv());

				connection.Open();
				command.ExecuteNonQuery();
			}
		}

		public void Import(DocumentationPage page)
		{
			using (var connection = SqlConnectionFactory.GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.DocumentationPage_Import;
				AddParameters(page, command);
				command.Parameters.Add(new SqlParameter("@id", page.Id));

				connection.Open();
				command.ExecuteNonQuery();
			}
		}

		public DocumentationPage Read(int id)
		{
			DocumentationPage result = null;

			using (var connection = SqlConnectionFactory.GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.DocumentationPage_ReadById;
				command.Parameters.Add(new SqlParameter("@id", id));

				connection.Open();
				using (var reader = command.ExecuteReader())
				{
					result = HydratePages(reader).FirstOrDefault();
				}
			}

			return result;
		}

		public IEnumerable<DocumentationPage> ReadAll()
		{
			using (var connection = SqlConnectionFactory.GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.DocumentationPage_ReadAll;

				connection.Open();
				var reader = command.ExecuteReader();
				foreach (var page in HydratePages(reader))
				{
					yield return page;
				}
			}
		}

		public List<DocumentationPage> ReadByParentId(int? parentId)
		{
			List<DocumentationPage> result = null;

			using (var connection = SqlConnectionFactory.GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.DocumentationPage_ReadByParentId;
				command.Parameters.Add(new SqlParameter("@parentId", (object)parentId ?? DBNull.Value));

				connection.Open();
				using (var reader = command.ExecuteReader())
				{
					result = new List<DocumentationPage>(HydratePages(reader));
				}
			}

			return result ?? new List<DocumentationPage>();
		}

		public List<DocumentationPage> ReadTableOfContents(bool includeHidden)
		{
			var result = new List<DocumentationPage>();

			using (var connection = SqlConnectionFactory.GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.DocumentationPage_ReadTableOfContents;
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
						result.Add(new DocumentationPage
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

		public void Update(DocumentationPage page)
		{
			using (var connection = SqlConnectionFactory.GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.DocumentationPage_Update;
				AddParameters(page, command);
				command.Parameters.Add(new SqlParameter("@id", page.Id));

				connection.Open();
				command.ExecuteNonQuery();
			}
		}

		#endregion
	}
}
