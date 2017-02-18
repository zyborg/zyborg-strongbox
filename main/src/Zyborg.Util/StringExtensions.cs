using System;
using System.Collections.Generic;
using System.Text;

namespace Zyborg.Util
{
    public static class StringExtensions
    {
		public static byte[] ToUtf8Bytes(this string s)
		{
			return System.Text.Encoding.UTF8.GetBytes(s);
		}
    }
}
