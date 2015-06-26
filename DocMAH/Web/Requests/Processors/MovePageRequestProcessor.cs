using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using DocMAH.Data;
using DocMAH.Data.Sql;
using DocMAH.Models;
using DocMAH.Web.Authorization;

namespace DocMAH.Web.Requests.Processors
{
	[EditAuthorization]
	public class MovePageRequestProcessor : IRequestProcessor
	{
		#region Constructors

		public MovePageRequestProcessor(IPageRepository pageRepository)
		{
			_pageRepository = pageRepository;
		}

		#endregion

		#region Private Fields

		private readonly IPageRepository _pageRepository;

		#endregion

		#region IRequestProcessor Members

		public ResponseState Process(string data)
		{
			var jsonSerializer = new JavaScriptSerializer();
			var moveRequest = jsonSerializer.Deserialize<MoveTocRequest>(data);

			// Remove page from old siblings collection.
			var page = _pageRepository.Read(moveRequest.PageId);
			var oldSiblings = _pageRepository.ReadByParentId(page.ParentPageId);
			oldSiblings.RemoveAt(page.Order);

			List<Page> newSiblings;
			int updateStartIndex, updateEndIndex;
			int insertIndex = moveRequest.NewPosition;

			// When the parent page changes...
			if (moveRequest.NewParentId != page.ParentPageId)
			{
				// Update siblings of old parent.
				for (int i = page.Order; i < oldSiblings.Count; i++)
				{
					oldSiblings[i].Order = i;
					_pageRepository.Update(oldSiblings[i]);
				}

				// Read new siblings and set update values.
				newSiblings = _pageRepository.ReadByParentId(moveRequest.NewParentId);
				updateStartIndex = moveRequest.NewPosition;
				updateEndIndex = newSiblings.Count;
			}
			else
			{
				// When the parent doesn't change, the siblings don't change.
				newSiblings = oldSiblings;

				// if the new position is greater than the old position ...
				if (moveRequest.NewPosition > page.Order)
				{
					updateStartIndex = page.Order;				// ... the old position is the start where orders must be updated.
					updateEndIndex = moveRequest.NewPosition;	// ... the new position is the end of where orders must be updated.
				}
				// if the new position is less than the old position ...
				else
				{
					updateStartIndex = moveRequest.NewPosition;	// ... the new position is the start of where orders must be updated.
					updateEndIndex = page.Order;				// ... the old position is the end of where orders must be updated.
				}
			}

			// Insert page in the new location and update order numbers.
			page.ParentPageId = moveRequest.NewParentId;
			newSiblings.Insert(insertIndex, page);
			for (int i = updateStartIndex; i <= updateEndIndex; i++)
			{
				newSiblings[i].Order = i;
				_pageRepository.Update(newSiblings[i]);
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
