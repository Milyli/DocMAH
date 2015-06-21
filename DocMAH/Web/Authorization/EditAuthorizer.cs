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

		public EditAuthorizer(HttpContextBase httpContext, IEditHelpConfiguration editHelpConfiguration)
		{
			_httpContext = httpContext;
			_editHelpConfiguration = editHelpConfiguration;
		}

		#endregion

		#region Private Fields
				
		private readonly HttpContextBase _httpContext;
		private readonly IEditHelpConfiguration _editHelpConfiguration;

		#endregion

		#region IAuthorizer Members

		public bool Authorize()
		{
			var request = _httpContext.Request;

			if (_editHelpConfiguration.IsDisabled)
				return false;

			if (_editHelpConfiguration.RequireAuthentication && (request == null || !request.IsAuthenticated))
				return false;

			if (_editHelpConfiguration.RequireLocalConnection && (request == null || !request.IsLocal))
				return false;

			return true;
		}

		#endregion
	}
}
