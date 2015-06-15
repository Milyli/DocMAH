using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocMAH.Data
{
	/// <summary>
	/// Methods for managing configuration values stored in the data store.
	/// </summary>
	public interface IConfigurationRepository
	{
		/// <summary>
		/// Read a configuration value from the data store by name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		int Read(string name);

		/// <summary>
		/// /Update a configuration value in the data store.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		void Update(string name, int value);
	}
}
