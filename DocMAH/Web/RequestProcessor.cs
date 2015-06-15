using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Xml;
using DocMAH.Configuration;
using DocMAH.Data;
using DocMAH.Data.Sql;
using DocMAH.Extensions;
using DocMAH.Models;
using DocMAH.Properties;

namespace DocMAH.Web
{
	public class RequestProcessor : IRequestProcessor
	{
		#region Constructors

		public RequestProcessor()
			: this(
				new SqlDataStore(), 
				new ConfigurationService(), 
				new SqlBulletRepository(),
				new SqlPageRepository())
		{

		}

		public RequestProcessor(
			IDataStore dataStore, 
			IConfigurationService databaseConfiguration, 
			IBulletRepository bulletRepository,
			IPageRepository pageRepository)
		{
			_dataStore = dataStore;
			_configurationService = databaseConfiguration;
			_bulletRepository = bulletRepository;
			_pageRepository = pageRepository;
		}

		#endregion

		#region Private Fields

		private const string JavaScriptLinkFormat = "<script src='{0}'></script>";
		private const string CssLinkFormat = "<link href='{0}' rel='stylesheet'/>";

		private readonly IDataStore _dataStore;
		private readonly IConfigurationService _configurationService;

		private readonly IBulletRepository _bulletRepository;
		private readonly IPageRepository _pageRepository;

		#endregion

		#region Private Methods

		/// <summary>
		/// By default (unconfigured jsUrl), creates a script link for the CDN URL provided.
		/// If configured, returns an empty string assuming that the first JavaScript link, 
		/// which does not use this method, has created a link to a bundle with all files.
		/// </summary>
		/// <param name="cdnUrl"></param>
		/// <returns></returns>
		private static string CreateBundledOrDefaultScriptLink(string cdnUrl)
		{
			if (string.IsNullOrEmpty(DocmahConfigurationSection.Current.JsUrl))
				return string.Format(JavaScriptLinkFormat, cdnUrl);
			else
				return string.Empty;
		}

		private static bool HasEditAuthorization(HttpContextBase context)
		{
			var access = new Access(context);
			if (!access.CanEdit)
			{
				context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
				WriteResponse(context, "text/html", "<html><body><h2>Unauthorized</h2></body></html>");
				return false;
			}
			return true;
		}

		private static string ReadPostData(HttpContextBase context)
		{
			var result = string.Empty;
			using (var reader = new StreamReader(context.Request.InputStream))
			{
				result = reader.ReadToEnd();
			}
			return result;
		}

		private static void WriteResponse(HttpContextBase context, string contentType, string content)
		{
			context.Response.Cache.SetNoStore();
			context.Response.ContentType = contentType;
			context.Response.Write(content);
			context.Response.Flush();
			context.Response.End();
		}

		#endregion

		#region Public Methods

		public void ProcessCssRequest(HttpContextBase context)
		{
			WriteResponse(context, "text/css", ResourcesExtensions.Minify(Resources.DocMAHStyles, Resources.DocMAHStyles_min));
		}

		public void ProcessDeletePageRequest(HttpContextBase context)
		{
			if (HasEditAuthorization(context))
			{
				var pageIdString = ReadPostData(context);
				var pageId = int.Parse(pageIdString);

				_bulletRepository.DeleteByPageId(pageId);

				// Get the page so we have the parent id.
				var page = _pageRepository.Read(pageId);

				// Get its siblings and remove the page from the collection.
				var siblings = _pageRepository.ReadByParentId(page.ParentPageId);
				page = siblings.Where(p => p.Id == pageId).First();
				siblings.Remove(page);

				// Insert deleted page's children in order at deleted page's location.
				var children = _pageRepository.ReadByParentId(pageId);
				for (int i = children.Count - 1; i >= 0; i--)
				{
					siblings.Insert(page.Order, children[i]);
				}

				// Update parent id and order on all of deleted page's children and siblings.
				for (int i = page.Order; i < siblings.Count; i++)
				{
					var sibling = siblings[i];
					sibling.ParentPageId = page.ParentPageId;
					sibling.Order = i;
					_pageRepository.Update(sibling);
				}

				// Delete page after all children have been moved.
				_pageRepository.Delete(pageId);
			}
		}

		public void ProcessDocumentationPageRequest(HttpContextBase context)
		{
			var configuration = DocmahConfigurationSection.Current;

			var documentationHtml = ResourcesExtensions.Minify(Resources.Html_Documentation, Resources.Html_Documentation_min);

			documentationHtml = documentationHtml.Replace("[TITLE]", configuration.Documentation.PageTitle);
			var request = context.Request;
			var returnLink = new UriBuilder(request.Url.Scheme, request.Url.Host, request.Url.Port, request.ApplicationPath);
			documentationHtml = documentationHtml.Replace("[RETURNLINK]", returnLink.ToString());

			var cssUrl = DocmahConfigurationSection.Current.CssUrl;
			if (string.IsNullOrEmpty(cssUrl))
				cssUrl = CdnUrls.cssJsTree;
			documentationHtml = documentationHtml.Replace("[JSTREECSS]", string.Format(CssLinkFormat, cssUrl));

			var customCssLink = string.Empty;
			var customCssUrl = DocmahConfigurationSection.Current.Documentation.CustomCss;
			if (!string.IsNullOrEmpty(customCssUrl))
				customCssLink = string.Format(CssLinkFormat, customCssUrl);
			documentationHtml = documentationHtml.Replace("[CUSTOMCSS]", customCssLink);

			var jQueryUrl = configuration.JsUrl;
			if (string.IsNullOrEmpty(jQueryUrl))
				jQueryUrl = CdnUrls.jsJQuery;
			documentationHtml = documentationHtml.Replace("[JQUERYURL]",
				string.Format(JavaScriptLinkFormat, jQueryUrl)
			);

			documentationHtml = documentationHtml.Replace("[JQUERYUIURL]", CreateBundledOrDefaultScriptLink(CdnUrls.jsJQueryUi));

			documentationHtml = documentationHtml.Replace("[JSTREEURL]", CreateBundledOrDefaultScriptLink(CdnUrls.jsJsTree));

			documentationHtml = documentationHtml.Replace("[firstTimeViewHTML]",
				ResourcesExtensions.Minify(Resources.Html_FirstTimeView, Resources.Html_FirstTimeView_min)
			);

			WriteResponse(context, "text/html", documentationHtml);
		}

		/// <summary>
		/// </summary>
		/// <param name="context"></param>
		/// <remarks>
		/// The ordering of the sql statements was planned to leave all current pages in place
		/// so the user settings for the pages is not lost on upgrades.
		/// </remarks>
		public void ProcessGenerateInstallScriptRequest(HttpContextBase context)
		{
			if (HasEditAuthorization(context))
			{
				// There is an open task to make this path configurable.
				var fileName = Path.Combine(context.Server.MapPath("~"), "ApplicationHelpInstall.xml");

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

				// Copy file to response stream for good measure.
				context.Response.Cache.SetNoStore();
				context.Response.ContentType = "text/plain";
				context.Response.AddHeader("Content-Disposition", "attachment;filename=ApplicationHelpInstall.xml");

				using (var fileReader = new StreamReader(fileName))
				{
					while (!fileReader.EndOfStream)
					{
						var line = fileReader.ReadLine();
						context.Response.Write(line);
						context.Response.Write(Environment.NewLine);
						context.Response.Flush();
					}
					fileReader.Close();
				}
				context.Response.End();
			}
		}

		public void ProcessJavaScriptRequest(HttpContextBase context)
		{
			WriteResponse(context, "application/javascript", ResourcesExtensions.Minify(Resources.DocMAHJavaScript, Resources.DocMAHJavaScript_min));
		}

		public void ProcessNotFound(HttpContextBase context)
		{
			context.Response.ContentType = "text/html";
			context.Response.StatusCode = 404;
			context.Response.Write("<html><body>404</body></html>");
			context.Response.Flush();
			context.Response.End();
		}

		public void ProcessReadApplicationSettingsRequest(HttpContextBase context)
		{
			var access = new Access(context);
			var applicationSettings = new ApplicationSettings
			{
				CanEdit = access.CanEdit,
			};

			var serializer = new JavaScriptSerializer();
			var applicationSettingsJson = serializer.Serialize(applicationSettings);

			WriteResponse(context, "application/json", applicationSettingsJson);
		}

		public void ProcessReadPageRequest(HttpContextBase context)
		{
			var id = int.Parse(context.Request["id"]);

			var page = _pageRepository.Read(id);
			page.Bullets = _bulletRepository.ReadByPageId(id);

			var serializer = new JavaScriptSerializer();
			var pageJson = serializer.Serialize(page);

			WriteResponse(context, "application/json", pageJson);
		}

		public void ProcessReadTableOfContentsRequest(HttpContextBase context)
		{
			var access = new Access(context);
			var pages = _pageRepository.ReadTableOfContents(access.CanEdit);

			var serializer = new JavaScriptSerializer();
			var pagesJson = serializer.Serialize(pages);

			WriteResponse(context, "application/json", pagesJson);
		}

		public void ProcessSaveHelpRequest(HttpContextBase context)
		{
			if (HasEditAuthorization(context))
			{
				string jsonString = ReadPostData(context);

				var jsonSerializer = new JavaScriptSerializer();
				var page = jsonSerializer.Deserialize<Page>(jsonString);

				if (page.Id > 0)	// For existing pages ...
				{
					// Validate that the parent and order are not changing on updates.
					var originalPage = _pageRepository.Read(page.Id);
					if (!(originalPage.Order == page.Order && originalPage.ParentPageId == page.ParentPageId))
						throw new InvalidOperationException("Changing page order and parent id not supported by SavePage. Use MovePage instead.");

					_pageRepository.Update(page);

					var existingBullets = _bulletRepository.ReadByPageId(page.Id);
					// Process incoming bullets. If they exist update, otherwise create.
					page.Bullets.ForEach(bullet =>
					{
						bullet.PageId = page.Id;
						if (existingBullets.Any(existing => existing.Id == bullet.Id))
							_bulletRepository.Update(bullet);
						else
							_bulletRepository.Create(bullet);
					});
					// Delete any existing bullets not included with incoming bullets.
					existingBullets.ForEach(existing =>
					{
						if (!page.Bullets.Any(bullet => bullet.Id == existing.Id))
							_bulletRepository.Delete(existing.Id);
					});
				}
				else // For new pages ...
				{
					// Push siblings after the starting at the new page's order up by one.
					var siblings = _pageRepository.ReadByParentId(page.ParentPageId);
					for (int i = page.Order; i < siblings.Count; i++)
					{
						siblings[i].Order++;
						_pageRepository.Update(siblings[i]);
					}

					_pageRepository.Create(page);
					page.Bullets.ForEach(bullet =>
					{
						bullet.PageId = page.Id;
						_bulletRepository.Create(bullet);
					});

				}

				var serializer = new JavaScriptSerializer();
				var pageJson = serializer.Serialize(page);
				WriteResponse(context, "application/json", pageJson);
			}
		}

		public void ProcessSaveUserPageSettingsRequest(HttpContextBase context)
		{
			if (context.Request.IsAuthenticated)
			{
				var userName = context.User.Identity.Name;
				var jsonString = string.Empty;
				using (var reader = new StreamReader(context.Request.InputStream))
				{
					jsonString = reader.ReadToEnd();
				}

				var jsonSerializer = new JavaScriptSerializer();
				var clientUserPageSettings = jsonSerializer.Deserialize<UserPageSettings>(jsonString);

				var databaseUserPageSettings = _dataStore.UserPageSettings_ReadByUserAndPage(userName, clientUserPageSettings.PageId);

				if (null == databaseUserPageSettings)
				{
					clientUserPageSettings.UserName = userName;
					_dataStore.UserPageSettings_Create(clientUserPageSettings);
				}
				else
				{
					databaseUserPageSettings.HidePage = clientUserPageSettings.HidePage;
					_dataStore.UserPageSettings_Update(databaseUserPageSettings);
				}
			}
			WriteResponse(context, "text/html", "Success");
		}

		public void ProcessMovePageRequest(HttpContextBase context)
		{
			if (HasEditAuthorization(context))
			{
				var moveRequestJson = ReadPostData(context);

				var jsonSerializer = new JavaScriptSerializer();
				var moveRequest = jsonSerializer.Deserialize<MoveTocRequest>(moveRequestJson);

				var page = _pageRepository.Read(moveRequest.PageId);
				var oldSiblings = _pageRepository.ReadByParentId(page.ParentPageId);
				List<Page> newSiblings;

				int insertIndex, updateStartIndex, updateEndIndex;

				oldSiblings.RemoveAt(page.Order);

				// When the parent page changes...
				if (moveRequest.NewParentId != page.ParentPageId)
				{
					// Update siblings of old parent.
					for (int i = page.Order; i < oldSiblings.Count; i++)
					{
						oldSiblings[i].Order = i;
						_pageRepository.Update(oldSiblings[i]);
					}

					// Read new siblings and set update values.
					newSiblings = _pageRepository.ReadByParentId(moveRequest.NewParentId);
					insertIndex = moveRequest.NewPosition;
					updateStartIndex = moveRequest.NewPosition;
					updateEndIndex = newSiblings.Count;
				}
				else
				{
					// when the parent doesn't change, the siblings don't change.
					newSiblings = oldSiblings;
					if (moveRequest.NewPosition > page.Order)
					{
						// if the new position is greater than the old position ...
						insertIndex = moveRequest.NewPosition;
						updateStartIndex = page.Order;				// ... the old position is the start where orders must be updated.
						updateEndIndex = moveRequest.NewPosition;	// ... the new position is the end of where orders must be updated.
					}
					else
					{
						// if the new position is less than the old position ...
						insertIndex = moveRequest.NewPosition;
						updateStartIndex = moveRequest.NewPosition;	// ... the new position is the start of where orders must be updated.
						updateEndIndex = page.Order;				// ... the old position is the end of where orders must be updated.
					}
				}

				// Insert that page in the new location and update order numbers.
				page.ParentPageId = moveRequest.NewParentId;
				newSiblings.Insert(insertIndex, page);
				for (int i = updateStartIndex; i <= updateEndIndex; i++)
				{
					newSiblings[i].Order = i;
					_pageRepository.Update(newSiblings[i]);
				}
			}
		}


		#endregion
	}
}
