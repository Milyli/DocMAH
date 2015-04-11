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

		public Bullet CreateBullet(int pageId)
		{
			return new Bullet
			{
				Text = "Unit test bullet text.",
				DocHorizontalOffset = 10,
				DocVerticalOffset = 10,
				HorizontalOffset = 20,
				VerticalOffset = 20,
				OffsetElementId = "TestBulletOffsetElementId",
				Number = 1,
				PageId = pageId,
			};
		}

		public Page CreatePage(int? parentPageId = null, string matchUrls = null, int order = 1, bool isHidden = false)
		{
			return new Page
			{
				Content = "This is some test content for a page.",
				DocHorizontalOffset = 10,
				DocVerticalOffset = 10,
				DocImageUrl = "/TestApplication/TestBackgroundImage.png",
				HorizontalOffset = 20,
				VerticalOffset = 20,
				OffsetElementId = "TestPageOffsetElementId",
				MatchUrls = matchUrls ?? string.Format("/Test/Run-{0}*",_pageCount++) ,
				Order = order,
				ParentPageId = parentPageId,
				PageType = PageTypes.FirstTimePage,
				SourceUrl = string.Format("/Test/Run/{0}", _pageCount),
				Title = string.Format("Unit Test Help {0}", _pageCount.ToString("00#")),
				IsHidden = isHidden,
			};
		}
	}
}
