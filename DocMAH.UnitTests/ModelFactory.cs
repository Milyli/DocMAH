using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocMAH.Models;

namespace DocMAH.UnitTests
{
	public class ModelFactory
	{
		public static int _pageCount = 0;
		public static int _bulletCount = 0;

		public Bullet CreateBullet(int id = 0, int pageId = 0)
		{
			var result = new Bullet
			{
				Id = id,
				Text = string.Format("Unit test bullet text - {0}.", _bulletCount),
				DocHorizontalOffset = 10,
				DocVerticalOffset = 10,
				HorizontalOffset = 20,
				VerticalOffset = 20,
				OffsetElementId = "TestBulletOffsetElementId",
				Number = 1,
				PageId = pageId,
			};

			_bulletCount++;

			return result;
		}

		public Page CreatePage(
			int id = 0,
			int? parentPageId = null,
			string matchUrls = null,
			int order = 1,
			bool isHidden = false)
		{
			var result = new Page
			{
				Id = id,
				Content = "This is some test content for a page.",
				DocHorizontalOffset = 10,
				DocVerticalOffset = 10,
				DocImageUrl = "/TestApplication/TestBackgroundImage.png",
				HorizontalOffset = 20,
				VerticalOffset = 20,
				OffsetElementId = "TestPageOffsetElementId",
				MatchUrls = matchUrls ?? string.Format("/Test/Run-{0}*", _pageCount),
				Order = order,
				ParentPageId = parentPageId,
				PageType = PageTypes.FirstTimePage,
				SourceUrl = string.Format("/Test/Run/{0}", _pageCount),
				Title = string.Format("Unit Test Help {0}", _pageCount.ToString("00#")),
				IsHidden = isHidden,
			};

			_pageCount++;

			return result;
		}
	}
}
