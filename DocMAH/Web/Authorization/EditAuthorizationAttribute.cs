using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using DocMAH.Configuration;
using DocMAH.Web.Authorization;
using DocMAH.Web.Requests;

namespace DocMAH.Web.Authorization
{
	/// <summary>
	/// Apply to IRequestProcessor implementations to indicate edit authorization is required.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class EditAuthorizationAttribute : Attribute
	{
		#region Constructors

		/// <summary>
		/// Initializes a new EditAuthorizationAttribute.
		/// </summary>
		/// <param name="requiresAuthorization">Set to false or remove the attribute to bypass authorization.</param>
		public EditAuthorizationAttribute(bool requiresAuthorization = true)
		{
			RequiresAuthorization = requiresAuthorization;
		}

		#endregion

		#region Private Fields

		public bool RequiresAuthorization { get; private set; }
		
		#endregion
	}
}
