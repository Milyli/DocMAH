using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
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
					_container.RegisterResolver<IContainer>(c => _container);

					// Request Process registration.
					_container.RegisterResolver<IRequestProcessorFactory>(c => new RequestProcessorFactory(c.ResolveInstance<IContainer>()));
					_container.RegisterNamedResolver<IRequestProcessor>(RequestTypes.Css, c => new CssRequestProcessor());
					_container.RegisterNamedResolver<IRequestProcessor>(RequestTypes.DeletePage, c => new DeletePageRequestProcessor(c.ResolveInstance<IBulletRepository>(), c.ResolveInstance<IPageRepository>()));
					_container.RegisterNamedResolver<IRequestProcessor>(RequestTypes.DocumentationPage, c => new DocumentationPageRequestProcessor(c.ResolveInstance<HttpContextBase>(), c.ResolveInstance<IContentConfiguration>(), c.ResolveInstance<IDocumentationConfiguration>()));
					_container.RegisterNamedResolver<IRequestProcessor>(RequestTypes.GenerateInstallScript, c => new GenerateInstallScriptRequestProcessor(c.ResolveInstance<IBulletRepository>(), c.ResolveInstance<DocMAH.Configuration.IDataStoreConfiguration>(), c.ResolveInstance<HttpContextBase>(), c.ResolveInstance<IPageRepository>()));
					_container.RegisterNamedResolver<IRequestProcessor>(RequestTypes.JavaScript, c => new JavaScriptRequestProcessor());
					_container.RegisterNamedResolver<IRequestProcessor>(RequestTypes.MovePage, c => new MovePageRequestProcessor(c.ResolveInstance<IPageRepository>()));
					_container.RegisterNamedResolver<IRequestProcessor>(RequestTypes.NotFound, c => new NotFoundRequestProcessor());
					_container.RegisterNamedResolver<IRequestProcessor>(RequestTypes.ReadApplicationSettings, c => new ReadApplicationSettingsRequestProcessor(c.ResolveInstance<IEditAuthorizer>()));
					_container.RegisterNamedResolver<IRequestProcessor>(RequestTypes.ReadPage, c => new ReadPageRequestProcessor(c.ResolveInstance<IBulletRepository>(), c.ResolveInstance<IPageRepository>()));
					_container.RegisterNamedResolver<IRequestProcessor>(RequestTypes.ReadTableOfContents, c => new ReadTableOfContentsRequestProcessor(c.ResolveInstance<IEditAuthorizer>(), c.ResolveInstance<IPageRepository>()));
					_container.RegisterNamedResolver<IRequestProcessor>(RequestTypes.SavePage, c => new SaveHelpRequestProcessor(c.ResolveInstance<IBulletRepository>(), c.ResolveInstance<IPageRepository>()));
					_container.RegisterNamedResolver<IRequestProcessor>(RequestTypes.SaveUserPageSettings, c => new SaveUserPageSettingsRequestProcessor(c.ResolveInstance<HttpContextBase>()));
					_container.RegisterNamedResolver<IRequestProcessor>(RequestTypes.Unauthorized, c => new UnauthorizedRequestProcessor());

					// DataStore registration.
					_container.RegisterResolver<IBulletRepository>(c => new SqlBulletRepository(c.ResolveInstance<ISqlConnectionFactory>()));
					_container.RegisterResolver<IConfigurationRepository>(c => new SqlConfigurationRepository(c.ResolveInstance<ISqlConnectionFactory>()));
					_container.RegisterResolver<IDataStore>(c => new SqlDataStore(c.ResolveInstance<DocMAH.Configuration.IDataStoreConfiguration>(), c.ResolveInstance<ISqlConnectionFactory>()));
					_container.RegisterResolver<IPageRepository>(c => new SqlPageRepository(c.ResolveInstance<ISqlConnectionFactory>()));
					_container.RegisterResolver<IUserPageSettingsRepository>(c => new SqlUserPageSettingsRepository(c.ResolveInstance<ISqlConnectionFactory>()));

					// Content manager registration.
					_container.RegisterResolver<IHelpContentManager>(c => new HelpContentManager(c.ResolveInstance<HttpContextBase>(), c.ResolveInstance<IDataStore>(), c.ResolveInstance<IDataStoreConfiguration>()));

					// Configuration registration.
					_container.RegisterResolver<IContentConfiguration>(c => DocmahConfigurationSection.Current);
					_container.RegisterResolver<IDataStoreConfiguration>(c => new DataStoreConfiguration(c.ResolveInstance<IConfigurationRepository>()));
					_container.RegisterResolver<IDocumentationConfiguration>(c => DocmahConfigurationSection.Current.DocumentationConfiguration);
					_container.RegisterResolver<IEditHelpConfiguration>(c => DocmahConfigurationSection.Current.EditHelpConfiguration);

					// SqlDataStore registration.
					_container.RegisterResolver<ISqlConnectionFactory>(c => new SqlConnectionFactory(c.ResolveInstance<IContentConfiguration>()));

					// Authorization registration.
					_container.RegisterResolver<IEditAuthorizer>(c => new EditAuthorizer(c.ResolveInstance<HttpContextBase>(), c.ResolveInstance<IEditHelpConfiguration>()));

					// HttpContext registration.
					_container.RegisterResolver<HttpContextBase>(c => { return new HttpContextWrapper(HttpContext.Current); });
				}
			}

			return _container;
		}
		
		#endregion

	}
}
