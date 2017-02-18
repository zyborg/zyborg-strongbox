using System;
using System.Collections.Generic;
using System.Text;

namespace Zyborg.Vault.Physical
{
	internal static class Constants
	{
		//const DefaultParallelOperations = 128
		public const int DefaultParallelOperations = 128;

		// DefaultCacheSize is used if no cache size is specified for NewCache
		public const int DefaultCacheSize = 32 * 1024;
	}
}
