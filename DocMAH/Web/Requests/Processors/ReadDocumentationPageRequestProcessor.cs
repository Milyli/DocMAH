using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using DocMAH.Data;
using DocMAH.Data.Sql;

namespace DocMAH.Web.Requests.Processors
{
	public class ReadDocumentationPageRequestProcessor : IRequestProcessor
	{
		#region Constructors

		public ReadDocumentationPageRequestProcessor(IDocumentationPageRepository documentationPageRepository)
		{
			_documentationPageRepository = documentationPageRepository;
		}

		#endregion

		#region Private Fields

		private readonly IDocumentationPageRepository _documentationPageRepository;

		#endregion

		#region IRequestProcessor Members

		public ResponseState Process(string data)
		{
			var id = int.Parse(data);

			var page = _documentationPageRepository.Read(id);

			var serializer = new JavaScriptSerializer();
			var pageJson = serializer.Serialize(page);

			return new ResponseState
			{
				Content = pageJson,
				ContentType = ContentTypes.Json,
			};
		}

		#endregion
	}
}
