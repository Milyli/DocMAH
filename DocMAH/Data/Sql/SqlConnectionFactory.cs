using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using DocMAH.Configuration;

namespace DocMAH.Data.Sql
{
	public class SqlConnectionFactory : ISqlConnectionFactory
	{
		#region Constructors

		public SqlConnectionFactory(IContentConfiguration contentConfiguration)
		{
			_contentConfiguration = contentConfiguration;
		}

		#endregion

		#region Private Fields

		private readonly IContentConfiguration _contentConfiguration;

		#endregion

		#region Public Methods
		
		public SqlConnection GetConnection(string initialCatalog = null)
		{
			string connectionString = Configurator.ConnectionString;
			if (string.IsNullOrEmpty(connectionString))
			{
				string connectionStringName = _contentConfiguration.ConnectionStringName;
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

		#endregion
	}
}
