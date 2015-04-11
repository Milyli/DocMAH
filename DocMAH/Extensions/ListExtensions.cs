using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocMAH.Extensions
{
	public static class ListExtensions
	{
		public static string ToCsv<Type>(this List<Type> values)
		{
			var result = new StringBuilder();
			foreach (var value in values)
			{
				result.Append(value);
				result.Append(',');
			}
			return result.ToString().Remove(result.Length - 1);
		}
	}
}
