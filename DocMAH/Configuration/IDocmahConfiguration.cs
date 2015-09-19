using System;
using System.Collections.Generic;
using System.Linq;

namespace DocMAH.Configuration
{
	public interface IDocmahConfiguration
	{
		string ConnectionStringName { get; set; }

		string CssUrl { get; set; }

		IDocumentationConfiguration DocumentationConfiguration { get; set; }

		IEditHelpConfiguration EditHelpConfiguration { get; set; }

		string JsUrl { get; set; }

		IPopupViewerConfiguration PopupViewerConfiguration { get; set; }
	}
}
