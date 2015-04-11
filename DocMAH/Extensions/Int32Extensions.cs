using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocMAH.Extensions
{
	public static class Int32Extensions
	{
		public static string ToNullableSqlValue(this int? value)
		{
			return value.HasValue ? value.ToString() : "NULL";
		}
	}
}
