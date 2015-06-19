using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DocMAH.Dependencies;
using DocMAH.Web.Requests.Processors;

namespace DocMAH.Web.Requests
{
	public class RequestProcessorFactory : IRequestProcessorFactory
	{
		#region Constructors

		public RequestProcessorFactory(IContainer container)
		{
			_container = container;
		}

		#endregion

		#region Private Fields

		private readonly IContainer _container;

		#endregion

		#region Public Methods

		public IRequestProcessor Create(string requestType)
		{
			return _container.ResolveNamedInstance<IRequestProcessor>(requestType);
		}

		#endregion
	}
}
