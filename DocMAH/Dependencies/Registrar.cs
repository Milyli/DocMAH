using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using DocMAH.Data;
using DocMAH.Data.Sql;
using DocMAH.Web.Authorization;
using DocMAH.Web.Requests;
using DocMAH.Web.Requests.Processors;

namespace DocMAH.Dependencies
{
	public class Registrar
	{
		private static Container _container;
		private static readonly object _lock = new object();

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
					_container.RegisterResolver<IRequestProcessorFactory>(c => new RequestProcessorFactory(c));
					_container.RegisterNamedResolver<IRequestProcessor>(RequestTypes.Css, c => new CssRequestProcessor());
					_container.RegisterNamedResolver<IRequestProcessor>(RequestTypes.DeletePage, c => new DeletePageRequestProcessor(c.ResolveInstance<IBulletRepository>(), c.ResolveInstance<IPageRepository>()));
					_container.RegisterNamedResolver<IRequestProcessor>(RequestTypes.DocumentationPage, c => new DocumentationPageRequestProcessor(c.ResolveInstance<HttpContextBase>()));
					_container.RegisterNamedResolver<IRequestProcessor>(RequestTypes.GenerateInstallScript, c => new GenerateInstallScriptRequestProcessor(c.ResolveInstance<IBulletRepository>(), c.ResolveInstance<IConfigurationService>(), c.ResolveInstance<HttpContextBase>(), c.ResolveInstance<IPageRepository>()));
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
					_container.RegisterResolver<IDataStore>(c => new SqlDataStore(c.ResolveInstance<IConfigurationService>(), c.ResolveInstance<ISqlConnectionFactory>()));
					_container.RegisterResolver<IPageRepository>(c => new SqlPageRepository(c.ResolveInstance<ISqlConnectionFactory>()));
					_container.RegisterResolver<IUserPageSettingsRepository>(c => new SqlUserPageSettingsRepository(c.ResolveInstance<ISqlConnectionFactory>()));

					// Service registration.
					_container.RegisterResolver<IConfigurationService>(c => new ConfigurationService(c.ResolveInstance<IConfigurationRepository>()));

					// SqlDataStore registration.
					_container.RegisterResolver<ISqlConnectionFactory>(c => new SqlConnectionFactory());

					// Authorization registration.
					_container.RegisterResolver<IEditAuthorizer>(c => new EditAuthorizer(c.ResolveInstance<HttpContextBase>()));

					// HttpContext registration.
					_container.RegisterResolver<HttpContextBase>(c => { return new HttpContextWrapper(HttpContext.Current); });
				}
			}

			return _container;
		}
	}
}
