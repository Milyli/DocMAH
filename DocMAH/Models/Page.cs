using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocMAH.Models
{
	public class Page
	{
		#region Constructors

		public Page()
		{
			Bullets = new List<Bullet>();
		}

		#endregion

		// Database Fields
		public int Id { get; set; }
		public PageTypes PageType { get; set; }
		public int? ParentPageId { get; set; }
		public int Order { get; set; }
		public string SourceUrl { get; set; }
		public string Title { get; set; }
		public string Content { get; set; }
		public int? VerticalOffset { get; set; }
		public int? HorizontalOffset { get; set; }
		public string OffsetElementId { get; set; }
		public string DocImageUrl { get; set; }
		public int? DocVerticalOffset { get; set; }
		public int? DocHorizontalOffset { get; set; }
		public bool IsHidden { get; set; }

		public string MatchUrls { get; set; }
		public List<Bullet> Bullets { get; set;}
	}
}
