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

		public ReadApplicationSettingsRequestProcessor(IDocumentationConfiguration documentationConfiguration, IEditAuthorizer editAuthorizer)
		{
			_documentationConfiguration = documentationConfiguration;
			_editAuthorizer = editAuthorizer;
		}

		#endregion

		#region Private Fields

		private readonly IDocumentationConfiguration _documentationConfiguration;
		private readonly IEditAuthorizer _editAuthorizer;

		#endregion

		#region IRequestProcessor Members

		public ResponseState Process(string data)
		{
			var applicationSettings = new ApplicationSettings
			{
				CanEdit = _editAuthorizer.Authorize(),
				DisableDocumentation = _documentationConfiguration.Disabled,
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
