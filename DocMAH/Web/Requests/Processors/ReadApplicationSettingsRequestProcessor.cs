using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using DocMAH.Configuration;
using DocMAH.Models;
using DocMAH.Web.Authorization;


namespace DocMAH.Web.Requests.Processors
{
	public class ReadApplicationSettingsRequestProcessor  : IRequestProcessor
	{
		#region Constructors

		public ReadApplicationSettingsRequestProcessor(IDocmahConfiguration docmahConfiguration, IEditAuthorizer editAuthorizer)
		{
			_docmahConfiguration = docmahConfiguration;
			_editAuthorizer = editAuthorizer;
		}

		#endregion

		#region Private Fields

		private readonly IDocmahConfiguration _docmahConfiguration;
		private readonly IEditAuthorizer _editAuthorizer;

		#endregion

		#region IRequestProcessor Members

		public ResponseState Process(string data)
		{
			var applicationSettings = new ApplicationSettings
			{
				CanEdit = _editAuthorizer.Authorize(),
				DisableDocumentation = _docmahConfiguration.DocumentationConfiguration.Disabled,
			};

			var serializer = new JavaScriptSerializer();
			var applicationSettingsJson = serializer.Serialize(applicationSettings);

			return new ResponseState
			{
				Content = applicationSettingsJson,
				ContentType = ContentTypes.Json,
			};
		}

		#endregion
	}
}
