using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Zyborg.Vault.Helper.Logging;

namespace Zyborg.Vault.Physical
{
	[TestClass]
    public class InmemTests
    {
		[TestMethod]
		public void TestInmem()
		{
			using (var lf = VaultLogger.CreateLoggerFactory(LogLevel.Trace))
			{
				var logger = lf.CreateLogger<IBackend>();

				var inm = InmemBackendFactory.INSTANCE.CreateBackend(null, logger);
				PhysicalTests.TestBackend(inm);
				PhysicalTests.TestBackend_ListPrefix(inm);
			}
		}
	}
}
