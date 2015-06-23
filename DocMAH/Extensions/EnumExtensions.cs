using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocMAH.Extensions
{
	public static class EnumExtensions
	{
		/// <summary>
		/// Gets the max value of the specified enum type.
		/// </summary>
		/// <typeparam name="TEnum"></typeparam>
		/// <returns></returns>
		public static TEnum GetMaxValue<TEnum>()
		{
			var values = Enum.GetValues(typeof(TEnum));

			if (values.Length == 0)
				return default(TEnum);

			return (TEnum)values.GetValue(values.Length - 1);
		}
	}
}
