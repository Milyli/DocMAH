using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using DocMAH.Data;
using DocMAH.Data.Sql;

namespace DocMAH.Web.Requests.Processors
{
	public class ReadPageRequestProcessor : IRequestProcessor
	{
		#region Constructors

		public ReadPageRequestProcessor(IBulletRepository bulletRepository, IPageRepository pageRepository)
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
			var id = int.Parse(data);

			var page = _pageRepository.Read(id);
			page.Bullets = _bulletRepository.ReadByPageId(id);

			var serializer = new JavaScriptSerializer();
			var pageJson = serializer.Serialize(page);

			return new ResponseState
			{
				Content = pageJson,
				ContentType = ContentTypes.Json,
			};
		}

		public bool RequiresEditAuthorization
		{
			get { return false; }
		}

		#endregion
	}
}
