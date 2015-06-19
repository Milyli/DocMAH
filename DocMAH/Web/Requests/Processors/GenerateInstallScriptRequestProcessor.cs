using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using DocMAH.Data;
using DocMAH.Data.Sql;
using DocMAH.Extensions;
using System.Net;
namespace DocMAH.Web.Requests.Processors
{
	public class GenerateInstallScriptRequestProcessor : IRequestProcessor
	{
		#region Constructors

		public GenerateInstallScriptRequestProcessor(
			IBulletRepository bulletRepository,
			IConfigurationService configurationService,
			HttpContextBase httpContext,
			IPageRepository pageRepository)
		{
			_bulletRepository = bulletRepository;
			_configurationService = configurationService;
			_httpContext = httpContext;
			_pageRepository = pageRepository;
		}

		#endregion

		#region Private Fields

		private readonly IBulletRepository _bulletRepository;
		private readonly IConfigurationService _configurationService;
		private readonly HttpContextBase _httpContext;
		private readonly IPageRepository _pageRepository;

		#endregion

		#region IRequestProcessor Members

		public ResponseState Process(string data)
		{
			// There is an open task to make this path configurable.
			var fileName = Path.Combine(_httpContext.Server.MapPath("~"), "ApplicationHelpInstall.xml");

			var settings = new XmlWriterSettings()
			{
				Indent = true,
				Encoding = Encoding.UTF8,
				NewLineChars = Environment.NewLine,
				IndentChars = "\t"
			};

			using (var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
			using (var xmlWriter = XmlWriter.Create(stream, settings))
			{
				xmlWriter.WriteStartElement(XmlNodeNames.UpdateScriptsElement);
				xmlWriter.WriteAttributeString(XmlNodeNames.FileSchemaVersionAttribute, _configurationService.DataStoreSchemaVersion.ToString());

				var nextHelpVersion = _configurationService.HelpContentVersion + 1;
				xmlWriter.WriteAttributeString(XmlNodeNames.FileHelpVersionAttribute, nextHelpVersion.ToString());

				var existingPageIds = new List<int>();
				var pageUrlScripts = new List<string>();
				foreach (var page in _pageRepository.ReadAll())
				{
					// Create insert or update statements for all existing pages.
					xmlWriter.WriteElementString(
						XmlNodeNames.UpdateScriptElement,
						string.Format(SqlScripts.Page_Install,
							page.Id,
							(int)page.PageType,
							page.ParentPageId.ToNullableSqlValue(),
							page.Order,
							page.SourceUrl.ToNullableSqlValue(),
							page.Title.ToSqlValue(),
							page.Content.ToSqlValue(),
							page.VerticalOffset.ToNullableSqlValue(),
							page.HorizontalOffset.ToNullableSqlValue(),
							page.OffsetElementId.ToNullableSqlValue(),
							page.DocImageUrl.ToNullableSqlValue(),
							page.DocVerticalOffset.ToNullableSqlValue(),
							page.DocHorizontalOffset.ToNullableSqlValue(),
							page.IsHidden ? "1" : "0"
						)
					);
					existingPageIds.Add(page.Id);

					// Add existing match URL insert statements to list to be written later.
					foreach (var matchUrl in page.MatchUrls.Split(' '))
					{
						if (!string.IsNullOrEmpty(matchUrl))
							pageUrlScripts.Add(
								string.Format("INSERT DocmahPageUrls VALUES ({0},{1});{2}",
								matchUrl.ToNullableSqlValue().Replace('*', '%'),
								page.Id,
								Environment.NewLine
							));
					}
				}

				// Delete bullets for all deleted pages.
				if (existingPageIds.Count > 0)
				{
					xmlWriter.WriteElementString(
						XmlNodeNames.UpdateScriptElement,
						string.Format("DELETE DocmahBullets WHERE PageId NOT IN({0});{1}", existingPageIds.ToCsv(), Environment.NewLine)
					);
				}

				// Delete all page urls as all will be recreated.
				xmlWriter.WriteElementString(
					XmlNodeNames.UpdateScriptElement,
					string.Format("DELETE DocmahPageUrls;{0}", Environment.NewLine)
				);

				// Delete user settings for all deleted pages. Otherwise, leave them alone.
				if (existingPageIds.Count > 0)
				{
					xmlWriter.WriteElementString(
						XmlNodeNames.UpdateScriptElement,
						string.Format("DELETE DocmahUserPageSettings WHERE PageId NOT IN({0});{1}", existingPageIds.ToCsv(), Environment.NewLine)
					);
				}

				if (existingPageIds.Count > 0)
				{
					// Delete all deleted pages.
					xmlWriter.WriteElementString(
						XmlNodeNames.UpdateScriptElement,
						string.Format("DELETE DocmahPages WHERE Id NOT IN({0});{1}", existingPageIds.ToCsv(), Environment.NewLine)
					);
				}

				// Create insert statements for all page urls.
				foreach (var script in pageUrlScripts)
					xmlWriter.WriteElementString(XmlNodeNames.UpdateScriptElement, script);

				// Create insert or update statements for bullets.
				var existingBulletIds = new List<int>();
				foreach (var bullet in _bulletRepository.ReadAll())
				{
					xmlWriter.WriteElementString(
						XmlNodeNames.UpdateScriptElement,
						string.Format(
							SqlScripts.Bullet_Install,
							bullet.Id,
							bullet.PageId,
							bullet.Number,
							bullet.Text.ToSqlValue(),
							bullet.VerticalOffset,
							bullet.HorizontalOffset,
							bullet.OffsetElementId.ToSqlValue(),
							bullet.DocVerticalOffset.ToNullableSqlValue(),
							bullet.DocHorizontalOffset.ToNullableSqlValue()
						)
					);
					existingBulletIds.Add(bullet.Id);
				}

				// Delete any bullets that were deleted for existing pages.
				if (existingBulletIds.Count > 0)
				{
					xmlWriter.WriteElementString(
						XmlNodeNames.UpdateScriptElement,
						string.Format("DELETE DocmahBullets WHERE Id NOT IN({0});{1}", existingBulletIds.ToCsv(), Environment.NewLine)
					);
				}

				// Update help version
				xmlWriter.WriteElementString(
					XmlNodeNames.UpdateScriptElement,
					string.Format("UPDATE DocmahConfiguration SET [Value] = {0} WHERE [Name] = '{1}';{2}", nextHelpVersion, ConfigurationService.HelpContentVersionKey, Environment.NewLine)
				);

				xmlWriter.Flush();
				xmlWriter.Close();
				stream.Close();
			}
			
			// Return results.
			return new ResponseState
			{
				Content = File.ReadAllText(fileName),
				ContentType = ContentTypes.Text,
				Disposition = "attachment;filename=ApplicationHelpInstall.xml"
			};
		}

		public string RequestType
		{
			get { return RequestTypes.GenerateInstallScript; }
		}

		public bool RequiresEditAuthorization
		{
			get { return true; }
		}

		#endregion
	}
}
