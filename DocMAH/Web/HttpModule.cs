using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using DocMAH.Data;

namespace DocMAH.Web
{
	public class HttpModule : IHttpModule
	{
		#region Constructors

		public HttpModule() : this(new DatabaseUpdater())
		{

		}

		public HttpModule(IDatabaseUpdater databaseUpdater)
		{
			_databaseUpdater = databaseUpdater;
		}

		#endregion

		#region Private Fields

		private IDatabaseUpdater _databaseUpdater;

		#endregion

		#region IHttpModule Members

		public void Dispose()
		{
			// Do nothing.
		}

		#region Private Fields

		const string ContextKey = "DocMAH.ModuleInstalled";

		#endregion

		public void Init(HttpApplication context)
		{
			_databaseUpdater.Update();

			context.PreSendRequestHeaders += AttachFilterEventHandler;
			context.PostReleaseRequestState += AttachFilterEventHandler;
		}

		void AttachFilterEventHandler(object sender, EventArgs e)
		{
			var context = HttpContext.Current;

			// Prevent the filter from being added twice.
			// Limit create help functionality to MvcHandler for now. (Prevents it from loading onto other modules: Glimpse, Elmah, etc...
			if (!context.Items.Contains(ContextKey) && null != context.CurrentHandler && context.CurrentHandler.GetType().FullName == "System.Web.Mvc.MvcHandler")
			{
				var response = context.Response;
				if (response.ContentType == "text/html")
					response.Filter = new HttpResponseFilter(response.Filter);
				context.Items.Add(ContextKey, new object());
			}
		}

		#endregion
	}
}
