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
	public class SaveHelpRequestProcessor : IRequestProcessor
	{
		#region Constructors

		public SaveHelpRequestProcessor(IBulletRepository bulletRepository, IPageRepository pageRepository)
		{
			_bulletRepository = bulletRepository;
			_pageRepository = pageRepository;
		}

		#endregion

		#region Private Fields

		private readonly IBulletRepository _bulletRepository;
		private readonly IPageRepository _pageRepository;

		#endregion

		#region IRequestProcessor Members

		public ResponseState Process(string data)
		{
			var jsonSerializer = new JavaScriptSerializer();
			var page = jsonSerializer.Deserialize<Page>(data);

			if (page.Id > 0)	// For existing pages ...
			{
				// Validate that the parent and order are not changing on updates.
				var originalPage = _pageRepository.Read(page.Id);
				if (!(originalPage.Order == page.Order && originalPage.ParentPageId == page.ParentPageId))
					throw new InvalidOperationException("Changing page order and parent id not supported by SavePage. Use MovePage instead.");

				_pageRepository.Update(page);

				var existingBullets = _bulletRepository.ReadByPageId(page.Id);
				// Process incoming bullets. If they exist update, otherwise create.
				page.Bullets.ForEach(bullet =>
				{
					bullet.PageId = page.Id;
					if (existingBullets.Any(existing => existing.Id == bullet.Id))
						_bulletRepository.Update(bullet);
					else
						_bulletRepository.Create(bullet);
				});
				// Delete any existing bullets not included with incoming bullets.
				existingBullets.ForEach(existing =>
				{
					if (!page.Bullets.Any(bullet => bullet.Id == existing.Id))
						_bulletRepository.Delete(existing.Id);
				});
			}
			else // For new pages ...
			{
				// Push siblings after the starting at the new page's order up by one.
				var siblings = _pageRepository.ReadByParentId(page.ParentPageId);
				for (int i = page.Order; i < siblings.Count; i++)
				{
					siblings[i].Order++;
					_pageRepository.Update(siblings[i]);
				}

				_pageRepository.Create(page);
				page.Bullets.ForEach(bullet =>
				{
					bullet.PageId = page.Id;
					_bulletRepository.Create(bullet);
				});

			}

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
