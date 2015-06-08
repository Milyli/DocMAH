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
			_updateScripts = new Dictionary<DatabaseVersions, string>();
			_updateScripts.Add(DatabaseVersions.Database_01, SqlScripts.Database_Update_01);
		}

		#endregion

		#region Private Fields

		private static Dictionary<DatabaseVersions, string> _updateScripts;

		#endregion

		#region Private Methods

		private static void Bullet_AddParameters(Bullet bullet, SqlCommand command)
		{
			command.Parameters.AddRange(new SqlParameter[] {
						new SqlParameter("@pageId", bullet.PageId),
						new SqlParameter("@number", bullet.Number),
						new SqlParameter("@text", bullet.Text),
						new SqlParameter("@verticalOffset", bullet.VerticalOffset),
						new SqlParameter("@horizontalOffset", bullet.HorizontalOffset),
						new SqlParameter("@offsetElementId", bullet.OffsetElementId),
						new SqlParameter("@docVerticalOffset", (object)bullet.DocVerticalOffset ?? DBNull.Value),
						new SqlParameter("@docHorizontalOffset", (object)bullet.DocHorizontalOffset ?? DBNull.Value),
					});
		}

		private static SqlConnection GetConnection(string initialCatalog = null)
		{
			string connectionString = Configurator.ConnectionString;
			if (string.IsNullOrEmpty(connectionString))
			{
				string connectionStringName = DocmahConfigurationSection.Current.ConnectionStringName;
				try
				{
					connectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
				}
				catch (NullReferenceException ex)
				{
					throw new InvalidOperationException("The DocMAH connection string has not been set. Set the connection string using either the docmah element connectionStringName attribute in web.config, or in DocMAHConfig.Configure and call Configure from Application_Start in Global.asax.", ex);
				}
			}

			// Override configuration values based on parameters.
			if (!string.IsNullOrEmpty(initialCatalog))
			{
				var builder = new SqlConnectionStringBuilder(connectionString);
				builder.InitialCatalog = initialCatalog;
				connectionString = builder.ToString();
			}

			return new SqlConnection(connectionString);
		}

		private static IEnumerable<Bullet> HydrateBullets(SqlDataReader reader)
		{
			var idOrdinal = reader.GetOrdinal("Id");
			var pageIdOrdinal = reader.GetOrdinal("PageId");
			var numberOrdinal = reader.GetOrdinal("Number");
			var textOrdinal = reader.GetOrdinal("Text");
			var verticalOffsetOrdinal = reader.GetOrdinal("VerticalOffset");
			var horizontalOffsetOrdinal = reader.GetOrdinal("HorizontalOffset");
			var offsetElementIdOrdinal = reader.GetOrdinal("OffsetElementId");
			var docVerticalOffsetOrdinal = reader.GetOrdinal("DocVerticalOffset");
			var docHorizontalOffsetOrdinal = reader.GetOrdinal("DocHorizontalOffset");

			while (reader.Read())
			{
				yield return new Bullet
				{
					Id = reader.GetInt32(idOrdinal),
					PageId = reader.GetInt32(pageIdOrdinal),
					Number = reader.GetInt32(numberOrdinal),
					Text = reader.GetString(textOrdinal),
					VerticalOffset = reader.GetInt32(verticalOffsetOrdinal),
					HorizontalOffset = reader.GetInt32(horizontalOffsetOrdinal),
					OffsetElementId = reader.GetString(offsetElementIdOrdinal),
					DocVerticalOffset = reader.IsDBNull(docVerticalOffsetOrdinal) ? (int?)null : reader.GetInt32(docVerticalOffsetOrdinal),
					DocHorizontalOffset = reader.IsDBNull(docHorizontalOffsetOrdinal) ? (int?)null : reader.GetInt32(docHorizontalOffsetOrdinal),
				};
			}
		}

		private static IEnumerable<Page> HydratePages(SqlDataReader reader)
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
				var result =  new Page
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

		private static void MatchUrls_CreateByPageId(int pageId, string matchUrls)
		{
			if (!string.IsNullOrEmpty(matchUrls))
			{
				var pageUrls = matchUrls.Trim().Split(' ');

				using (var connection = GetConnection())
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

		private static void MatchUrls_DeleteByPageId(int pageId)
		{
			using (var connection = GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.PageUrl_DeleteByPageId;
				command.Parameters.Add(new SqlParameter("@pageId", pageId));

				connection.Open();
				command.ExecuteNonQuery();
			}
		}

		private static string MatchUrls_ReadByPageId(int pageId)
		{
			var result = new StringBuilder();

			using (var connection = GetConnection())
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

		public void Bullet_Create(Bullet bullet)
		{
			using (var connection = GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.Bullet_Create;
				Bullet_AddParameters(bullet, command);

				connection.Open();
				bullet.Id = (int)command.ExecuteScalar();
			}
		}

		public void Bullet_Delete(int id)
		{
			using (var connection = GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.Bullet_Delete;
				command.Parameters.Add(new SqlParameter("@id", id));

				connection.Open();
				command.ExecuteNonQuery();
			}
		}

		public IEnumerable<Bullet> Bullet_ReadAll()
		{
			using (var connection = GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.Bullet_ReadAll;

				connection.Open();
				var reader = command.ExecuteReader();
				foreach (var bullet in HydrateBullets(reader))
					yield return bullet;
			}
		}

		public void Bullet_DeleteByPageId(int pageId)
		{
			using (var connection = GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.Bullet_DeleteByPageId;
				command.Parameters.Add(new SqlParameter("@pageId", pageId));

				connection.Open();
				command.ExecuteNonQuery();
			}
		}

		public List<Bullet> Bullet_ReadByPageId(int pageId)
		{
			List<Bullet> result;

			using (var connection = GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.Bullet_ReadByPageId;
				command.Parameters.Add(new SqlParameter("@pageId", pageId));

				connection.Open();
				using (var reader = command.ExecuteReader())
				{
					result = new List<Bullet>(HydrateBullets(reader));
				}
			}

			return result;
		}

		public void Bullet_Update(Bullet bullet)
		{
			using (var connection = GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.Bullet_Update;
				Bullet_AddParameters(bullet, command);
				command.Parameters.Add(new SqlParameter("@id", bullet.Id));

				connection.Open();
				command.ExecuteNonQuery();
			}
		}

		public int Configuration_Read(string name)
		{
			using (var connection = GetConnection())
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

		public void Configuration_Update(string name, int value)
		{
			using (var connection = GetConnection())
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

		public void DataStore_Create()
		{
			using (var connection = GetConnection(initialCatalog: "master"))
			using (var command = connection.CreateCommand())
			{
				var builder = new SqlConnectionStringBuilder { ConnectionString = GetConnection().ConnectionString };
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.Database_Create.Replace("CatalogName", builder.InitialCatalog);
				connection.Open();
				command.ExecuteNonQuery();
			}
		}

		public void DataStore_Delete()
		{
			using (var connection = GetConnection(initialCatalog: "master"))
			using (var command = connection.CreateCommand())
			{
				var builder = new SqlConnectionStringBuilder { ConnectionString = GetConnection().ConnectionString };
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.Database_Drop.Replace("CatalogName", builder.InitialCatalog);
				connection.Open();
				command.ExecuteNonQuery();
			}
		}

		public void Database_Update(DatabaseVersions toVersion)
		{
			using (var connection = GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = _updateScripts[toVersion];
				connection.Open();
				command.ExecuteNonQuery();
			}
		}

		public void Database_RunScript(string sql)
		{
			using (var connection = GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = sql;
				connection.Open();
				command.ExecuteNonQuery();
			}
		}

		public void Page_Create(Page page)
		{
			using (var connection = GetConnection())
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

		public void Page_Delete(int id)
		{
			MatchUrls_DeleteByPageId(id);

			using (var connection = GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.Page_Delete;
				command.Parameters.Add(new SqlParameter("@id", id));

				connection.Open();
				command.ExecuteNonQuery();
			}
		}

		public IEnumerable<Page> Page_ReadAll()
		{
			using (var connection = GetConnection())
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

		public Page Page_ReadById(int id)
		{
			Page result = null;

			using (var connection = GetConnection())
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

		public List<Page> Page_ReadByParentId(int? parentId)
		{
			List<Page> result = null;

			using (var connection = GetConnection())
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

		public Page Page_ReadByUrl(string url)
		{
			Page result = null;

			using (var connection = GetConnection())
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

		public List<Page> Page_ReadTableOfContents(bool includeHidden)
		{
			var result = new List<Page>();

			using (var connection = GetConnection())
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

		public void Page_Update(Page page)
		{
			using (var connection = GetConnection())
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

		public void UserPageSettings_Create(UserPageSettings userPageSettings)
		{
			using (var connection = GetConnection())
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

			using (var connection = GetConnection())
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
			using (var connection = GetConnection())
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
