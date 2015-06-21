using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using DocMAH.Data;
using DocMAH.Data.Sql;
using DocMAH.Models;
using DocMAH.UnitTests;
using DocMAH.Web;
using DocMAH.Web.Requests;
using NUnit.Framework;

namespace DocMAH.IntegrationTests.Tests
{
	[TestFixture]
	public class HttpHandlerIntegrationTests : BaseIntegrationTestFixture
	{
		#region Tests

		/// <summary>
		/// Tests that pages are moved correctly via the web service call.
		/// </summary>
		/// <remarks>
		/// Promoted from B36_PageUrlsDeletedOnPageReorder
		/// </remarks>
		[Test]
		[Description("Makes a move page request.")]
		public void MovePage_Success()
		{
			// Arrange
			var pageRepository = Container.ResolveInstance<IPageRepository>();

			var childMatchUrl = "/Pages/Child";
			var movedMatchUrl = "/Pages/Moved";

			var parentPage = Models.CreatePage();
			pageRepository.Create(parentPage);
			var childPage = Models.CreatePage(parentPageId: parentPage.Id, matchUrls: childMatchUrl);
			pageRepository.Create(childPage);
			var pageToMove = Models.CreatePage(matchUrls: movedMatchUrl);
			pageRepository.Create(pageToMove);

			var moveRequestModel = new MoveTocRequest()
			{
				NewParentId = parentPage.Id,
				NewPosition = 0,
				PageId = pageToMove.Id
			};
			var serializer = new JavaScriptSerializer();
			var moveRequestData = serializer.Serialize(moveRequestModel);

			HttpContext.SetRequestContent(moveRequestData);
			HttpContext.SetRequestParameter("m", RequestTypes.MovePage);

			var handler = new HttpHandler(Container);

			// Act
			handler.ProcessWrappedRequest(HttpContext.Object);

			// Assert
			var movedPage = pageRepository.Read(pageToMove.Id);
			var existingChildPage = pageRepository.Read(childPage.Id);

			Assert.That(existingChildPage.MatchUrls, Is.EqualTo(childMatchUrl), "The original child page's match URL should remain the same."); // This is the bug.
			Assert.That(movedPage.MatchUrls, Is.EqualTo(movedMatchUrl), "The moved page's match URL should remain the same.");					// This should not change.
		}
		
		#endregion
	}
}
