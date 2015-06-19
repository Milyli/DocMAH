using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocMAH.Dependencies
{
	public class Container : IContainer
	{
		#region Private Fields

		private readonly Dictionary<string, object> configuration
					   = new Dictionary<string, object>();
		private readonly Dictionary<Type, Dictionary<string, Creator>> typeCreators
					   = new Dictionary<Type, Dictionary<string, Creator>>();

		private const string DefaultKey = "default";

		#endregion

		#region Delegation

		public delegate object Creator(Container container);

		#endregion

		#region Public Properties

		public Dictionary<string, object> Configuration
		{
			get { return configuration; }
		}

		#endregion

		#region Public Methods

		public void RegisterCreator<T>(Creator creator)
		{
			RegisterNamedCreator<T>(DefaultKey, creator);
		}

		public void RegisterNamedCreator<T>(string name, Creator creator)
		{
			var namedCreators = typeCreators[typeof(T)];
			if (null == namedCreators)
			{
				namedCreators = new Dictionary<string, Creator>();
				typeCreators.Add(typeof(T), namedCreators);
			}

			namedCreators[name] = creator;
		}

		public T ResolveInstance<T>()
		{
			return ResolveNamedInstance<T>(DefaultKey);
		}

		public T ResolveNamedInstance<T>(string name)
		{
			var namedCreators = typeCreators[typeof(T)];
			if (null == namedCreators)
				throw new InvalidOperationException(string.Format("Dependency creator not registered for type '{0}'.", typeof(T).FullName));

			var creator = namedCreators[name];
			if (null == creator)
				throw new InvalidOperationException(string.Format("Dependency creator not registered for type '{0}' with name '{1}'.", typeof(T).FullName, name));

			return (T)creator(this);
		}

		public T GetConfiguration<T>(string name)
		{
			return (T)configuration[name];
		}

		#endregion
	}
}
