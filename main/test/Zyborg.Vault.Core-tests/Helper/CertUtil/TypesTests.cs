using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zyborg.Vault.Helper.CertUtil
{
	[TestClass]
    public class TypesTests
    {
		[TestMethod]
		public void TestPrivateKeyType()
		{
			var x = PrivateKeyType.From(string.Empty);
			var y = PrivateKeyType.From("rsa");
			var z = PrivateKeyType.From("ec");

			Assert.AreSame(PrivateKeyType.UnknownPrivateKey, x);
			Assert.AreSame(PrivateKeyType.RSAPrivateKey, y);
			Assert.AreSame(PrivateKeyType.ECPrivateKey, z);

			Assert.IsFalse(PrivateKeyType.Labels().Contains("foo"));
			var a = PrivateKeyType.From("foo");
			Assert.IsTrue(PrivateKeyType.Labels().Contains("foo"));
			var b = PrivateKeyType.From("foo");
			Assert.AreSame(a, b);

		}

		[TestMethod]
		public void TestTLSUsage()
		{
			var x = TLSUsage.From(0);
			Assert.AreEqual(TLSUsage.TLSUnknown.Label, x.Label);
			Assert.AreSame(TLSUsage.TLSUnknown, x);
		}
    }
}
