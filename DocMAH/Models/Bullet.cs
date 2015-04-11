using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocMAH.Models
{
	public class Bullet
	{
		public int Id { get; set; }
		public int PageId { get; set; }
		public int Number { get; set; }
		public string Text { get; set; }
		public int VerticalOffset { get; set; }
		public int HorizontalOffset { get; set; }
		public string OffsetElementId { get; set; }
		public int? DocVerticalOffset { get; set; }
		public int? DocHorizontalOffset { get; set; }
	}
}
