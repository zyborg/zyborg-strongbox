using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Zyborg.Vault.Helper.Salt
{
	[TestClass]
    public class SaltTests
    {
		[TestMethod]
		//~ func TestSalt(t *testing.T) {
		public void TestSalt()
		{
			//~ inm := &logical.InmemStorage{}
			//~ conf := &Config{}
			var inm = new Logical.InmemStorage();
			var conf = new Config();

			//~ salt, err := NewSalt(inm, conf)
			//~ if err != nil {
			//~ 	t.Fatalf("err: %v", err)
			//~ }
			var salt = Salt.NewSalt(inm, conf);

			//~ if !salt.DidGenerate() {
			//~ 	t.Fatalf("expected generation")
			//~ }
			Assert.IsTrue(salt.DidGenerate());

			// Verify the salt exists
			//~ out, err := inm.Get(DefaultLocation)
			//~ if err != nil {
			//~ 	t.Fatalf("err: %v", err)
			//~ }
			//~ if out == nil {
			//~ 	t.Fatalf("missing salt")
			//~ }
			var @out = inm.Get(Salt.DefaultLocation);
			Assert.IsNotNull(@out);

			// Create a new salt, should restore
			//~ salt2, err := NewSalt(inm, conf)
			//~ if err != nil {
			//~ 	t.Fatalf("err: %v", err)
			//~ }
			var salt2 = Salt.NewSalt(inm, conf);

			//~ if salt2.DidGenerate() {
			//~ 	t.Fatalf("unexpected generation")
			//~ }
			Assert.IsFalse(salt2.DidGenerate());

			// Check for a match
			//~ if salt.salt != salt2.salt {
			//~ 	t.Fatalf("salt mismatch: %s %s", salt.salt, salt2.salt)
			//~ }
			Assert.AreEqual(salt.SaltValue, salt.SaltValue);

			// Verify a match
			//~ id := "foobarbaz"
			//~ sid1 := salt.SaltID(id)
			//~ sid2 := salt2.SaltID(id)
			//~ 
			//~ if sid1 != sid2 {
			//~ 	t.Fatalf("mismatch")
			//~ }
			var id = "foobarbaz";
			var sid1 = salt.SaltID(id);
			var sid2 = salt2.SaltID(id);
			Assert.AreEqual(sid1, sid2);
		}

		[TestMethod]
		//~ func TestSaltID(t *testing.T) {
		public void TestSaltID()
		{
			//~ salt, err := uuid.GenerateUUID()
			//~ if err != nil {
			//~ 	t.Fatal(err)
			//~ }
			//~ id := "foobarbaz"
			var salt = Guid.NewGuid().ToString();
			var id = "foobarbaz";

			//~ sid1 := SaltID(salt, id, SHA1Hash)
			//~ sid2 := SaltID(salt, id, SHA1Hash)
			var sid1 = Salt.SaltID(salt, id, Salt.SHA1Hash);
			var sid2 = Salt.SaltID(salt, id, Salt.SHA1Hash);

			//~ if len(sid1) != sha1.Size*2 {
			//~ 	t.Fatalf("Bad len: %d %s", len(sid1), sid1)
			//~ }
			Assert.AreEqual(SHA1.Create().HashSize / 8 * 2, sid1.Length);

			//~ if sid1 != sid2 {
			//~ 	t.Fatalf("mismatch")
			//~ }
			Assert.AreEqual(sid1, sid2);

			//~ sid1 = SaltID(salt, id, SHA256Hash)
			//~ sid2 = SaltID(salt, id, SHA256Hash)
			sid1 = Salt.SaltID(salt, id, Salt.SHA256Hash);
			sid2 = Salt.SaltID(salt, id, Salt.SHA256Hash);

			//~if len(sid1) != sha256.Size*2 {
			//~	t.Fatalf("Bad len: %d", len(sid1))
			//~}
			Assert.AreEqual(SHA256.Create().HashSize / 8 * 2, sid1.Length);

			//~if sid1 != sid2 {
			//~	t.Fatalf("mismatch")
			//~}
			Assert.AreEqual(sid1, sid2);
		}
	}
}
