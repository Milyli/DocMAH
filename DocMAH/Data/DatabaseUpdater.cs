using System;
using System.IO;
using System.Web;
using System.Xml;
using DocMAH.Data.Sql;

namespace DocMAH.Data
{
	public class DatabaseUpdater : IDataStoreUpdater
	{
		#region Constructors

		public DatabaseUpdater()
			: this(new HttpContextWrapper(HttpContext.Current))
		{
		}

		public DatabaseUpdater(HttpContextBase httpContext)
			: this(httpContext, new SqlDataStore(), new DatabaseConfiguration())
		{
		}

		public DatabaseUpdater(HttpContextBase httpContext, IDataStore databaseAccess, IDatabaseConfiguration databaseConfiguration)
		{
			_databaseAccess = databaseAccess;
			_databaseConfiguration = databaseConfiguration;
			_httpContext = httpContext;
		}

		#endregion

		#region Private Fields

		private IDataStore _databaseAccess;
		private IDatabaseConfiguration _databaseConfiguration;
		private HttpContextBase _httpContext;

		#endregion

		#region Public Methods

		public void Update()
		{
			if (!(bool)(_httpContext.Application["DMH.Initialized"] ?? false))
			{
				var databaseVersions = Enum.GetValues(typeof(DatabaseVersions));

				// Run each database script higher than the current version in order.
				foreach (DatabaseVersions databaseVersion in databaseVersions)
				{
					if (_databaseConfiguration.DatabaseSchemaVersion < (int)databaseVersion)
					{
						_databaseAccess.Database_Update(databaseVersion);
					}
				}

				// Find help installation script. Read and execute it if it exists.
				var versionChecks = false;

				var fileName = Path.Combine(_httpContext.Server.MapPath("~"), "ApplicationHelpInstall.xml");
				if (File.Exists(fileName))
				{
					using (var xmlReader = new XmlTextReader(fileName))
					{
						while (xmlReader.Read())
						{
							if (xmlReader.NodeType == XmlNodeType.Element)
							{
								if (xmlReader.LocalName == XmlNodeNames.UpdateScriptsElement)
								{
									var fileSchemaVersion = int.Parse(xmlReader.GetAttribute(XmlNodeNames.FileSchemaVersionAttribute));
									if (fileSchemaVersion > _databaseConfiguration.DatabaseSchemaVersion)
										throw new InvalidOperationException(string.Format(
											"Unable to update help content. Current database version is {0}. Help install script generated for schema version {1}.",
											_databaseConfiguration.DatabaseSchemaVersion,
											fileSchemaVersion));

									var fileHelpVersion = int.Parse(xmlReader.GetAttribute(XmlNodeNames.FileHelpVersionAttribute));
									if (fileHelpVersion <= _databaseConfiguration.DatabaseHelpVersion)
									{
										break;	// Database is already up to date.
									}

									versionChecks = true;
								}
								else if (xmlReader.LocalName == XmlNodeNames.UpdateScriptElement)
								{
									if (!versionChecks)
										throw new InvalidOperationException("Cannot execute help upgrade scripts. Schema and/or help version checks did not pass.");
									var sql = xmlReader.ReadElementContentAsString();
									_databaseAccess.Database_RunScript(sql);
								}
							}
						}

						xmlReader.Close();
					}
				}

				_httpContext.Application["DMH.Initialized"] = true;
			}
		}

		#endregion
	}
}
