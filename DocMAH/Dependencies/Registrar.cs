using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using DocMAH.Data;
using DocMAH.Data.Sql;
using DocMAH.Web.Authorization;

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


					// DataStore registration.
					_container.RegisterCreator<IBulletRepository>(c => new SqlBulletRepository(c.CreateInstance<ISqlConnectionFactory>()));
					_container.RegisterCreator<IConfigurationRepository>(c => new SqlConfigurationRepository(c.CreateInstance<ISqlConnectionFactory>()));
					_container.RegisterCreator<IDataStore>(c => new SqlDataStore(c.CreateInstance<IConfigurationService>(), c.CreateInstance<ISqlConnectionFactory>()));
					_container.RegisterCreator<IPageRepository>(c => new SqlPageRepository(c.CreateInstance<ISqlConnectionFactory>()));
					_container.RegisterCreator<IUserPageSettingsRepository>(c => new SqlUserPageSettingsRepository(c.CreateInstance<ISqlConnectionFactory>()));

					// Service registration.
					_container.RegisterCreator<IConfigurationService>(c => new ConfigurationService(c.CreateInstance<IConfigurationRepository>()));

					// SqlDataStore registration.
					_container.RegisterCreator<ISqlConnectionFactory>(c => new SqlConnectionFactory());

					// Authorization registration.
					_container.RegisterCreator<IEditAuthorizer>(c => new EditAuthorizer(c.CreateInstance<HttpContextBase>()));

					// HttpContext registration.
					_container.RegisterCreator<HttpContextBase>(c => new HttpContextWrapper(HttpContext.Current));
				}
			}

			return _container;
		}
	}
}
