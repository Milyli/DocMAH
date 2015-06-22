using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using DocMAH.Extensions;

namespace DocMAH.Web.Requests
{
	public interface IRequestProcessor
	{
		/// <summary>
		/// Process the request.
		/// </summary>
		/// <param name="responseState">Various response state values to override.</param>
		/// <returns>Data that should be written to the response stream.</returns>
		ResponseState Process(string data);
	}
}
