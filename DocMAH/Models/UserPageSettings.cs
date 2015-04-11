using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocMAH.Models
{
	public class UserPageSettings
	{
		public int Id { get; set; }
		public int PageId { get; set; }
		public bool HidePage { get; set; }
		public string UserName { get; set; }
	}
}
