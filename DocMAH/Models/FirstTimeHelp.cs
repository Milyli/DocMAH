using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocMAH.Models
{
	public class FirstTimeHelp
	{
		#region Constructors

		public FirstTimeHelp()
		{
			Bullets = new List<Bullet>();
		}

		#endregion

		#region Public Properties

		public int Id { get; set; }
		public string SourceUrl { get; set; }
		public string Title { get; set; }
		public string Content { get; set; }
		public int VerticalOffset { get; set; }
		public int HorizontalOffset { get; set; }
		public string OffsetElementId { get; set; }

		public List<Bullet> Bullets { get; set; }
		public string MatchUrls { get; set; }
		#endregion
	}
}
