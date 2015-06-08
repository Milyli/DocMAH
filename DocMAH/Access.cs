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
		#region Constructors

		public Access(HttpContextBase httpContext)
		{
			_httpContext = httpContext;
		}

		#endregion

		#region Private Fields

		private HttpContextBase _httpContext;

		#endregion

		/// <summary>
		/// Returns true if the current request is authorized to edit documentation.
		/// </summary>
		public bool CanEdit
		{
			get
			{
				var request = _httpContext.Request;

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
