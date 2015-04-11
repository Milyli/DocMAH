using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using DocMAH.Extensions;

namespace DocMAH.Web
{
	public interface IRequestProcessor
	{
		void ProcessCssRequest(HttpContextBase context);
		void ProcessDeletePageRequest(HttpContextBase context);
		void ProcessDocumentationPageRequest(HttpContextBase context);
		void ProcessGenerateInstallScriptRequest(HttpContextBase context);
		void ProcessJavaScriptRequest(HttpContextBase context);
		void ProcessNotFound(HttpContextBase context);
		void ProcessReadApplicationSettingsRequest(HttpContextBase context);
		void ProcessReadPageRequest(HttpContextBase context);
		void ProcessReadTableOfContentsRequest(HttpContextBase context);
		void ProcessSaveHelpRequest(HttpContextBase context);
		void ProcessSaveUserPageSettingsRequest(HttpContextBase context);
		void ProcessMovePageRequest(HttpContextBase context);
	}
}
