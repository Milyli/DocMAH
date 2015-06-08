using DocMAH.Data.Sql;

namespace DocMAH.Data
{
	public class DatabaseConfiguration : IDatabaseConfiguration
	{
		#region Constructors

		public DatabaseConfiguration()
			: this(new SqlDataStore())
		{

		}

		public DatabaseConfiguration(IDataStore databaseAccess)
		{
			_databaseAccess = databaseAccess;
		}

		#endregion

		#region Constants

		public const string DatabaseSchemaVersionKey = "DatabaseSchemaVersion";
		public const string DatabaseHelpVersionKey = "DatabaseHelpVersion";

		#endregion

		#region Private Fields

		private IDataStore _databaseAccess;

		#endregion

		#region Public Properties

		public int DatabaseHelpVersion
		{
			get { return _databaseAccess.Configuration_Read(DatabaseHelpVersionKey); }
			set { _databaseAccess.Configuration_Update(DatabaseHelpVersionKey, value); }
		}

		public int DatabaseSchemaVersion
		{
			get { return _databaseAccess.Configuration_Read(DatabaseSchemaVersionKey); }
			set { _databaseAccess.Configuration_Update(DatabaseSchemaVersionKey, value); }
		}

		#endregion
	}
}
