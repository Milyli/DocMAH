using DocMAH.Data.Sql;

namespace DocMAH.Data.Sql
{
	public class SqlConfigurationService : IConfigurationService
	{
		#region Constructors

		public SqlConfigurationService()
			: this(new SqlDataStore())
		{

		}

		public SqlConfigurationService(IDataStore dataStore)
		{
			_dataStore = dataStore;
		}

		#endregion

		#region Constants

		public const string DatabaseSchemaVersionKey = "DatabaseSchemaVersion";
		public const string DatabaseHelpVersionKey = "DatabaseHelpVersion";

		#endregion

		#region Private Fields

		private IDataStore _dataStore;

		#endregion

		#region Public Properties

		public int DatabaseHelpVersion
		{
			get { return _dataStore.Configuration_Read(DatabaseHelpVersionKey); }
			set { _dataStore.Configuration_Update(DatabaseHelpVersionKey, value); }
		}

		public int DatabaseSchemaVersion
		{
			get { return _dataStore.Configuration_Read(DatabaseSchemaVersionKey); }
			set { _dataStore.Configuration_Update(DatabaseSchemaVersionKey, value); }
		}

		#endregion
	}
}
