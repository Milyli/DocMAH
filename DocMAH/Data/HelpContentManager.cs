﻿using System;
using System.IO;
using System.Web;
using System.Xml;
using DocMAH.Data.Sql;

namespace DocMAH.Data
{
	public class HelpContentManager : IHelpContentManager
	{
		#region Constructors

		public HelpContentManager(HttpContextBase httpContext, IDataStore dataStore, IConfigurationService configurationService)
		{
			_dataStore = dataStore;
			_configurationService = configurationService;
			_httpContext = httpContext;
		}

		#endregion

		#region Public Fields

		public const string DocmahInitializedKey = "DocMAH.Initialized";

		#endregion

		#region Private Fields

		private readonly IDataStore _dataStore;
		private readonly IConfigurationService _configurationService; 
		private readonly HttpContextBase _httpContext;

		#endregion

		#region Public Methods

		public void UpdateDataStoreContent()
		{
			if (!(bool)(_httpContext.Application[DocmahInitializedKey] ?? false))
			{
				// TODO: Remove datastore update call from ContentFileManager. Move to Application Start
				// Update data store as needed.
				_dataStore.DataStore_Update();

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
									if (fileSchemaVersion > _configurationService.DataStoreSchemaVersion)
										throw new InvalidOperationException(string.Format(
											"Unable to update help content. Current database version is {0}. Help install script generated for schema version {1}.",
											_configurationService.DataStoreSchemaVersion,
											fileSchemaVersion));

									var fileHelpVersion = int.Parse(xmlReader.GetAttribute(XmlNodeNames.FileHelpVersionAttribute));
									if (fileHelpVersion <= _configurationService.HelpContentVersion)
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
									_dataStore.Database_RunScript(sql);
								}
							}
						}

						xmlReader.Close();
					}
				}

				_httpContext.Application[DocmahInitializedKey] = true;
			}
		}

		#endregion
	}
}