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
					_container.RegisterCreator<IContainer>(c => _container);

					// Request Process registration.
					_container.RegisterNamedCreator<IRequestProcessor>(RequestTypes.Css, c => new CssRequestProcessor());
					_container.RegisterNamedCreator<IRequestProcessor>(RequestTypes.DeletePage, c => new DeletePageRequestProcessor(c.ResolveInstance<IBulletRepository>(), c.ResolveInstance<IPageRepository>()));
					_container.RegisterNamedCreator<IRequestProcessor>(RequestTypes.DocumentationPage, c => new DocumentationPageRequestProcessor(c.ResolveInstance<HttpContextBase>()));
					_container.RegisterNamedCreator<IRequestProcessor>(RequestTypes.GenerateInstallScript, c => new GenerateInstallScriptRequestProcessor(c.ResolveInstance<IBulletRepository>(), c.ResolveInstance<IConfigurationService>(), c.ResolveInstance<HttpContextBase>(), c.ResolveInstance<IPageRepository>()));
					_container.RegisterNamedCreator<IRequestProcessor>(RequestTypes.JavaScript, c => new JavaScriptRequestProcessor());
					_container.RegisterNamedCreator<IRequestProcessor>(RequestTypes.MovePage, c => new MovePageRequestProcessor(c.ResolveInstance<IPageRepository>()));
					_container.RegisterNamedCreator<IRequestProcessor>(RequestTypes.NotFound, c => new NotFoundRequestProcessor());
					_container.RegisterNamedCreator<IRequestProcessor>(RequestTypes.ReadApplicationSettings, c => new ReadApplicationSettingsRequestProcessor(c.ResolveInstance<IEditAuthorizer>()));
					_container.RegisterNamedCreator<IRequestProcessor>(RequestTypes.ReadPage, c => new ReadPageRequestProcessor(c.ResolveInstance<IBulletRepository>(), c.ResolveInstance<IPageRepository>()));
					_container.RegisterNamedCreator<IRequestProcessor>(RequestTypes.ReadTableOfContents, c => new ReadTableOfContentsRequestProcessor(c.ResolveInstance<IEditAuthorizer>(), c.ResolveInstance<IPageRepository>()));
					_container.RegisterNamedCreator<IRequestProcessor>(RequestTypes.SavePage, c => new SaveHelpRequestProcessor(c.ResolveInstance<IBulletRepository>(), c.ResolveInstance<IPageRepository>()));
					_container.RegisterNamedCreator<IRequestProcessor>(RequestTypes.SaveUserPageSettings, c => new SaveUserPageSettingsRequestProcessor(c.ResolveInstance<HttpContextBase>()));
					_container.RegisterNamedCreator<IRequestProcessor>(RequestTypes.Unauthorized, c => new UnauthorizedRequestProcessor());

					// DataStore registration.
					_container.RegisterCreator<IBulletRepository>(c => new SqlBulletRepository(c.ResolveInstance<ISqlConnectionFactory>()));
					_container.RegisterCreator<IConfigurationRepository>(c => new SqlConfigurationRepository(c.ResolveInstance<ISqlConnectionFactory>()));
					_container.RegisterCreator<IDataStore>(c => new SqlDataStore(c.ResolveInstance<IConfigurationService>(), c.ResolveInstance<ISqlConnectionFactory>()));
					_container.RegisterCreator<IPageRepository>(c => new SqlPageRepository(c.ResolveInstance<ISqlConnectionFactory>()));
					_container.RegisterCreator<IUserPageSettingsRepository>(c => new SqlUserPageSettingsRepository(c.ResolveInstance<ISqlConnectionFactory>()));

					// Service registration.
					_container.RegisterCreator<IConfigurationService>(c => new ConfigurationService(c.ResolveInstance<IConfigurationRepository>()));

					// SqlDataStore registration.
					_container.RegisterCreator<ISqlConnectionFactory>(c => new SqlConnectionFactory());

					// Authorization registration.
					_container.RegisterCreator<IEditAuthorizer>(c => new EditAuthorizer(c.ResolveInstance<HttpContextBase>()));

					// HttpContext registration.
					_container.RegisterCreator<HttpContextBase>(c => new HttpContextWrapper(HttpContext.Current));
				}
			}

			return _container;
		}
	}
}
