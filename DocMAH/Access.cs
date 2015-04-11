using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using DocMAH.Configuration;

namespace DocMAH
{
	public class Access
	{
		/// <summary>
		/// Returns true if the current request is authorized to edit documentation.
		/// </summary>
		public bool CanEdit
		{
			get
			{
				var request = HttpContext.Current.Request;

				if (DocmahConfigurationSection.Current.EditHelp.IsDisabled)
					return false;
				
				if (DocmahConfigurationSection.Current.EditHelp.RequireAuthentication && (request == null || !request.IsAuthenticated))
					return false;

				if (DocmahConfigurationSection.Current.EditHelp.RequireLocalConnection && (request == null || !request.IsLocal))
					return false;

				return true;
			}
		}
	}
}
