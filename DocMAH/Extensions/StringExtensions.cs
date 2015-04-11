using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocMAH.Extensions
{
	public static class StringExtensions
	{
		public static string ToNullableSqlValue(this string value)
		{
			return null == value ? "NULL" : string.Format("'{0}'", value.Replace("'", "''"));
		}

		public static string ToSqlValue(this string value)
		{
			return string.Format("'{0}'",value.Replace("'", "''"));
		}
	}
}
