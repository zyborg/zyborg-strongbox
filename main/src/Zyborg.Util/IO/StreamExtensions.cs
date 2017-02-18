using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Zyborg.Util.IO
{
    public static class StreamExtensions
    {
		public static void Write(this Stream s, byte[] buffer)
		{
			s.Write(buffer, 0, buffer.Length);
		}
    }
}
