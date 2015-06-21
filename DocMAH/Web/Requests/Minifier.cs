using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using DocMAH.Adapters;

namespace DocMAH.Web.Requests
{
	public class Minifier : IMinifier
	{
		#region Constructors

		public Minifier(IDebugger debugger, HttpContextBase httpContext)
		{
			_debugger = debugger;
			_httpContext = httpContext;
		}

		#endregion

		#region Private Fields

		private readonly IDebugger _debugger;
		private readonly HttpContextBase _httpContext;

		#endregion

		#region Public Methods

		public string Minify(string full, string minified)
		{
			return _debugger.IsAttached || (_httpContext.IsDebuggingEnabled) ? full : minified;
		}

		#endregion
	}
}
