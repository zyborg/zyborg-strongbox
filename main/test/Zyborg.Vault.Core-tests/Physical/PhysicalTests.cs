using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zyborg.Util;
using Zyborg.Vault.Helper.Logging;

namespace Zyborg.Vault.Physical
{
    [TestClass]
    public class PhysicalTests
    {
		[TestMethod]
		public void TestNewBackend()
		{
			using (var lf = VaultLogger.CreateLoggerFactory(LogLevel.Trace))
			{
				var logger = lf.CreateLogger<IBackend>();

				Assert.ThrowsException<Exception>(
					() => Global.NewBackend("foobar", logger, null),
					"Expected exception for non-existent 'foobar' backend");

				var b = Global.NewBackend("inmem", logger, null);
				Assert.IsNotNull(b, "Expected 'inmem' backend");
			}
		}

		public static void TestBackend(IBackend b)
		{
			// Should be empty
			//~ keys, err := b.List("")
			//~ if err != nil {
			//~ 	t.Fatalf("err: %v", err)
			//~ }
			//~ if len(keys) != 0 {
			//~ 	t.Fatalf("bad: %v", keys)
			//~ }
			var keys = b.List("");
			Assert.AreEqual(0, keys.Count());

			// Delete should work if it does not exist
			//~ err = b.Delete("foo")
			//~ if err != nil {
			//~ 	t.Fatalf("err: %v", err)
			//~ }
			b.Delete("foo");

			// Get should fail
			//~ out, err := b.Get("foo")
			//~ if err != nil {
			//~ 	t.Fatalf("err: %v", err)
			//~ }
			//~ if out != nil {
			//~ 	t.Fatalf("bad: %v", out)
			//~ }
			var @out = b.Get("foo");
			Assert.IsNull(@out);

			// Make an entry
			//~ e := &Entry{Key: "foo", Value: []byte("test")}
			//~ err = b.Put(e)
			//~ if err != nil {
			//~ 	t.Fatalf("err: %v", err)
			//~ }
			var e = new Entry { Key = "foo", Value = "test".ToUtf8Bytes() };
			b.Put(e);

			// Get should work
			//~ out, err = b.Get("foo")
			//~ if err != nil {
			//~ 	t.Fatalf("err: %v", err)
			//~ }
			//~ if !reflect.DeepEqual(out, e) {
			//~ 	t.Fatalf("bad: %v expected: %v", out, e)
			//~ }
			@out = b.Get("foo");
			Assert.IsNotNull(@out);

			// List should not be empty
			//~ keys, err = b.List("")
			//~ if err != nil {
			//~ 	t.Fatalf("err: %v", err)
			//~ }
			//~ if len(keys) != 1 {
			//~ 	t.Fatalf("bad: %v", keys)
			//~ }
			//~ if keys[0] != "foo" {
			//~ 	t.Fatalf("bad: %v", keys)
			//~ }
			keys = b.List("");
			Assert.AreEqual(1, keys.Count());
			Assert.AreEqual("foo", keys.ElementAt(0));

			// Delete should work
			//~ err = b.Delete("foo")
			//~ if err != nil {
			//~ 	t.Fatalf("err: %v", err)
			//~ }
			b.Delete("foo");

			// Should be empty
			//~ keys, err = b.List("")
			//~ if err != nil {
			//~ 	t.Fatalf("err: %v", err)
			//~ }
			//~ if len(keys) != 0 {
			//~ 	t.Fatalf("bad: %v", keys)
			//~ }
			keys = b.List("");
			Assert.AreEqual(0, keys.Count());

			// Get should fail
			//~ out, err = b.Get("foo")
			//~ if err != nil {
			//~ 	t.Fatalf("err: %v", err)
			//~ }
			//~ if out != nil {
			//~ 	t.Fatalf("bad: %v", out)
			//~ }
			@out = b.Get("foo");
			Assert.IsNull(@out);

			// Multiple Puts should work; GH-189
			//~ e = &Entry{Key: "foo", Value: []byte("test")}
			//~ err = b.Put(e)
			//~ if err != nil {
			//~ 	t.Fatalf("err: %v", err)
			//~ }
			//~ e = &Entry{Key: "foo", Value: []byte("test")}
			//~ err = b.Put(e)
			//~ if err != nil {
			//~ 	t.Fatalf("err: %v", err)
			//~ }
			e = new Entry { Key = "foo", Value = "test".ToUtf8Bytes() };
			b.Put(e);
			e = new Entry { Key = "foo", Value = "test".ToUtf8Bytes() };
			b.Put(e);

			// Make a nested entry
			//~ e = &Entry{Key: "foo/bar", Value: []byte("baz")}
			//~ err = b.Put(e)
			//~ if err != nil {
			//~ 	t.Fatalf("err: %v", err)
			//~ }
			e = new Entry { Key = "foo/bar", Value = "baz".ToUtf8Bytes() };
			b.Put(e);

			//~ keys, err = b.List("")
			//~ if err != nil {
			//~ 	t.Fatalf("err: %v", err)
			//~ }
			//~ if len(keys) != 2 {
			//~ 	t.Fatalf("bad: %v", keys)
			//~ }
			//~ sort.Strings(keys)
			//~ if keys[0] != "foo" || keys[1] != "foo/" {
			//~ 	t.Fatalf("bad: %v", keys)
			//~ }
			keys = b.List("");
			Assert.AreEqual(2, keys.Count());
			keys = keys.OrderBy(x => x);
			Assert.AreEqual("foo", keys.ElementAt(0));
			Assert.AreEqual("foo/", keys.ElementAt(1));

			// Delete with children should work
			//~ err = b.Delete("foo")
			//~ if err != nil {
			//~ 	t.Fatalf("err: %v", err)
			//~ }
			b.Delete("foo");

			// Get should return the child
			//~ out, err = b.Get("foo/bar")
			//~ if err != nil {
			//~ 	t.Fatalf("err: %v", err)
			//~ }
			//~ if out == nil {
			//~ 	t.Fatalf("missing child")
			//~ }
			@out = b.Get("foo/bar");
			Assert.IsNotNull(@out);

			// Removal of nested secret should not leave artifacts
			//~ e = &Entry{Key: "foo/nested1/nested2/nested3", Value: []byte("baz")}
			//~ err = b.Put(e)
			//~ if err != nil {
			//~ 	t.Fatalf("err: %v", err)
			//~ }
			e = new Entry { Key = "foo/nested1/nested2/nested3", Value = "baz".ToUtf8Bytes() };
			b.Put(e);

			//~ err = b.Delete("foo/nested1/nested2/nested3")
			//~ if err != nil {
			//~ 	t.Fatalf("failed to remove nested secret: %v", err)
			//~ }
			b.Delete("foo/nested1/nested2/nested3");

			//~ keys, err = b.List("foo/")
			//~ if err != nil {
			//~ 	t.Fatalf("err: %v", err)
			//~ }
			keys = b.List("foo/");

			//~ if len(keys) != 1 {
			//~ 	t.Fatalf("there should be only one key left after deleting nested "+
			//~ 		"secret: %v", keys)
			//~ }
			Assert.AreEqual(1, keys.Count());

			//~ if keys[0] != "bar" {
			//~ 	t.Fatalf("bad keys after deleting nested: %v", keys)
			//~ }
			Assert.AreEqual("bar", keys.ElementAt(0));

			// Make a second nested entry to test prefix removal
			//~ e = &Entry{Key: "foo/zip", Value: []byte("zap")}
			//~ err = b.Put(e)
			//~ if err != nil {
			//~ 	t.Fatalf("err: %v", err)
			//~ }
			e = new Entry { Key = "foo/zip", Value = "zap".ToUtf8Bytes() };
			b.Put(e);

			// Delete should not remove the prefix
			//~ err = b.Delete("foo/bar")
			//~ if err != nil {
			//~ 	t.Fatalf("err: %v", err)
			//~ }
			b.Delete("foo/bar");

			//~ keys, err = b.List("")
			//~ if err != nil {
			//~ 	t.Fatalf("err: %v", err)
			//~ }
			//~ if len(keys) != 1 {
			//~ 	t.Fatalf("bad: %v", keys)
			//~ }
			//~ if keys[0] != "foo/" {
			//~ 	t.Fatalf("bad: %v", keys)
			//~ }
			keys = b.List("");
			Assert.AreEqual(1, keys.Count());
			Assert.AreEqual("foo/", keys.ElementAt(0));

			// Delete should remove the prefix
			//~ err = b.Delete("foo/zip")
			//~ if err != nil {
			//~ 	t.Fatalf("err: %v", err)
			//~ }
			b.Delete("foo/zip");

			//~ keys, err = b.List("")
			//~ if err != nil {
			//~ 	t.Fatalf("err: %v", err)
			//~ }
			//~ if len(keys) != 0 {
			//~ 	t.Fatalf("bad: %v", keys)
			//~ }
			keys = b.List("");
			Assert.AreEqual(0, keys.Count());
		}

		public static void TestBackend_ListPrefix(IBackend b)
		{
			//~ e1 := &Entry{Key: "foo", Value: []byte("test")}
			//~ e2 := &Entry{Key: "foo/bar", Value: []byte("test")}
			//~ e3 := &Entry{Key: "foo/bar/baz", Value: []byte("test")}
			var e1 = new Entry { Key = "foo", Value = "test".ToUtf8Bytes() };
			var e2 = new Entry { Key = "foo/bar", Value = "test".ToUtf8Bytes() };
			var e3 = new Entry { Key = "foo/bar/baz", Value = "test".ToUtf8Bytes() };

			//~ defer func() {
			//~ 	b.Delete("foo")
			//~ 	b.Delete("foo/bar")
			//~ 	b.Delete("foo/bar/baz")
			//~ }()

			using (var defer = new Util.Defer())
			{
				defer.Add(() =>
				{
					b.Delete("foo");
					b.Delete("foo/bar");
					b.Delete("foo/bar/baz");
				});

				//~ err := b.Put(e1)
				//~ if err != nil {
				//~ 	t.Fatalf("err: %v", err)
				//~ }
				//~ err = b.Put(e2)
				//~ if err != nil {
				//~ 	t.Fatalf("err: %v", err)
				//~ }
				//~ err = b.Put(e3)
				//~ if err != nil {
				//~ 	t.Fatalf("err: %v", err)
				//~ }
				b.Put(e1);
				b.Put(e2);
				b.Put(e3);

				// Scan the root
				//~ keys, err := b.List("")
				//~ if err != nil {
				//~ 	t.Fatalf("err: %v", err)
				//~ }
				//~ if len(keys) != 2 {
				//~ 	t.Fatalf("bad: %v", keys)
				//~ }
				//~ sort.Strings(keys)
				//~ if keys[0] != "foo" {
				//~ 	t.Fatalf("bad: %v", keys)
				//~ }
				//~ if keys[1] != "foo/" {
				//~ 	t.Fatalf("bad: %v", keys)
				//~ }
				var keys = b.List("");
				Assert.AreEqual(2, keys.Count());
				keys = keys.OrderBy(x => x);
				Assert.AreEqual("foo", keys.ElementAt(0));
				Assert.AreEqual("foo/", keys.ElementAt(1));

				// Scan foo/
				//~ keys, err = b.List("foo/")
				//~ if err != nil {
				//~ 	t.Fatalf("err: %v", err)
				//~ }
				//~ if len(keys) != 2 {
				//~ 	t.Fatalf("bad: %v", keys)
				//~ }
				//~ sort.Strings(keys)
				//~ if keys[0] != "bar" {
				//~ 	t.Fatalf("bad: %v", keys)
				//~ }
				//~ if keys[1] != "bar/" {
				//~ 	t.Fatalf("bad: %v", keys)
				//~ }
				keys = b.List("foo/");
				Assert.AreEqual(2, keys.Count());
				keys = keys.OrderBy(x => x);
				Assert.AreEqual("bar", keys.ElementAt(0));
				Assert.AreEqual("bar/", keys.ElementAt(1));

				// Scan foo/bar/
				//~ keys, err = b.List("foo/bar/")
				//~ if err != nil {
				//~ 	t.Fatalf("err: %v", err)
				//~ }
				//~ sort.Strings(keys)
				//~ if len(keys) != 1 {
				//~ 	t.Fatalf("bad: %v", keys)
				//~ }
				//~ if keys[0] != "baz" {
				//~ 	t.Fatalf("bad: %v", keys)
				//~ }
				keys = b.List("foo/bar/");
				keys = keys.OrderBy(x => x);
				Assert.AreEqual(1, keys.Count());
				Assert.AreEqual("baz", keys.ElementAt(0));
			}
		}
	}
}
