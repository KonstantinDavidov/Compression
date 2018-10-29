using System;
using System.ComponentModel;

namespace GZipTest
{
	public static class Extensions
	{
		public static bool TryGetEnumValueFromDescription<T>(string description, out T result)
		{
			var fis = typeof(T).GetFields();

			result = default(T);

			foreach (var fi in fis)
			{
				var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

				if (attributes.Length > 0 && attributes[0].Description == description)
				{
					result = (T) Enum.Parse(typeof(T), fi.Name);
					return true;
				}
			}

			return false;
		}
	}
}
