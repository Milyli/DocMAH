using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DocMAH.Web.Requests.Processors;

namespace DocMAH.Web.Requests
{
	public class RequestProcessorFactory : IRequestProcessorFactory
	{
		#region Constructors

		static RequestProcessorFactory()
		{
			_processorTypes = new Dictionary<string, Type>();

			// Build dictionary of request types and the processor that handlers them.
			var assembly = Assembly.GetExecutingAssembly();
			var allTypes = assembly.GetTypes();
			foreach(var type in allTypes)
			{
				if (type.IsSubclassOf(typeof(IRequestProcessor)))
				{
					var processor = (IRequestProcessor)Activator.CreateInstance(type);
					_processorTypes.Add(processor.RequestType, type);
				}
			}
		}

		#endregion

		#region Private Fields

		private static Dictionary<string, Type> _processorTypes;

		#endregion

		#region Public Methods

		public IRequestProcessor Create(string requestType)
		{
			IRequestProcessor result;

			var resultType = _processorTypes[requestType];
			if (null == resultType)
				resultType = _processorTypes[RequestTypes.NotFound];

			result = (IRequestProcessor)Activator.CreateInstance(resultType);

			return result;
		}

		#endregion
	}
}
