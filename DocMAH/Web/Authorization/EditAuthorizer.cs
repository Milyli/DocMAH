using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using DocMAH.Configuration;

namespace DocMAH.Web.Authorization
{
	public class EditAuthorizer : IEditAuthorizer
	{
		#region Constructors

		public EditAuthorizer()
			: this(new HttpContextWrapper(HttpContext.Current))
		{

		}

		public EditAuthorizer(HttpContextBase httpContext)
		{
			_httpContext = httpContext;
		}

		#endregion

		#region Private Fields

		private HttpContextBase _httpContext;

		#endregion

		#region IAuthorizer Members

		public bool Authorize()
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

		#endregion
	}
}
