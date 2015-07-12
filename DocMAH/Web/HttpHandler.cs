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
using DocMAH.Dependencies;
using DocMAH.Web.Authorization;

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
			: this(Registrar.Initialize())
		{
		}

		/// <summary>
		/// Test constructor.
		/// </summary>
		/// <param name="container"></param>
		public HttpHandler(IContainer container)
		{
			_editAuthorizer = container.Resolve<IEditAuthorizer>();
			_requestProcessorFactory = container.Resolve<IRequestProcessorFactory>();
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

		private bool IsAuthorized(IRequestProcessor processor)
		{
			var result = true;

			var attribute = (EditAuthorizationAttribute)processor.GetType().GetCustomAttributes(typeof(EditAuthorizationAttribute), false).FirstOrDefault();
			if (null != attribute && attribute.RequiresAuthorization)
				result = _editAuthorizer.Authorize();

			return result;
		}

		private static void WriteResponse(HttpContextBase context, ResponseState responseState)
		{
			context.Response.Cache.SetNoStore();
			context.Response.ContentType = responseState.ContentType;
			context.Response.StatusCode = (int)responseState.StatusCode;
			if (!string.IsNullOrEmpty(responseState.Disposition))
				context.Response.AddHeader("Content-Disposition", responseState.Disposition);
			context.Response.Write(responseState.Content);
			context.Response.Flush();
			context.Response.End();
		}


		#endregion

		#region Internal Methods

		internal void ProcessRequestInternal(HttpContextBase context)
		{
			// Read post data or request parameters
			var data = ReadPostData(context);
			if (string.IsNullOrEmpty(data))
				data = context.Request["id"];

			// Get the processor for the request and check authorization.
			var method = context.Request["m"];
			IRequestProcessor requestProcessor;
			try
			{
				requestProcessor = _requestProcessorFactory.Create(method);
			}
			catch (InvalidOperationException ex)
			{
				// Handle requests for invalid request types.
				if (!ex.Message.Contains("Dependency creator not registered")) throw;
				requestProcessor = _requestProcessorFactory.Create(RequestTypes.NotFound);
			}

			if (!IsAuthorized(requestProcessor))
			{
				// If authorization fails, replace the processor with the unauthorized processor.
				requestProcessor = _requestProcessorFactory.Create(RequestTypes.Unauthorized);
			}

			// Process the request
			var response = requestProcessor.Process(data);

			// Create not found response when processor can not find it's data.
			if (response.StatusCode == HttpStatusCode.NotFound)
			{
				requestProcessor = _requestProcessorFactory.Create(RequestTypes.NotFound);
				response = requestProcessor.Process(null);
			}

			// Return the response to the user.
			WriteResponse(context, response);
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
			ProcessRequestInternal(wrapper);
		}

		#endregion
	}
}
