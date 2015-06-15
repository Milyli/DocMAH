using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocMAH.Extensions
{
	public static class EnumExtensions
	{
		public static TResult GetMaxValue<TEnum, TResult>()
		{
			var values = Enum.GetValues(typeof(TEnum));

			if (values.Length == 0)
				return default(TResult);

			return (TResult)values.GetValue(values.Length - 1);
		}
	}
}
