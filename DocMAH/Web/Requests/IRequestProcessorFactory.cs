using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocMAH.Web.Requests
{
	public interface IRequestProcessorFactory
	{
		IRequestProcessor Create(string requestType);
	}
}
