using System;
using System.Collections.Generic;
using System.Text;

namespace Zyborg.Util
{
    public static class TypeExtensions
    {
		public static bool IsSignedInteger(this Type t)
		{
			return t == typeof(Int16) || t == typeof(Int32) || t == typeof(Int64);
		}

		public static bool IsUnsignedInteger(this Type t)
		{
			return t == typeof(UInt16) || t == typeof(UInt32) || t == typeof(UInt64);
		}

		public static bool IsInteger(this Type t)
		{
			return IsSignedInteger(t) || IsUnsignedInteger(t);
		}

		public static bool IsRealNumber(this Type t)
		{
			return t == typeof(Double) || t == typeof(Single) || t == typeof(Decimal);
		}

		public static bool IsNumber(this Type t)
		{
			return IsInteger(t) || IsRealNumber(t);
		}
	}
}
