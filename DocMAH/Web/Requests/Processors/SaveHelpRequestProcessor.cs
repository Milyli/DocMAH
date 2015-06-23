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
			var clientPage = jsonSerializer.Deserialize<Page>(data);

			if (clientPage.Id > 0)	// For existing pages ...
			{
				// Validate that the parent and order are not changing on updates.
				var dataStorePage = _pageRepository.Read(clientPage.Id);
				if (!(dataStorePage.Order == clientPage.Order && dataStorePage.ParentPageId == clientPage.ParentPageId))
					throw new InvalidOperationException("Changing page order and parent id not supported by SavePage. Use MovePage instead.");

				_pageRepository.Update(clientPage);

				var dataStoreBullets = _bulletRepository.ReadByPageId(clientPage.Id);
				// Process incoming bullets. If they exist update, otherwise create.
				foreach(var clientBullet in clientPage.Bullets)
				{
					clientBullet.PageId = clientPage.Id;
					if (dataStoreBullets.Any(dataStoreBullet => dataStoreBullet.Id == clientBullet.Id))
						_bulletRepository.Update(clientBullet);
					else
						_bulletRepository.Create(clientBullet);
				}
				// Delete any existing bullets not included with incoming bullets.
				foreach(var dataStoreBullet in dataStoreBullets)
				{
					if (!clientPage.Bullets.Any(clientBullet => clientBullet.Id == dataStoreBullet.Id))
						_bulletRepository.Delete(dataStoreBullet.Id);
				}
			}
			else // For new pages ...
			{
				// Push siblings after the starting at the new page's order up by one.
				var siblings = _pageRepository.ReadByParentId(clientPage.ParentPageId);
				for (int i = clientPage.Order; i < siblings.Count; i++)
				{
					siblings[i].Order++;
					_pageRepository.Update(siblings[i]);
				}

				_pageRepository.Create(clientPage);
				foreach(var clientBullet in clientPage.Bullets)
				{
					clientBullet.PageId = clientPage.Id;
					_bulletRepository.Create(clientBullet);
				}
			}

			var serializer = new JavaScriptSerializer();
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
