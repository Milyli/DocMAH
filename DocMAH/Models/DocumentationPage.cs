using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocMAH.Models
{
	public class DocumentationPage
	{
		#region Public Properties

		public int Id { get; set; }
		public int? ParentPageId { get; set; }
		public int Order { get; set; }
		public string Title { get; set; }
		public string Content { get; set; }
		public bool IsHidden { get; set; }
		
		#endregion
	}
}
