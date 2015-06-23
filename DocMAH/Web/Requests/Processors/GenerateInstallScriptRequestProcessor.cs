using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using DocMAH.Data;
using DocMAH.Data.Sql;
using DocMAH.Extensions;
using System.Net;
using DocMAH.Configuration;
using DocMAH.Web.Authorization;
using DocMAH.Content;
using System.Web.Hosting;
using DocMAH.Adapters;
namespace DocMAH.Web.Requests.Processors
{
	[EditAuthorization]
	public class GenerateInstallScriptRequestProcessor : IRequestProcessor
	{
		#region Constructors

		public GenerateInstallScriptRequestProcessor(
			IPath path,
			IHelpContentManager helpContentManager)
		{
			_path = path;
			_helpContentManager = helpContentManager;
		}

		#endregion

		#region Private Fields

		private readonly IPath _path;
		private readonly IHelpContentManager _helpContentManager;

		#endregion

		#region IRequestProcessor Members

		public ResponseState Process(string data)
		{
			// There is an open task to make this path configurable.
			var fileName = Path.Combine(_path.MapPath("~"), ContentFileConstants.ContentFileName);

			// Write exported help to file.
			_helpContentManager.ExportContent(fileName);

			// Return results.
			return new ResponseState
			{
				Content = File.ReadAllText(fileName),
				ContentType = ContentTypes.Text,
				Disposition = string.Format("attachment;filename={0}", ContentFileConstants.ContentFileName),
			};
		}

		#endregion
	}
}
