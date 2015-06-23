using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace DocMAH.Dependencies
{
	/// <summary>
	/// This is a simple IoC container that satisfies the needs of DocMAH for now.
	/// We went this route to prevent the need of a dependent NuGet package
	/// that might conflict with other project package references.
	/// </summary>
	public class Container : IContainer
	{
		#region Private Fields

		private readonly Dictionary<string, object> configuration
					   = new Dictionary<string, object>();
		private readonly Dictionary<Type, Dictionary<string, object>> typeCreators
					   = new Dictionary<Type, Dictionary<string, object>>();

		private const string DefaultKey = "default";

		#endregion
		
		#region Public Properties

		public Dictionary<string, object> Configuration
		{
			get { return configuration; }
		}

		#endregion

		#region Public Methods

		public void Register<TDependency>(Func<Container, TDependency> resolver)
		{
			Register<TDependency>(DefaultKey, resolver);
		}

		public void Register<TDependency>(string name, Func<Container, TDependency> resolver)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name", "The name of the resolver is required.");
			if (null == resolver)
				throw new ArgumentNullException("resolver", "A resolver is required to create the desired instance.");

			var dependencyType = typeof(TDependency);
			if (!typeCreators.ContainsKey(dependencyType))
				typeCreators.Add(dependencyType, new Dictionary<string, object>());

			var namedCreators = typeCreators[dependencyType];

			namedCreators[name] = resolver;
		}

		public TDependency Resolve<TDependency>()
		{
			return Resolve<TDependency>(DefaultKey);
		}

		public TDependency Resolve<TDependency>(string name)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name", "A name must be provided to resolve a named dependency.");

			if (!typeCreators.ContainsKey(typeof(TDependency)))
				throw new InvalidOperationException(string.Format("Dependency creator not registered for type '{0}'.", typeof(TDependency).FullName));
			var namedCreators = typeCreators[typeof(TDependency)];

			if (!namedCreators.ContainsKey(name))
				throw new InvalidOperationException(string.Format("Dependency creator not registered for type '{0}' with name '{1}'.", typeof(TDependency).FullName, name));
			var creator = namedCreators[name];

			return ((Func<Container, TDependency>)creator)(this);
		}

		public T GetConfiguration<T>(string name)
		{
			return (T)configuration[name];
		}

		#endregion
	}
}
