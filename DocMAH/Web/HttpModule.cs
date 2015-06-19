using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using DocMAH.Data;
using DocMAH.Dependencies;

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

		private HttpContextBase _httpContext;
		private IContainer _container;
		private IDataStore _dataStore;
		private IHelpContentManager _helpContentManager;

		#endregion

		#region IHttpModule Members

		public void Dispose()
		{
			// Do nothing.
		}

		#region Public Fields

		public const string ContextKey = "DocMAH.Container";

		#endregion

		public void Init(HttpApplication application)
		{
			_container = Registrar.Initialize();

			_dataStore = _container.ResolveInstance<IDataStore>();
			_dataStore.DataStore_Update();

			_helpContentManager = _container.ResolveInstance<IHelpContentManager>();
			_helpContentManager.UpdateDataStoreContent();

			application.PreSendRequestHeaders += AttachFilterEventHandler;
			application.PostReleaseRequestState += AttachFilterEventHandler;
		}

		void AttachFilterEventHandler(object sender, EventArgs e)
		{
			// If a test context has been set, use it. Otherwise, use the current context.
			var context = _httpContext ?? new HttpContextWrapper(HttpContext.Current);

			// Prevent the filter from being added twice.
			// Limit create help functionality to MvcHandler for now. (Prevents it from loading onto other modules: Glimpse, Elmah, etc...
			if (!context.Items.Contains(ContextKey)
				&& null != context.CurrentHandler
				&& context.CurrentHandler.GetType().FullName == "System.Web.Mvc.MvcHandler")
			{
				var response = context.Response;
				if (response.ContentType == "text/html")
					response.Filter = new HttpResponseFilter(
						response.Filter, 
						_container.ResolveInstance<IBulletRepository>(), 
						_container.ResolveInstance<IPageRepository>(), 
						_container.ResolveInstance<IUserPageSettingsRepository>());
				context.Items.Add(ContextKey, _container);
			}
		}

		#endregion
	}
}
