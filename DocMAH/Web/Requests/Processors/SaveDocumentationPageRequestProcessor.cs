using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using DocMAH.Data;
using DocMAH.Models;

namespace DocMAH.Web.Requests.Processors
{
	public class SaveDocumentationPageRequestProcessor : IRequestProcessor
	{
		#region Constructors

		public SaveDocumentationPageRequestProcessor(IDocumentationPageRepository documentationPageRepository)
		{
			_documentationPageRepository = documentationPageRepository;
		}

		#endregion

		#region Private Fields

		private readonly IDocumentationPageRepository _documentationPageRepository;

		#endregion

		#region Public Methods

		public ResponseState Process(string data)
		{
			var serializer = new JavaScriptSerializer();
			var clientPage = serializer.Deserialize<DocumentationPage>(data);

			if (clientPage.Id > 0)
			{
				// Validate that the parent and order are not changing on updates.
				var dataStorePage = _documentationPageRepository.Read(clientPage.Id);
				if (!(dataStorePage.Order == clientPage.Order && dataStorePage.ParentPageId == clientPage.ParentPageId))
					throw new InvalidOperationException("Changing page order and parent id not supported by SavePage. Use MovePage instead.");

				_documentationPageRepository.Update(clientPage);
			}
			else
			{
				// Push siblings after the starting at the new page's order up by one.
				var siblings = _documentationPageRepository.ReadByParentId(clientPage.ParentPageId);
				for (int i = clientPage.Order; i < siblings.Count; i++)
				{
					siblings[i].Order++;
					_documentationPageRepository.Update(siblings[i]);
				}
			}

			var pageJson = serializer.Serialize(clientPage);
			return new ResponseState
			{
				Content = pageJson,
				ContentType = ContentTypes.Json,
			};
		}

		#endregion
	}
}
