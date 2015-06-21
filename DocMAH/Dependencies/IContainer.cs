using System;
using System.Collections.Generic;
using System.Linq;

namespace DocMAH.Dependencies
{
	public interface IContainer
	{
		Dictionary<string, object> Configuration { get; }

		TValue GetConfiguration<TValue>(string name);

		void Register<TDependency>(Func<Container, TDependency> resolver);
		void Register<TDependency>(string name, Func<Container, TDependency> resolver);

		TDependency Resolve<TDependency>();
		TDependency Resolve<TDependency>(string name);
	}
}
