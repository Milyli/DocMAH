using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using DocMAH.Configuration;
using DocMAH.Data;
using DocMAH.Models;
using DocMAH.Properties;

namespace DocMAH.Web
{
	public class HttpResponseFilter : Stream
	{
		#region Constructors

		public HttpResponseFilter(Stream stream)
		{
			_stream = stream;
			_writer = new StreamWriter(_stream, Encoding.UTF8);
		}

		#endregion

		#region Private Fields

		private readonly Stream _stream;
		private readonly StreamWriter _writer; // Stream writer to write to response on.
		private string _unprocessedContent; // Content from previous write that could not be added.

		#endregion

		#region Private Methods

		/// <summary>
		/// Injects help content into response stream when needed.
		/// </summary>
		/// <param name="content"></param>
		/// <returns></returns>
		private string InsertContent(string content)
		{
			// TODO: break out documentation link script and only write first time view script if page is present or user can edit.
			if (!string.IsNullOrEmpty(content))
			{
				var access = new Access();

				// Look for complete beginning of head tag.
				// Inject CSS links after opening head tag so that styles may be overridden by site specific CSS.
				// Inject edit links instead of providing them programmatically so that the URLs are not exposed to users without permission.
				var headIndex = content.ToLower().IndexOf("<head>");
				if (headIndex >= 0)
				{
					_writer.Write(content.Remove(headIndex + 6));
					_writer.Write(Resources.Html_CssLink);
					content = content.Substring(headIndex + 6);
				}

				// Look for complete end of body tag.
				// Inject javascript before end body tag so that jQuery libraries in existing bundles are already loaded when the JS runs.
				var endBodyIndex = content.ToLower().IndexOf("</body>");
				if (endBodyIndex >= 0)
				{
					_writer.Write(content.Remove(endBodyIndex));
					_writer.Write(FormatHtmlViewHelp());
					if (access.CanEdit)
					{
						_writer.Write(ResourcesExtensions.Minify(Resources.Html_FirstTimeEdit, Resources.Html_FirstTimeEdit_min));
					}
					content = content.Substring(endBodyIndex);
				}
			}
			return content;
		}

		private string FormatHtmlViewHelp()
		{
			var requestUrl = HttpContext.Current.Request.Url.AbsolutePath;
			var database = new SqlDatabaseAccess();
			var page = database.Page_ReadByUrl(requestUrl.Replace('*', '%'));
			UserPageSettings userPageSettings = null;

			if (null != page)
			{
				page.Bullets = database.Bullet_ReadByPageId(page.Id);
				if (HttpContext.Current.Request.IsAuthenticated)
				{
					var userName = HttpContext.Current.User.Identity.Name;
					userPageSettings = database.UserPageSettings_ReadByUserAndPage(userName, page.Id);
				}
			}

			// The HTML is reused on the documentation page.
			// When injecting into other requests, the initialization scripts must be included.
			var firstTimeViewHtml = ResourcesExtensions.Minify(Resources.Html_FirstTimeView, Resources.Html_FirstTimeView_min);
			firstTimeViewHtml += ResourcesExtensions.Minify(Resources.Html_FirstTimeViewInjectedScripts, Resources.Html_FirstTimeViewInjectedScripts_min);

			// TODO: Iron out javascript reference injection for first time help in base site pages.
			// Attach jQueryUi CDN locations if not configured.
			// Leaving out jQuery for the time being as it's likely included.
			var javaScriptDependencies = new DocmahConfigurationSection().JsUrl;
			if (string.IsNullOrEmpty(javaScriptDependencies))
				firstTimeViewHtml += string.Format("<script src='{0}' type='application/javascript'></script>", CdnUrls.jsJQueryUi);

			// TODO: Cache the non-dynamic portion of the first time HTML for faster loading.
			
			var serializer = new JavaScriptSerializer();
			var pageJson = serializer.Serialize(page);
			firstTimeViewHtml = firstTimeViewHtml.Replace("[PAGEJSON]", pageJson);

			var userPageSettingsJson = serializer.Serialize(userPageSettings);
			firstTimeViewHtml = firstTimeViewHtml.Replace("[USERPAGESETTINGSJSON]", userPageSettingsJson);

			var access = new Access();
			var applicationSettings = new ApplicationSettings { CanEdit = access.CanEdit };
			var applicationSettingsJson = serializer.Serialize(applicationSettings);
			firstTimeViewHtml = firstTimeViewHtml.Replace("[APPLICATIONSETTINGSJSON]", applicationSettingsJson);

			return firstTimeViewHtml;
		}

		#endregion

		#region Public Methods

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_stream.Dispose();
				_writer.Dispose();
			}

			base.Dispose(disposing);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{

			// Prepend any leftover partial tags to current buffer and check for end body tag.
			_unprocessedContent = _unprocessedContent + Encoding.UTF8.GetString(buffer);

			// Look for complete head and body tags, return if body tag found and content written.
			_unprocessedContent = InsertContent(_unprocessedContent);

			// Check to see if the write buffer ends in the middle of any tag.
			// If so, write all but the beginning of the tag to the response stream.
			// Save beginning of tag for next write call.
			if (!string.IsNullOrEmpty(_unprocessedContent))
			{
				var lastBeginTagChar = _unprocessedContent.LastIndexOf('<');
				var lastEndTagChar = _unprocessedContent.LastIndexOf('>');
				if (lastBeginTagChar > lastEndTagChar)
				{
					// If the input ends in the middle of a tag,
					// write the input up to the start of the tag,
					// save the rest of the input for the next call to write.
					// If the full content ends in the middle of a tag, there's bigger problems.
					_writer.Write(_unprocessedContent.Remove(lastBeginTagChar));
					_unprocessedContent = _unprocessedContent.Substring(lastBeginTagChar);
				}
				else
				// Otherwise, finish writing all of the content.
				{
					_writer.Write(_unprocessedContent);
					_unprocessedContent = null;
				}
			}
		}

		public override bool CanRead
		{
			get { throw new NotImplementedException(); }
		}

		public override bool CanSeek
		{
			get { return _stream.CanSeek; }
		}

		public override bool CanWrite
		{
			get { return _stream.CanWrite; }
		}

		public override void Flush()
		{
			_unprocessedContent = InsertContent(_unprocessedContent);
			_writer.Write(_unprocessedContent);
			_unprocessedContent = null;
			_writer.Flush();
		}

		public override long Length
		{
			get { throw new NotImplementedException(); }
		}

		public override long Position
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotImplementedException();
		}

		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
