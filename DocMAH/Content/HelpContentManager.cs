using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using DocMAH.Configuration;
using DocMAH.Data;
using DocMAH.Extensions;
using DocMAH.Models;

namespace DocMAH.Content
{
	public class HelpContentManager : IHelpContentManager
	{
		#region Constructors

		public HelpContentManager(
			IBulletRepository bulletRepository,
			IDataStoreConfiguration dataStoreConfiguration,
			IPageRepository pageRepository,
			IUserPageSettingsRepository userPageSettingsRepository)
		{
			_bulletRepository = bulletRepository;
			_dataStoreConfiguration = dataStoreConfiguration;
			_pageRepository = pageRepository;
			_userPageSettingsRepository = userPageSettingsRepository;
		}

		#endregion

		#region Public Fields

		public const string DocmahInitializedKey = "DocMAH.Initialized";

		#endregion

		#region Private Fields

		private readonly IBulletRepository _bulletRepository;
		private readonly IDataStoreConfiguration _dataStoreConfiguration;
		private readonly IPageRepository _pageRepository;
		private readonly IUserPageSettingsRepository _userPageSettingsRepository;

		#endregion

		#region Public Methods

		public void ExportContent(string fileName)
		{
			var settings = new XmlWriterSettings()
			{
				Indent = true,
				Encoding = Encoding.UTF8,
				NewLineChars = Environment.NewLine,
				IndentChars = "\t"
			};

			var contentFileSchemaVersion = (int)EnumExtensions.GetMaxValue<ContentFileSchemaVersions>();

			using (var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
			using (var xmlWriter = XmlWriter.Create(stream, settings))
			{
				xmlWriter.WriteStartElement(ContentFileConstants.RootNode);
				xmlWriter.WriteAttributeString(ContentFileConstants.DataStoreSchemaVersionAttribute, _dataStoreConfiguration.DataStoreSchemaVersion.ToString());
				xmlWriter.WriteAttributeString(ContentFileConstants.FileSchemaVersionAttribute, contentFileSchemaVersion.ToString());
				xmlWriter.WriteAttributeString(ContentFileConstants.FileContentVersionAttribute, (_dataStoreConfiguration.HelpContentVersion + 1).ToString());

				xmlWriter.WriteStartElement(ContentFileConstants.PageCollectionElement);
				var pageSerializer = new XmlSerializer(typeof(Page));
				foreach (var page in _pageRepository.ReadAll())
				{
					pageSerializer.Serialize(xmlWriter, page);
				}
				xmlWriter.WriteEndElement();

				xmlWriter.WriteStartElement(ContentFileConstants.BulletCollectionElement);
				var bulletSeriallizer = new XmlSerializer(typeof(Bullet));
				foreach (var bullet in _bulletRepository.ReadAll())
				{
					bulletSeriallizer.Serialize(xmlWriter, bullet);
				}
				xmlWriter.WriteEndElement();

				xmlWriter.WriteEndElement();

				xmlWriter.Flush();
				xmlWriter.Close();
				stream.Close();
			}
		}

		public void ImportContent(string fileName)
		{
			if (File.Exists(fileName))
			{
				var contentUpToDate = false;
				var versionsValidated = false;
				var pageIds = new List<int>();
				var bulletIds = new List<int>();
				int fileDataStoreSchemaVersion = -1;
				int fileSchemaVersion = -1;
				int fileContentVersion = -1;

				using (var xmlReader = new XmlTextReader(fileName))
				{
					var pageSerializer = new XmlSerializer(typeof(Page));
					var bulletSerializer = new XmlSerializer(typeof(Bullet));

					while (xmlReader.Read())
					{
						if (xmlReader.NodeType == XmlNodeType.Element)
						{
							if (xmlReader.LocalName == ContentFileConstants.RootNode)
							{
								// Validate schema and content versions.
								fileDataStoreSchemaVersion = int.Parse(xmlReader.GetAttribute(ContentFileConstants.DataStoreSchemaVersionAttribute));
								fileSchemaVersion = int.Parse(xmlReader.GetAttribute(ContentFileConstants.FileSchemaVersionAttribute));
								fileContentVersion = int.Parse(xmlReader.GetAttribute(ContentFileConstants.FileContentVersionAttribute));

								int dataStoreSchemaVersion = _dataStoreConfiguration.DataStoreSchemaVersion;
								if (fileDataStoreSchemaVersion > dataStoreSchemaVersion)
									throw new InvalidOperationException(string.Format(
										"Unable to update help content. Current database version is {0}. Help install script generated for schema version {1}.",
										dataStoreSchemaVersion,
										fileDataStoreSchemaVersion
										));

								if (fileContentVersion <= _dataStoreConfiguration.HelpContentVersion)
								{
									contentUpToDate = true;
									break; // DataStore content is already up to date.
								}

								versionsValidated = true;
							}
							else if (xmlReader.LocalName == "Page")
							{
								// Deserialize Page elements and import to data store.
								if (!versionsValidated)
									throw new InvalidOperationException("Attempted to import pages without version validation.");

								var page = (Page)pageSerializer.Deserialize(xmlReader);
								_pageRepository.Import(page);
								pageIds.Add(page.Id);
							}
							else if (xmlReader.LocalName == "Bullet")
							{
								// Deserialize Bullet elements and import to data store.
								if (!versionsValidated)
									throw new InvalidOperationException("Attempted to import bullets without version validation.");

								var bullet = (Bullet)bulletSerializer.Deserialize(xmlReader);
								_bulletRepository.Import(bullet);
								bulletIds.Add(bullet.Id);
							}
						}
					}

					xmlReader.Close();
				}

				// If the content was not already up to date, clean up deleted content.
				if (!contentUpToDate)
				{
					if (!versionsValidated)
						throw new InvalidOperationException("Attempted to delete old data without version validation.");

					_bulletRepository.DeleteExcept(bulletIds);
					_userPageSettingsRepository.DeleteExcept(pageIds);
					_pageRepository.DeleteExcept(pageIds);
					_dataStoreConfiguration.HelpContentVersion = fileContentVersion;
				}
			}
		}
		#endregion
	}
}
