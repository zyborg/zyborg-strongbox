using System;
using System.Collections.Generic;
using System.Text;

namespace Zyborg.Util
{
    public static class BytesExtensions
    {
		public static string ToUtf8String(this byte[] b)
		{
			return System.Text.Encoding.UTF8.GetString(b);
		}
    }
}
