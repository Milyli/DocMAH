using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocMAH.Configuration
{
	using DocMAH.Data;
	/// <summary>
	/// Data store agnostic accessors of named configuration values residing in data store.
	/// </summary>
	public interface IDataStoreConfiguration
	{
		int HelpContentVersion { get; set; }
		int DataStoreSchemaVersion { get; set; }
	}
}
