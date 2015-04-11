using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocMAH.Data
{
	public class DatabaseConfiguration : IDatabaseConfiguration
	{
		#region Constructors

		public DatabaseConfiguration()
			: this(new SqlDatabaseAccess())
		{

		}

		public DatabaseConfiguration(IDatabaseAccess databaseAccess)
		{
			_databaseAccess = databaseAccess;
		}

		#endregion

		#region Constants

		public const string DatabaseSchemaVersionKey = "DatabaseSchemaVersion";
		public const string DatabaseHelpVersionKey = "DatabaseHelpVersion";

		#endregion

		#region Private Fields

		private IDatabaseAccess _databaseAccess;

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
