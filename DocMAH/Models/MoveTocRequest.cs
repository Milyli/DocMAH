using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocMAH.Models
{
	public class MoveTocRequest
	{
		public int PageId { get; set; }
		public int? NewParentId { get; set; }
		public int NewPosition { get; set; }
	}
}
