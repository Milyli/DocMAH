using System;
using System.Collections.Generic;
using System.Linq;

namespace DocMAH.Configuration
{
	public interface IDocumentationConfiguration
	{
		/// <summary>
		/// Title of the documentation page.
		/// </summary>
		string PageTitle { get; set; }

		/// <summary>
		/// Address of CSS link to add custom styles to the documentation page.
		/// </summary>
		string CustomCss { get; set; }

		/// <summary>
		/// Set to true when the documentation page should not be shown in the application.
		/// </summary>
		bool Disabled { get; set; }
	}
}
