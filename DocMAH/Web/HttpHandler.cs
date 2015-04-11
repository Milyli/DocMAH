using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using DocMAH.Configuration;
using DocMAH.Models;
using DocMAH.Properties;
using DocMAH.Extensions;
using DocMAH.Data;

namespace DocMAH.Web
{
	public class HttpHandler : IHttpHandler
	{
		#region Constructors

		public HttpHandler()
			: this(new RequestProcessor())
		{
		}

		public HttpHandler(IRequestProcessor requestProcessor)
		{
			_requestProcessor = requestProcessor;
		}

		#endregion

		#region Private Fields

		private IRequestProcessor _requestProcessor;

		#endregion

		#region IHttpHandler Members

		public bool IsReusable
		{
			get { return false; }
		}

		public void ProcessRequest(HttpContext context)
		{
			var requestProcessor = new RequestProcessor();
			var wrapper = new HttpContextWrapper(context);

			var method = context.Request["m"];
			if (method == "DeletePage")
				requestProcessor.ProcessDeletePageRequest(wrapper);
			else if (method == "CSS")
				requestProcessor.ProcessCssRequest(wrapper);
				else if (method=="DocumentationPage")
				requestProcessor.ProcessDocumentationPageRequest(wrapper);
			else if (method=="GenerateInstallScript")
				requestProcessor.ProcessGenerateInstallScriptRequest(wrapper);
			else if (method=="JavaScript")
				requestProcessor.ProcessJavaScriptRequest(wrapper);
			else if (method=="MovePage")
				requestProcessor.ProcessMovePageRequest(wrapper);
			else if (method=="NotFound")
				requestProcessor.ProcessNotFound(wrapper);
			else if (method=="ReadApplicationSettings")
				requestProcessor.ProcessReadApplicationSettingsRequest(wrapper);
			else if (method=="ReadPage")
				requestProcessor.ProcessReadPageRequest(wrapper);
			else if (method=="ReadTableOfContents")
				requestProcessor.ProcessReadTableOfContentsRequest(wrapper);
			else if (method=="SavePage")
				requestProcessor.ProcessSaveHelpRequest(wrapper);
			else if (method=="SaveUserPageSettings")
				requestProcessor.ProcessSaveUserPageSettingsRequest(wrapper);
			else
				requestProcessor.ProcessNotFound(wrapper);
		}

		#endregion
	}
}
