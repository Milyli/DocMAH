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
		public static int _bulletCount = 0;
		public static int _documentationPageCount = 0;
		public static int _firstTimeHelpCount = 0;

		public Bullet CreateBullet(int id = 0, int pageId = 0)
		{
			var result = new Bullet
			{
				Id = id,
				Text = string.Format("Unit test bullet text - {0}.", _bulletCount),
				HorizontalOffset = 20,
				VerticalOffset = 20,
				OffsetElementId = "TestBulletOffsetElementId",
				Number = 1,
				PageId = pageId,
			};

			_bulletCount++;

			return result;
		}

		public DocumentationPage CreateDocumentationPage(
			int id = 0,
			int? parentPageId = null,
			string title = null,
			int order = 1,
			bool isHidden = false)
		{
			title = title ?? string.Format("Unit Test Help {0}", _documentationPageCount.ToString("00#"));

			var result = new DocumentationPage
			{
				Id = id,
				Content = "Test content for documentation page.",
				Order = order,
				ParentPageId = parentPageId,
				Title = title,
				IsHidden = isHidden,
			};

			_documentationPageCount++;

			return result;
		}

		public FirstTimeHelp CreateFirstTimeHelp(
			int id = 0,
			string matchUrls = null
			)
		{
			var result = new FirstTimeHelp
			{
				Id = id,
				Content = "Test content for first time help.",
				HorizontalOffset = 20,
				VerticalOffset = 20,
				OffsetElementId = "TestPageOffsetElementId",
				MatchUrls = matchUrls ?? string.Format("/Test/Run-{0}*", _firstTimeHelpCount),
				SourceUrl = string.Format("/Test/Run/{0}", _firstTimeHelpCount),
				Title = string.Format("Unit Test Help {0}", _firstTimeHelpCount.ToString("00#")),
			};

			_firstTimeHelpCount++;

			return result;
		}
	}
}
