using System;
using System.Collections.Generic;
using System.Linq;

namespace DocMAH.Dependencies
{
	public interface IContainer
	{
		Dictionary<string, object> Configuration { get; }

		TValue GetConfiguration<TValue>(string name);

		void RegisterResolver<TDependency>(Func<Container, TDependency> resolver);
		void RegisterNamedResolver<TDependency>(string name, Func<Container, TDependency> resolver);

		TDependency ResolveInstance<TDependency>();
		TDependency ResolveNamedInstance<TDependency>(string name);
	}
}
