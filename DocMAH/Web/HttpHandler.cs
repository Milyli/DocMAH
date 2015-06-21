using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using DocMAH.Configuration;
using DocMAH.Models;
using DocMAH.Properties;
using DocMAH.Extensions;
using DocMAH.Data;
using DocMAH.Web.Requests;
using DocMAH.Web.Authorization;
using DocMAH.Dependencies;

namespace DocMAH.Web
{
	public class HttpHandler : IHttpHandler
	{
		#region Constructors

		/// <summary>
		/// Runtime constructor.
		/// The handler is instantiated by the framework so I can't change this.
		/// </summary>
		public HttpHandler()
			: this((IContainer)HttpContext.Current.Items[HttpModule.ContextKey])
		{
		}

		/// <summary>
		/// Test constructor.
		/// </summary>
		/// <param name="container"></param>
		public HttpHandler(IContainer container)
		{
			_editAuthorizer = container.ResolveInstance<IEditAuthorizer>();
			_requestProcessorFactory = container.ResolveInstance<IRequestProcessorFactory>();
		}

		#endregion

		#region Private Fields

		private readonly IRequestProcessorFactory _requestProcessorFactory;
		private readonly IEditAuthorizer _editAuthorizer;

		#endregion

		#region Private Methods

		private static string ReadPostData(HttpContextBase context)
		{
			var result = string.Empty;
			using (var reader = new StreamReader(context.Request.InputStream))
			{
				result = reader.ReadToEnd();
			}
			return result;
		}

		private static void WriteResponse(HttpContextBase context, ResponseState responseState)
		{
			context.Response.Cache.SetNoStore();
			context.Response.ContentType = responseState.ContentType;
			context.Response.StatusCode = (int)responseState.StatusCode;
			if (!string.IsNullOrEmpty(responseState.Disposition))
				context.Response.AddHeader("Content-Disposition", responseState.Disposition);
			context.Response.Cache.SetNoStore();
			context.Response.Write(responseState.Content);
			context.Response.Flush();
			context.Response.End();
		}


		#endregion

		#region IHttpHandler Members

		public bool IsReusable
		{
			get { return false; }
		}

		public void ProcessRequest(HttpContext context)
		{
			var wrapper = new HttpContextWrapper(context);
			ProcessWrappedRequest(wrapper);
		}

		public void ProcessWrappedRequest(HttpContextBase context)
		{
			// Read post data or request parameters
			var data = ReadPostData(context);
			if (string.IsNullOrEmpty(data))
				data = context.Request["id"];

			// Get the processor for the request and check authorization.
			var method = context.Request["m"];
			var requestProcessor = _requestProcessorFactory.Create(method);
			if (requestProcessor.RequiresEditAuthorization && !_editAuthorizer.Authorize())
			{
				// If authorization fails, replace the processor with the unauthorized processor.
				requestProcessor = _requestProcessorFactory.Create(RequestTypes.Unauthorized);
			}

			// Process the request and return the response.
			var response = requestProcessor.Process(data);
			WriteResponse(context, response);
		}

		#endregion
	}
}
