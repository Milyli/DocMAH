using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using DocMAH.Adapters;
using DocMAH.Configuration;
using DocMAH.Data;
using DocMAH.Data.Sql;
using DocMAH.Web.Authorization;
using DocMAH.Web.Requests;
using DocMAH.Web.Requests.Processors;

namespace DocMAH.Dependencies
{
	public class Registrar
	{
		#region Private Fields

		private static Container _container;
		private static readonly object _lock = new object();		

		#endregion

		#region Public Methods

		public static Container Initialize()
		{
			lock (_lock)
			{
				if (null == _container)
				{
					_container = new Container();

					// Self registration.
					_container.Register<IContainer>(c => _container);

					// Adapter registration.
					_container.Register<IDebugger>(c => new DebuggerAdapter());

					// Request Process registration.
					_container.Register<IRequestProcessorFactory>(c => new RequestProcessorFactory(c.Resolve<IContainer>()));
					_container.Register<IRequestProcessor>(RequestTypes.Css, c => new CssRequestProcessor(c.Resolve<IMinifier>()));
					_container.Register<IRequestProcessor>(RequestTypes.DeletePage, c => new DeletePageRequestProcessor(c.Resolve<IBulletRepository>(), c.Resolve<IPageRepository>()));
					_container.Register<IRequestProcessor>(RequestTypes.DocumentationPage, c => new DocumentationPageRequestProcessor(c.Resolve<HttpContextBase>(), c.Resolve<IContentConfiguration>(), c.Resolve<IDocumentationConfiguration>(), c.Resolve<IMinifier>()));
					_container.Register<IRequestProcessor>(RequestTypes.GenerateInstallScript, c => new GenerateInstallScriptRequestProcessor(c.Resolve<IBulletRepository>(), c.Resolve<DocMAH.Configuration.IDataStoreConfiguration>(), c.Resolve<HttpContextBase>(), c.Resolve<IPageRepository>()));
					_container.Register<IRequestProcessor>(RequestTypes.JavaScript, c => new JavaScriptRequestProcessor(c.Resolve<IMinifier>()));
					_container.Register<IRequestProcessor>(RequestTypes.MovePage, c => new MovePageRequestProcessor(c.Resolve<IPageRepository>()));
					_container.Register<IRequestProcessor>(RequestTypes.NotFound, c => new NotFoundRequestProcessor());
					_container.Register<IRequestProcessor>(RequestTypes.ReadApplicationSettings, c => new ReadApplicationSettingsRequestProcessor(c.Resolve<IEditAuthorizer>()));
					_container.Register<IRequestProcessor>(RequestTypes.ReadPage, c => new ReadPageRequestProcessor(c.Resolve<IBulletRepository>(), c.Resolve<IPageRepository>()));
					_container.Register<IRequestProcessor>(RequestTypes.ReadTableOfContents, c => new ReadTableOfContentsRequestProcessor(c.Resolve<IEditAuthorizer>(), c.Resolve<IPageRepository>()));
					_container.Register<IRequestProcessor>(RequestTypes.SavePage, c => new SaveHelpRequestProcessor(c.Resolve<IBulletRepository>(), c.Resolve<IPageRepository>()));
					_container.Register<IRequestProcessor>(RequestTypes.SaveUserPageSettings, c => new SaveUserPageSettingsRequestProcessor(c.Resolve<HttpContextBase>(), c.Resolve<IUserPageSettingsRepository>()));
					_container.Register<IRequestProcessor>(RequestTypes.Unauthorized, c => new UnauthorizedRequestProcessor());

					// DataStore registration.
					_container.Register<IBulletRepository>(c => new SqlBulletRepository(c.Resolve<ISqlConnectionFactory>()));
					_container.Register<IConfigurationRepository>(c => new SqlConfigurationRepository(c.Resolve<ISqlConnectionFactory>()));
					_container.Register<IDataStore>(c => new SqlDataStore(c.Resolve<DocMAH.Configuration.IDataStoreConfiguration>(), c.Resolve<ISqlConnectionFactory>()));
					_container.Register<IPageRepository>(c => new SqlPageRepository(c.Resolve<ISqlConnectionFactory>()));
					_container.Register<IUserPageSettingsRepository>(c => new SqlUserPageSettingsRepository(c.Resolve<ISqlConnectionFactory>()));

					// Content manager registration.
					_container.Register<IHelpContentManager>(c => new HelpContentManager(c.Resolve<HttpContextBase>(), c.Resolve<IDataStore>(), c.Resolve<IDataStoreConfiguration>()));

					// Configuration registration.
					_container.Register<IContentConfiguration>(c => DocmahConfigurationSection.Current);
					_container.Register<IDataStoreConfiguration>(c => new DataStoreConfiguration(c.Resolve<IConfigurationRepository>()));
					_container.Register<IDocumentationConfiguration>(c => DocmahConfigurationSection.Current.DocumentationConfiguration);
					_container.Register<IEditHelpConfiguration>(c => DocmahConfigurationSection.Current.EditHelpConfiguration);

					// SqlDataStore registration.
					_container.Register<ISqlConnectionFactory>(c => new SqlConnectionFactory(c.Resolve<IContentConfiguration>()));

					// Authorization registration.
					_container.Register<IEditAuthorizer>(c => new EditAuthorizer(c.Resolve<HttpContextBase>(), c.Resolve<IEditHelpConfiguration>()));

					// Other web registration.
					_container.Register<HttpContextBase>(c => { return new HttpContextWrapper(HttpContext.Current); });
					_container.Register<IMinifier>(c => new Minifier(c.Resolve<IDebugger>(), c.Resolve<HttpContextBase>()));
				}
			}

			return _container;
		}
		
		#endregion

	}
}
