using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using DocMAH.Configuration;
using DocMAH.Data;
using DocMAH.Data.Sql;
using DocMAH.Dependencies;
using Moq;
using NUnit.Framework;
using DocMAH.Content;
namespace DocMAH.UnitTests
{
	public class TestFixtureDataStoreManager
	{
		#region Constructors

		public TestFixtureDataStoreManager()
		{
			// Initialize a private container so that modifications do not interfere with tests.
			_container = Registrar.Initialize();
		}

		#endregion

		#region Private Fields

		private readonly IContainer _container;

		#endregion

		#region Public Methods

		public void CreateDataStore()
		{
			// Create data store for unit tests.
			var dataStore = _container.Resolve<IDataStore>();
			dataStore.Create();
			dataStore.Update();
		}

		public void DeleteDataStore()
		{
			var dataStore = _container.Resolve<IDataStore>();
			dataStore.Delete();
		}

		public void DeleteInstallFile(string fileName)
		{
			if (File.Exists(fileName))
			{
				File.Delete(fileName);
			}
		}

		public void ExportContent(string fileName)
		{
			var helpContentManager = _container.Resolve<IHelpContentManager>();
			helpContentManager.ExportContent(fileName);
		}

		public void ImportContent(string fileName)
		{
			var helpContentManager = _container.Resolve<IHelpContentManager>();
			helpContentManager.ImportContent(fileName);
		}

		#endregion
	}
}
