using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Zyborg.Util
{
    public static class ProcessExtensions
    {
		/// <summary>
		/// An approximation for argv[0].
		/// </summary>
		/// <returns></returns>
		public static string GetArv0()
		{
			var p = Process.GetCurrentProcess();
			var m = p.MainModule;
			return m.FileName;
		}
    }
}
