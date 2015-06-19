using System;
using System.Collections.Generic;
using System.Linq;

namespace DocMAH.Dependencies
{
	public interface IContainer
	{
		Dictionary<string, object> Configuration { get; }

		T CreateInstance<T>();
		T CreateNamedInstance<T>(string name);

		T GetConfiguration<T>(string name);

		void RegisterCreator<T>(Container.Creator creator);
		void RegisterNamedCreator<T>(Container.Creator creator, string name);
	}
}
