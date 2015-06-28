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
	public class SaveFirstTimeHelpRequestProcessor : IRequestProcessor
	{
		#region Constructors

		public SaveFirstTimeHelpRequestProcessor(IBulletRepository bulletRepository, IFirstTimeHelpRepository firstTimeHelpRepository)
		{
			_bulletRepository = bulletRepository;
			_firstTimeHelpRepository = firstTimeHelpRepository;
		}

		#endregion

		#region Private Fields

		private readonly IBulletRepository _bulletRepository;
		private readonly IFirstTimeHelpRepository _firstTimeHelpRepository;

		#endregion

		#region IRequestProcessor Members

		public ResponseState Process(string data)
		{
			var serializer = new JavaScriptSerializer();
			var clientHelp = serializer.Deserialize<FirstTimeHelp>(data);

			if (clientHelp.Id > 0)	// For existing pages ...
			{
				_firstTimeHelpRepository.Update(clientHelp);

				var dataStoreBullets = _bulletRepository.ReadByPageId(clientHelp.Id);
				// Process incoming bullets. If they exist update, otherwise create.
				foreach(var clientBullet in clientHelp.Bullets)
				{
					clientBullet.PageId = clientHelp.Id;
					if (dataStoreBullets.Any(dataStoreBullet => dataStoreBullet.Id == clientBullet.Id))
						_bulletRepository.Update(clientBullet);
					else
						_bulletRepository.Create(clientBullet);
				}
				// Delete any existing bullets not included with incoming bullets.
				foreach(var dataStoreBullet in dataStoreBullets)
				{
					if (!clientHelp.Bullets.Any(clientBullet => clientBullet.Id == dataStoreBullet.Id))
						_bulletRepository.Delete(dataStoreBullet.Id);
				}
			}
			else // For new pages ...
			{
				_firstTimeHelpRepository.Create(clientHelp);
				foreach(var clientBullet in clientHelp.Bullets)
				{
					clientBullet.PageId = clientHelp.Id;
					_bulletRepository.Create(clientBullet);
				}
			}

			var pageJson = serializer.Serialize(clientHelp);
			return new ResponseState
			{
				Content = pageJson,
				ContentType = ContentTypes.Json,
			};
		}

		#endregion
	}
}
