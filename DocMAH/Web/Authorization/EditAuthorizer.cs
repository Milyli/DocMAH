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

		public EditAuthorizer(HttpContextBase httpContext, IDocmahConfiguration docmahConfiguration)
		{
			_httpContext = httpContext;
			_docmahConfiguration = docmahConfiguration;
		}

		#endregion

		#region Private Fields
				
		private readonly HttpContextBase _httpContext;
		private readonly IDocmahConfiguration _docmahConfiguration;

		#endregion

		#region IAuthorizer Members

		public bool Authorize()
		{
			var request = _httpContext.Request;

			if (_docmahConfiguration.EditHelpConfiguration.IsDisabled)
				return false;

			if (_docmahConfiguration.EditHelpConfiguration.RequireAuthentication && (request == null || !request.IsAuthenticated))
				return false;

			if (_docmahConfiguration.EditHelpConfiguration.RequireLocalConnection && (request == null || !request.IsLocal))
				return false;

			return true;
		}

		#endregion
	}
}
