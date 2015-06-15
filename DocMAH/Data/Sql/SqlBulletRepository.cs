using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using DocMAH.Models;

namespace DocMAH.Data.Sql
{
	public class SqlBulletRepository : BaseSqlRepository, IBulletRepository
	{
		#region Constructors

		public SqlBulletRepository()
		{
		}

		public SqlBulletRepository(ISqlConnectionFactory sqlConnectionFactory)
			: base(sqlConnectionFactory)
		{
		}

		#endregion

		#region Private Methods

		private static void AddParameters(Bullet bullet, SqlCommand command)
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

		#endregion

		#region Public Methods

		public void Create(Bullet bullet)
		{
			using (var connection = SqlConnectionFactory.GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.Bullet_Create;
				AddParameters(bullet, command);

				connection.Open();
				bullet.Id = (int)command.ExecuteScalar();
			}
		}

		public void Delete(int id)
		{
			using (var connection = SqlConnectionFactory.GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.Bullet_Delete;
				command.Parameters.Add(new SqlParameter("@id", id));

				connection.Open();
				command.ExecuteNonQuery();
			}
		}

		public IEnumerable<Bullet> ReadAll()
		{
			using (var connection = SqlConnectionFactory.GetConnection())
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

		public void DeleteByPageId(int pageId)
		{
			using (var connection = SqlConnectionFactory.GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.Bullet_DeleteByPageId;
				command.Parameters.Add(new SqlParameter("@pageId", pageId));

				connection.Open();
				command.ExecuteNonQuery();
			}
		}

		public List<Bullet> ReadByPageId(int pageId)
		{
			List<Bullet> result;

			using (var connection = SqlConnectionFactory.GetConnection())
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

		public void Update(Bullet bullet)
		{
			using (var connection = SqlConnectionFactory.GetConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = SqlScripts.Bullet_Update;
				AddParameters(bullet, command);
				command.Parameters.Add(new SqlParameter("@id", bullet.Id));

				connection.Open();
				command.ExecuteNonQuery();
			}
		}

		#endregion
	}
}
