using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocMAH.Models
{
	public class ApplicationSettings
	{
		/// <summary>
		/// Set to true if the current request can edit any of the documentation.
		/// </summary>
		public bool CanEdit { get; set; }

		/// <summary>
		/// Set to true if the documentation page should be disabled.
		/// </summary>
		public bool DisableDocumentation { get; set; }
	}
}
