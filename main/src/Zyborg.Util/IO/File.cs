using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Zyborg.Util.IO
{
    public class File
    {
		public static string CreateTempFile(string path = null, string prefix = null)
		{
			if (path == null)
				path = Path.GetTempPath();

			var fullPath = Path.Combine(path, $"{prefix}{Path.GetRandomFileName()}");
			using (var fs = System.IO.File.Open(fullPath, FileMode.CreateNew))
			{
				return Path.GetFullPath(fullPath);
			}
		}
	}
}
