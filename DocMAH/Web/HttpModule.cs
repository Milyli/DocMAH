using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
		internal HttpModule(HttpContextBase httpContext, IContainer container, IDataStore dataStore, IHelpContentManager helpContentManager)
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

		#region Internal Fields

		internal const string ContainerKey = "DocMAH.Container";
		internal const string DocmahInitializedKey = "DocMAH.Initialized";

		internal const string MvcHandlerName = "System.Web.Mvc.MvcHandler";
		
		#endregion

		#region Internal Methods

		internal void AttachFilterEventHandler(object sender, EventArgs e)
		{
			// If a test context has been set, use it. Otherwise, use the current context.
			var context = _httpContext ?? new HttpContextWrapper(HttpContext.Current);

			// Prevent the filter from being added twice.
			if (!context.Items.Contains(ContainerKey))
			{
				// Flag that filter creation routine has been processed.
				context.Items.Add(ContainerKey, true);
				
				// Limit create help functionality to compatible handlers. 
				// This prevents DocMAH from loading onto other handlers: Glimpse, Elmah, etc...
				if (null != context.CurrentHandler
					&& CompatibleHandlers.Contains(context.CurrentHandler.GetType().FullName))
				{
					var response = context.Response;
					if (response.ContentType.ToLower() == ContentTypes.Html)
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

		#region Public Fields

		// TODO: Consider making this a mutable list so that users can configure custom handlers here.
		//			That might be too much power though. For now, just let them see what's available.
		public readonly ReadOnlyCollection<string> CompatibleHandlers = 
			new ReadOnlyCollection<string>(new List<string> { 
				MvcHandlerName,
			});

		#endregion

		#region IHttpModule Members

		public void Dispose()
		{
			// Do nothing.
		}

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

		#endregion
	}
}
