using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Zyborg.Vault.Helper.Logging
{
    public static class VaultLogger
    {
		public static ILoggerFactory CreateLoggerFactory(LogLevel level)
		{
			var lf = new LoggerFactory();
			lf.AddConsole(level);

			return lf;
		}
    }
}
