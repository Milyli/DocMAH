using System;
using System.Collections.Generic;
using System.Linq;

namespace DocMAH.Web.Html
{
	public interface IHtmlBuilder
	{
		string CreateDocumentationPageHtml();
		string CreateFirstTimeHelpHtml();
		string CreateFirstTimeHelpCssLink();
	}
}
