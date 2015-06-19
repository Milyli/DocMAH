using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using DocMAH.Data;
using DocMAH.Data.Sql;
using DocMAH.Web.Authorization;

namespace DocMAH.Web.Requests.Processors
{
	public class ReadTableOfContentsRequestProcessor : IRequestProcessor
	{
		#region Constructors

		public ReadTableOfContentsRequestProcessor(IEditAuthorizer editAuthorizer, IPageRepository pageRepository)
		{
			_editAuthorizer = editAuthorizer;
			_pageRepository = pageRepository;
		}

		#endregion

		#region Private Fields

		private readonly IEditAuthorizer _editAuthorizer;
		private readonly IPageRepository _pageRepository;

		#endregion

		#region IRequestProcessor Members

		public ResponseState Process(string data)
		{
			var pages = _pageRepository.ReadTableOfContents(_editAuthorizer.Authorize());

			var serializer = new JavaScriptSerializer();
			var pagesJson = serializer.Serialize(pages);

			return new ResponseState
			{
				Content = pagesJson,
				ContentType = ContentTypes.Json,
			};				
		}

		public string RequestType
		{
			get { return RequestTypes.ReadTableOfContents; }
		}

		public bool RequiresEditAuthorization
		{
			get { return false; }
		}

		#endregion
	}
}
