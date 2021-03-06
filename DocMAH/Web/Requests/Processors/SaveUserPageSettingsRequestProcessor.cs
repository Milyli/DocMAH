﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using DocMAH.Data;
using DocMAH.Models;
using DocMAH.Web.Authorization;

namespace DocMAH.Web.Requests.Processors
{
	[EditAuthorization]
	public class SaveUserPageSettingsRequestProcessor : IRequestProcessor
	{
		#region Constructors
		
		public SaveUserPageSettingsRequestProcessor(HttpContextBase httpContext, IUserPageSettingsRepository userPageSettingsRepository)
		{
			_httpContext = httpContext;
			_userPageSettingsRepository = userPageSettingsRepository;
		}

		#endregion

		#region Private Fields

		private readonly HttpContextBase _httpContext;
		private readonly IUserPageSettingsRepository _userPageSettingsRepository;

		#endregion

		#region IRequestProcessor Members

		public ResponseState Process(string data)
		{
			var userName = _httpContext.User.Identity.Name;

			var jsonSerializer = new JavaScriptSerializer();
			var clientUserPageSettings = jsonSerializer.Deserialize<UserPageSettings>(data);

			var databaseUserPageSettings = _userPageSettingsRepository.Read(userName, clientUserPageSettings.PageId);

			if (null == databaseUserPageSettings)
			{
				clientUserPageSettings.UserName = userName;
				_userPageSettingsRepository.Create(clientUserPageSettings);
			}
			else
			{
				databaseUserPageSettings.HidePage = clientUserPageSettings.HidePage;
				_userPageSettingsRepository.Update(databaseUserPageSettings);
			}

			return new ResponseState
			{
				Content = "Success",
				ContentType = ContentTypes.Html,
			};
		}

		#endregion
	}
}
