using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
			IDocumentationPageRepository documentationPpageRepository,
			IFirstTimeHelpRepository firstTimeHelpRepository,
			IUserPageSettingsRepository userPageSettingsRepository)
		{
			_bulletRepository = bulletRepository;
			_dataStoreConfiguration = dataStoreConfiguration;
			_documentationPageRepository = documentationPpageRepository;
			_firstTimeHelpRepository = firstTimeHelpRepository;
			_userPageSettingsRepository = userPageSettingsRepository;
		}

		#endregion

		#region Public Fields

		public const string DocmahInitializedKey = "DocMAH.Initialized";

		#endregion

		#region Private Fields

		private readonly IBulletRepository _bulletRepository;
		private readonly IDataStoreConfiguration _dataStoreConfiguration;
		private readonly IDocumentationPageRepository _documentationPageRepository;
		private readonly IFirstTimeHelpRepository _firstTimeHelpRepository;
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
			var contentFileContentVersion = _dataStoreConfiguration.HelpContentVersion + 1;

			using (var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
			using (var xmlWriter = XmlWriter.Create(stream, settings))
			{
				xmlWriter.WriteStartElement(ContentFileConstants.RootNode);
				xmlWriter.WriteAttributeString(ContentFileConstants.DataStoreSchemaVersionAttribute, _dataStoreConfiguration.DataStoreSchemaVersion.ToString());
				xmlWriter.WriteAttributeString(ContentFileConstants.FileSchemaVersionAttribute, contentFileSchemaVersion.ToString());
				xmlWriter.WriteAttributeString(ContentFileConstants.FileContentVersionAttribute, contentFileContentVersion.ToString());

				xmlWriter.WriteStartElement(ContentFileConstants.DocumentationPageCollectionElement);
				var pageSerializer = new XmlSerializer(typeof(DocumentationPage));
				foreach (var page in _documentationPageRepository.ReadAll())
				{
					pageSerializer.Serialize(xmlWriter, page);
				}
				xmlWriter.WriteEndElement();

				xmlWriter.WriteStartElement(ContentFileConstants.FirstTimeHelpCollectionElement);
				var helpSerializer = new XmlSerializer(typeof(FirstTimeHelp));
				foreach(var help in _firstTimeHelpRepository.ReadAll())
				{
					helpSerializer.Serialize(xmlWriter, help);
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

			_dataStoreConfiguration.HelpContentVersion = contentFileContentVersion;
		}

		public void ImportContent(string fileName)
		{
			if (File.Exists(fileName))
			{
				var contentUpToDate = false;
				var versionsValidated = false;
				var helps = new List<FirstTimeHelp>();
				var pages = new List<DocumentationPage>();
				var bullets = new List<Bullet>();
				int fileDataStoreSchemaVersion = -1;
				int fileSchemaVersion = -1;
				int fileContentVersion = -1;

				using (var xmlReader = new XmlTextReader(fileName))
				{
					var helpSerializer = new XmlSerializer(typeof(FirstTimeHelp));
					var pageSerializer = new XmlSerializer(typeof(DocumentationPage));
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
							else if (xmlReader.LocalName == typeof(FirstTimeHelp).Name)
							{
								if (!versionsValidated)
									throw new InvalidOperationException("Attempted to import first time help without version validation.");

								var help = (FirstTimeHelp)helpSerializer.Deserialize(xmlReader);
								helps.Add(help);
							}
							else if (xmlReader.LocalName == typeof(DocumentationPage).Name)
							{
								// Deserialize Page elements and import to data store.
								if (!versionsValidated)
									throw new InvalidOperationException("Attempted to import documentation pages without version validation.");

								var page = (DocumentationPage)pageSerializer.Deserialize(xmlReader);
								pages.Add(page);
							}
							else if (xmlReader.LocalName == typeof(Bullet).Name)
							{
								// Deserialize Bullet elements and import to data store.
								if (!versionsValidated)
									throw new InvalidOperationException("Attempted to import bullets without version validation.");

								var bullet = (Bullet)bulletSerializer.Deserialize(xmlReader);
								bullets.Add(bullet);
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

					var bulletIds = bullets.Select(b => b.Id).ToList();
					var helpIds = helps.Select(h => h.Id).ToList();
					var pageIds = pages.Select(p => p.Id).ToList();

					_bulletRepository.DeleteExcept(bulletIds);
					_userPageSettingsRepository.DeleteExcept(pageIds);
					_firstTimeHelpRepository.DeleteExcept(helpIds);
					_documentationPageRepository.DeleteExcept(pageIds);

					helps.ForEach(h => _firstTimeHelpRepository.Import(h)); // Pages must be imported after deleting old ones so that page URLs do not conflict.
					bullets.ForEach(b => _bulletRepository.Import(b));
					pages.ForEach(p => _documentationPageRepository.Import(p)); 

					_dataStoreConfiguration.HelpContentVersion = fileContentVersion;
				}
			}
		}
		#endregion
	}
}
