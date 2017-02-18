using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Zyborg.Vault.Logical
{
	[TestClass]
    public class InmemStorageTests
    {
		[TestMethod]
		//~ func TestInmemStorage(t *testing.T) {
		//~ 	TestStorage(t, new(InmemStorage))
		//~ }
		public void TestInmemStorage()
		{
			Tests.TestStorage(new InmemStorage());
		}
    }
}
