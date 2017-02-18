using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Zyborg.Util.IO
{
    public class Directory
    {
		public static string CreateTempDir(string path = null, string prefix = null)
		{
			if (path == null)
				path = Path.GetTempPath();

			var fullPath = Path.Combine(path, $"{prefix}{Path.GetRandomFileName()}");
			if (System.IO.Directory.Exists(fullPath))
				throw new InvalidOperationException("temporary directory already exists");

			var dir = System.IO.Directory.CreateDirectory(fullPath);
			return dir.FullName;
		}
    }
}
