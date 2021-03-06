﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DocMAH.Data;
using DocMAH.Data.Sql;
using DocMAH.Web.Authorization;

namespace DocMAH.Web.Requests.Processors
{
	[EditAuthorization]
	public class DeletePageRequestProcessor : IRequestProcessor
	{
		#region Constructors

		public DeletePageRequestProcessor(IBulletRepository bulletRepository, IDocumentationPageRepository pageRepository, IUserPageSettingsRepository userPageSettingsRepository)
		{
			_bulletRepository = bulletRepository;
			_pageRepository = pageRepository;
			_userPageSettingsRepository = userPageSettingsRepository;
		}

		#endregion

		#region Private Fields

		private readonly IBulletRepository _bulletRepository;
		private readonly IDocumentationPageRepository _pageRepository;
		private readonly IUserPageSettingsRepository _userPageSettingsRepository;

		#endregion

		#region IRequestProcessor Members

		public ResponseState Process(string data)
		{
			var pageId = int.Parse(data);

			_bulletRepository.DeleteByPageId(pageId);
			_userPageSettingsRepository.DeleteByPageId(pageId);

			// Get the page so we have the parent id.
			var page = _pageRepository.Read(pageId);

			// Get its siblings and remove the page from the collection.
			var siblings = _pageRepository.ReadByParentId(page.ParentPageId);
			page = siblings.Where(p => p.Id == pageId).First();
			siblings.Remove(page);

			// Insert deleted page's children in order at deleted page's location.
			var children = _pageRepository.ReadByParentId(pageId);
			for (int i = children.Count - 1; i >= 0; i--)
			{
				siblings.Insert(page.Order, children[i]);
			}

			// Update parent id and order on all of deleted page's children and siblings.
			for (int i = page.Order; i < siblings.Count; i++)
			{
				var sibling = siblings[i];
				sibling.ParentPageId = page.ParentPageId;
				sibling.Order = i;
				_pageRepository.Update(sibling);
			}

			// Delete page after all children have been moved.
			_pageRepository.Delete(pageId);

			// Return generic success message.
			return new ResponseState
			{
				ContentType = ContentTypes.Html,
			};
		}

		#endregion
	}
}
