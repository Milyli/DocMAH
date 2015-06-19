﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using DocMAH.Models;
using DocMAH.Web.Authorization;

namespace DocMAH.Web.Requests.Processors
{
	public class ReadApplicationSettingsRequestProcessor  : IRequestProcessor
	{
		#region Constructors

		public ReadApplicationSettingsRequestProcessor(IEditAuthorizer editAuthorizer)
		{
			_editAuthorizer = editAuthorizer;
		}

		#endregion

		#region Private Fields

		private readonly IEditAuthorizer _editAuthorizer;

		#endregion

		#region IRequestProcessor Members

		public ResponseState Process(string data)
		{
			var applicationSettings = new ApplicationSettings
			{
				CanEdit = _editAuthorizer.Authorize(),
			};

			var serializer = new JavaScriptSerializer();
			var applicationSettingsJson = serializer.Serialize(applicationSettings);

			return new ResponseState
			{
				Content = applicationSettingsJson,
				ContentType = ContentTypes.Json,
			};
		}

		public string RequestType
		{
			get { return RequestTypes.ReadApplicationSettings; }
		}

		public bool RequiresEditAuthorization
		{
			get { return false; }
		}

		#endregion
	}
}
