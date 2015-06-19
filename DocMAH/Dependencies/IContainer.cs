using System;
using System.Collections.Generic;
using System.Linq;

namespace DocMAH.Dependencies
{
	public interface IContainer
	{
		Dictionary<string, object> Configuration { get; }

		T GetConfiguration<T>(string name);

		void RegisterCreator<T>(Container.Creator creator);
		void RegisterNamedCreator<T>(string name, Container.Creator creator);

		T ResolveInstance<T>();
		T ResolveNamedInstance<T>(string name);
	}
}
