using System;
using System.Collections.Generic;
using System.Text;

namespace Zyborg.Util
{
    public static class CopyExtensions
    {
		public static IDictionary<TKey, TValue> DeepCopy<TKey, TValue>(
				this IDictionary<TKey, TValue> orig, Type dType = null)
		{
			if (dType == null)
				dType = typeof(Dictionary<TKey, TValue>);

			var copy = (IDictionary<TKey, TValue>)Activator.CreateInstance(dType, orig);
			return copy;
		}

		public static T[] DeepCopy<T>(this T[] orig)
		{
			var copy = (T[])Array.CreateInstance(typeof(T), orig.Length);
			Array.Copy(orig, copy, orig.Length);
			return copy;
		}

		public static IList<T> DeepCopy<T>(this IList<T> orig, Type lType = null)
		{
			if (lType == null)
				lType = typeof(List<T>);
			var copy = (IList<T>)Activator.CreateInstance(lType, orig);
			return copy;
		}
    }
}
