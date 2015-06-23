using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using DocMAH.Content;
using DocMAH.Data;
using DocMAH.Dependencies;
using DocMAH.Web.Authorization;
using DocMAH.Web.Requests;

namespace DocMAH.Web
{
	public class HttpModule : IHttpModule
	{
		#region Constructors

		/// <summary>
		/// Runtime constructor.
		/// </summary>
		public HttpModule()
		{
			_container = Registrar.Initialize();
			_dataStore = _container.Resolve<IDataStore>();
			_helpContentManager = _container.Resolve<IHelpContentManager>();
		}

		/// <summary>
		/// Test constructor.
		/// </summary>
		/// <param name="httpContext"></param>
		/// <param name="container"></param>
		/// <param name="dataStore"></param>
		/// <param name="helpContentManager"></param>
		public HttpModule(HttpContextBase httpContext, IContainer container, IDataStore dataStore, IHelpContentManager helpContentManager)
		{
			_httpContext = httpContext;
			_container = container;
			_dataStore = dataStore;
			_helpContentManager = helpContentManager;
		}

		#endregion

		#region Private Fields

		private readonly HttpContextBase _httpContext;
		private readonly IContainer _container;
		private readonly IDataStore _dataStore;
		private readonly IHelpContentManager _helpContentManager;

		#endregion

		#region IHttpModule Members

		public void Dispose()
		{
			// Do nothing.
		}

		#region Public Fields

		public const string ContextKey = "DocMAH.Container";
		public const string DocmahInitializedKey = "DocMAH.Initialized";

		#endregion

		public void Init(HttpApplication application)
		{

			if (!(bool)(application.Application[DocmahInitializedKey] ?? false))
			{
				_dataStore.DataStore_Update();

				var fileName = Path.Combine(HostingEnvironment.MapPath("~"), ContentFileConstants.ContentFileName);
				_helpContentManager.ImportContent(fileName);

				application.Application[DocmahInitializedKey] = true;
			}

			application.PreSendRequestHeaders += AttachFilterEventHandler;
			application.PostReleaseRequestState += AttachFilterEventHandler;
		}

		void AttachFilterEventHandler(object sender, EventArgs e)
		{
			// If a test context has been set, use it. Otherwise, use the current context.
			var context = _httpContext ?? new HttpContextWrapper(HttpContext.Current);

			// Prevent the filter from being added twice.
			// Limit create help functionality to MvcHandler for now. (Prevents it from loading onto other modules: Glimpse, Elmah, etc...
			if (!context.Items.Contains(ContextKey))				
			{
				context.Items.Add(ContextKey, _container);

				if (null != context.CurrentHandler 
					&& context.CurrentHandler.GetType().FullName == "System.Web.Mvc.MvcHandler")
				{
					var response = context.Response;
					if (response.ContentType == "text/html")
						response.Filter = new HttpResponseFilter(
							response.Filter,
							_container.Resolve<IBulletRepository>(),
							_container.Resolve<IEditAuthorizer>(),
							_container.Resolve<IMinifier>(),
							_container.Resolve<IPageRepository>(),
							_container.Resolve<IUserPageSettingsRepository>());
				}
			}
		}

		#endregion
	}
}
