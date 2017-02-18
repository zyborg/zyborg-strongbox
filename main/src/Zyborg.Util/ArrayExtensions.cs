using System;
using System.Collections.Generic;
using System.Text;

namespace Zyborg.Util
{
    public static class ArrayExtensions
    {
		public static T[] Append<T>(this T[] array, params T[] values)
		{
			T[] newArray = new T[array.Length + values.Length];
			Array.Copy(array, newArray, array.Length);
			Array.Copy(values, 0, newArray, array.Length, values.Length);
			return newArray;
		}
    }
}
