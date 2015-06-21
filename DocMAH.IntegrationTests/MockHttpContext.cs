using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Moq;

namespace DocMAH.IntegrationTests
{
	public class MockHttpContext : IDisposable
	{
		#region Constructors
		/// <summary>
		/// Class that mocks HttpContext.
		/// </summary>
		public MockHttpContext()
		{
			_applicationStateMock = new Mock<HttpApplicationStateBase>();
			_contextMock = new Mock<HttpContextBase>();
			_requestMock = new Mock<HttpRequestBase>();
			_responseMock = new Mock<HttpResponseBase>();
			_sessionMock = new Mock<HttpSessionStateBase>();
			_serverMock = new Mock<HttpServerUtilityBase>();
			_paramsCollection = new NameValueCollection();
			_cachePolicyMock = new Mock<HttpCachePolicyBase>();
			_postedFiles = new HttpFileCollectionMock();

			_contextMock.Setup(c => c.Application).Returns(_applicationStateMock.Object);
			_contextMock.Setup(c => c.Request).Returns(_requestMock.Object);
			_contextMock.Setup(c => c.Response).Returns(_responseMock.Object);
			_contextMock.Setup(c => c.Session).Returns(_sessionMock.Object);
			_contextMock.Setup(c => c.Server).Returns(_serverMock.Object);
			_requestMock.Setup(c => c.Params).Returns(_paramsCollection);
			_responseMock.Setup(c => c.Cache).Returns(_cachePolicyMock.Object);
			_requestMock.Setup(m => m.Files).Returns(_postedFiles);
			_requestMock.Setup(r => r.InputStream).Returns(() => { return _requestStream; });

			_responseStream = new MemoryStream();
			_responseMock.Setup(m => m.OutputStream).Returns(_responseStream);
		}

		public void Dispose()
		{
			if (_requestStream != null)
			{
				_requestStream.Dispose();
				_requestStream = null;
			}
			if (_responseStream != null)
			{
				_responseStream.Dispose();
				_responseStream = null;
			}
		}

		#endregion

		#region Private Fields

		private readonly Mock<HttpApplicationStateBase> _applicationStateMock;
		private readonly Mock<HttpContextBase> _contextMock;
		private readonly Mock<HttpRequestBase> _requestMock;
		private readonly Mock<HttpResponseBase> _responseMock;
		private readonly Mock<HttpSessionStateBase> _sessionMock;
		private readonly Mock<HttpServerUtilityBase> _serverMock;
		private readonly NameValueCollection _paramsCollection;
		private readonly Mock<HttpCachePolicyBase> _cachePolicyMock;
		private readonly HttpFileCollectionMock _postedFiles;

		private MemoryStream _requestStream = new MemoryStream();
		private MemoryStream _responseStream; // Response value written to this string builder.

		#endregion

		#region Public Properties

		public HttpContextBase Object { get { return _contextMock.Object; } }

		public string Response
		{
			get
			{
				_responseStream.Position = 0;
				var reader = new StreamReader(_responseStream);
				return reader.ReadToEnd();
			}
		}

		#endregion

		//public static HttpContextBase FakeHttpContext(this MockRepository mocks, string url)
		//{
		//    HttpContextBase context = FakeHttpContext(mocks);
		//    context.Request.SetupRequestUrl(url);
		//    return context;
		//}

		//public static void SetFakeControllerContext(this MockRepository mocks, Controller controller)
		//{
		//    var httpContext = mocks.FakeHttpContext();
		//    ControllerContext context = new ControllerContext(new RequestContext(httpContext, new RouteData()), controller);
		//    controller.ControllerContext = context;
		//}

		//static string GetUrlFileName(string url)
		//{
		//    if (url.Contains("?"))
		//        return url.Substring(0, url.IndexOf("?"));
		//    else
		//        return url;
		//}

		//static NameValueCollection GetQueryStringParameters(string url)
		//{
		//    if (url.Contains("?"))
		//    {
		//        NameValueCollection parameters = new NameValueCollection();

		//        string[] parts = url.Split("?".ToCharArray());
		//        string[] keys = parts[1].Split("&".ToCharArray());

		//        foreach (string key in keys)
		//        {
		//            string[] part = key.Split("=".ToCharArray());
		//            parameters.Add(part[0], part[1]);
		//        }

		//        return parameters;
		//    }
		//    else
		//    {
		//        return null;
		//    }
		//}

		//public static void SetHttpMethodResult(this HttpRequestBase request, string httpMethod)
		//{
		//    SetupResult.For(request.HttpMethod).Return(httpMethod);
		//}

		public void AddApplicationState(string key, object value)
		{
			_applicationStateMock.Setup(a => a[key]).Returns(value);
		}

		/// <summary>
		/// Adds a byte array to the uploaded file collection of the request.
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="contents"></param>
		public void AddUploadedFile(string fileName, byte[] contents)
		{
			_postedFiles.AddFile(fileName, contents);
		}

		/// <summary>
		/// Sets the referrer Url on the HttpRequest.
		/// </summary>
		/// <param name="referrer">Provide a string that will be returned as a URI for calls to the ReferrerUrl property on the HttpRequestObject.</param>
		public void SetReferrerUrl(string referrer)
		{
			_requestMock.Setup(m => m.UrlReferrer).Returns(new Uri(referrer));
		}

		public void SetRequestContent(string content)
		{
			_requestStream = new MemoryStream(Encoding.UTF8.GetBytes(content));
		}

		/// <summary>
		/// Sets headers on the HttpRequest.
		/// </summary>
		/// <param name="headers">Provide a NameValueCollection that will be returned in code for calls to the Headers property on the HttpRequest object.</param>
		public void SetRequestHeaders(NameValueCollection headers)
		{
			_requestMock.Setup(r => r.Headers).Returns(headers);
		}

		public void SetRequestParameter(string key, string value)
		{
			_requestMock.Setup(r => r[key]).Returns(value);
		}

		public void SetRequestUrl(string url)
		{
			_requestMock.Setup(m => m.Url).Returns(new Uri(url));
		}

		///// <summary>
		///// Sets a value for the MapPath method on HttpServerUtilities object.
		///// </summary>
		///// <param name="serverPath">The path that should be returned as if on the server.</param>
		//public void SetMapPath(string serverPath)
		//{
		//    _serverMock.Setup(s => s.MapPath(It.IsAny<string>())).Returns(serverPath);
		//}

		/// <summary>
		/// Sets a value for the MapPath method on HttpServerUtilities object.
		/// </summary>
		/// <param name="webPath">The expected web url that identifies the resource.</param>
		/// <param name="serverPath">The local path that should be returned of the location of the file on the server.</param>
		public void SetMapPath(string webPath, string serverPath)
		{
			_serverMock.Setup(s => s.MapPath(webPath)).Returns(serverPath);
		}

		/// <summary>
		/// Sets values in the session that will be used by code.
		/// </summary>
		/// <param name="key">The key of the value that will be requested.</param>
		/// <param name="value">The value that will be returned.</param>
		public void SetSessionValue(string key, object value)
		{
			_sessionMock.SetupGet(s => s[key]).Returns(value);
		}
	}

	public class HttpFileCollectionMock : HttpFileCollectionBase
	{
		public void AddFile(string fileName, byte[] contents)
		{
			var file = new HttpFileMock(fileName, contents);

			BaseAdd(fileName, file);
		}

		public override HttpPostedFileBase this[int index]
		{
			get { return (HttpPostedFileBase)BaseGet(index); }
		}
	}

	public class HttpFileMock : HttpPostedFileBase
	{
		public HttpFileMock(string fileName, byte[] contents)
		{
			_fileName = fileName;
			_inputStream = new MemoryStream(contents);
		}

		private string _fileName;
		private Stream _inputStream;

		public override int ContentLength { get { return (int)_inputStream.Length; } }
		public override string FileName { get { return _fileName; } }
		public override Stream InputStream { get { return _inputStream; } }

	}

}
