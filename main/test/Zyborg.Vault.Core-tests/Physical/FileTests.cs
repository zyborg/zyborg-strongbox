using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Microsoft.Extensions.Logging;
using Zyborg.Vault.Physical;
using System.Collections.Generic;
using System.Diagnostics;
using Zyborg.Vault.Helper.Logging;
using Zyborg.Util;
using System.Linq;
using KellermanSoftware.CompareNetObjects;
using Newtonsoft.Json;

namespace Zyborg.Vault.Physical
{
    [TestClass]
    public class FileTests
    {
		[TestMethod]
		//~ func TestFileBackend_Base64URLEncoding(t *testing.T) {
		public void TestFileBackend_Base64UrlEncoding()
		{
			using (var defer = new Util.Defer())
			using (var lf = VaultLogger.CreateLoggerFactory(LogLevel.Trace))
			{
				//backendPath, err:= ioutil.TempDir("", "vault")
				//if err != nil {
				//	t.Fatalf("err: %s", err)
				//}
				//defer os.RemoveAll(backendPath)
				var backendPath = Util.IO.Directory.CreateTempDir(prefix: "vault");
				defer.Add(() =>
				{
					Debug.WriteLine($"WOULD DELETE ALL AT:  [{backendPath}]");
					//Directory.Delete(backendPath, true);
				});

				//~ logger:= logformat.NewVaultLogger(log.LevelTrace)
				var logger = lf.CreateLogger<FileBackend>();

				//~ b, err:= NewBackend("file", logger, map[string]string{
				//~ 	"path": backendPath,
				//~ })
				//~ if err != nil {
				//~ 	t.Fatalf("err: %s", err)
				//~ }
				var b = Global.NewBackend("file", logger, new ConfigMap<string>
				{
					["path"] = backendPath,
				});

				// List the entries. Length should be zero.
				//~ keys, err:= b.List("")
				//~ if err != nil {
				//~ 	t.Fatalf("err: %v", err)
				//~ }
				//~ if len(keys) != 0 {
				//~ 	t.Fatalf("bad: len(keys): expected: 0, actual: %d", len(keys))
				//~ }
				var keys = b.List("");
				Assert.AreEqual(0, keys.Count());

				// Create a storage entry without base64 encoding the file name
				//~ rawFullPath:= filepath.Join(backendPath, "_foo")
				//~ e:= &Entry{ Key: "foo", Value: []byte("test")}
				//~ f, err:= os.OpenFile(
				//~    rawFullPath,
				//~    os.O_CREATE | os.O_TRUNC | os.O_WRONLY,
				//~    0600)
				//~ if err != nil {
				//~ 	t.Fatal(err)
				//~ }
				//~ json.NewEncoder(f).Encode(e)
				//~ f.Close()
				var rawFullPath = Path.Combine(backendPath, "_foo");
				var e = new Entry { Key = "foo", Value = "test".ToUtf8Bytes() };
				using (var fs = new FileStream(rawFullPath, FileMode.Create, FileAccess.Write))
				{
					var bytes = JsonConvert.SerializeObject(e).ToUtf8Bytes();
					fs.Write(bytes, 0, bytes.Length);
				}

				// Get should work
				//~ out, err:= b.Get("foo")
				//~ if err != nil {
				//~ 	t.Fatalf("err: %v", err)
				//~ }
				//~ if !reflect.DeepEqual(out, e) {
				//~ 	t.Fatalf("bad: %v expected: %v", out, e)
				//~ }
				var @out = b.Get("foo");
				var compare = new CompareLogic();
				Assert.IsTrue(compare.Compare(e, @out).AreEqual);

				// List the entries. There should be one entry.
				//~ keys, err = b.List("")
				//~ if err != nil {
				//~ 	t.Fatalf("err: %v", err)
				//~ }
				//~ if len(keys) != 1 {
				//~ 	t.Fatalf("bad: len(keys): expected: 1, actual: %d", len(keys))
				//~ }
				keys = b.List("");
				Assert.AreEqual(1, keys.Count());

				//~ err = b.Put(e)
				//~ if err != nil {
				//~ 	t.Fatalf("err: %v", err)
				//~ }
				b.Put(e);

				// List the entries again. There should still be one entry.
				//~ keys, err = b.List("")
				//~ if err != nil {
				//~ 	t.Fatalf("err: %v", err)
				//~ }
				//~ if len(keys) != 1 {
				//~ 	t.Fatalf("bad: len(keys): expected: 1, actual: %d", len(keys))
				//~ }
				keys = b.List("");
				Assert.AreEqual(1, keys.Count());

				// Get should work
				//~ out, err = b.Get("foo")
				//~ if err != nil {
				//~ 	t.Fatalf("err: %v", err)
				//~ }
				//~ if !reflect.DeepEqual(out, e) {
				//~ 	t.Fatalf("bad: %v expected: %v", out, e)
				//~ }
				@out = b.Get("foo");
				Assert.IsTrue(compare.Compare(e, @out).AreEqual);

				//~ err = b.Delete("foo")
				//~ if err != nil {
				//~ 	t.Fatalf("err: %v", err)
				//~ }
				b.Delete("foo");

				//~ out, err = b.Get("foo")
				//~ if err != nil {
				//~ 	t.Fatalf("err: %v", err)
				//~ }
				//~ if out != nil {
				//~ 	t.Fatalf("bad: entry: expected: nil, actual: %#v", e)
				//~ }
				@out = b.Get("foo");
				Assert.IsNull(@out);

				//~ keys, err = b.List("")
				//~ if err != nil {
				//~ 	t.Fatalf("err: %v", err)
				//~ }
				//~ if len(keys) != 0 {
				//~ 	t.Fatalf("bad: len(keys): expected: 0, actual: %d", len(keys))
				//~ }
				keys = b.List("");
				Assert.AreEqual(0, keys.Count());

				//~ f, err = os.OpenFile(
				//~ 	rawFullPath,
				//~ 	os.O_CREATE | os.O_TRUNC | os.O_WRONLY,
				//~ 	0600)
				//~ if err != nil {
				//~ 	t.Fatal(err)
				//~ }
				//~ json.NewEncoder(f).Encode(e)
				//~ f.Close()
				using (var fs = new FileStream(rawFullPath, FileMode.Create, FileAccess.Write))
				{
					var bytes = JsonConvert.SerializeObject(e).ToUtf8Bytes();
					fs.Write(bytes, 0, bytes.Length);
				}

				//~ keys, err = b.List("")
				//~ if err != nil {
				//~ 	t.Fatalf("err: %v", err)
				//~ }
				//~ if len(keys) != 1 {
				//~ 	t.Fatalf("bad: len(keys): expected: 1, actual: %d", len(keys))
				//~ }
				keys = b.List("");
				Assert.AreEqual(1, keys.Count());
			}
		}

        [TestMethod]
		//~ func TestFileBackend(t *testing.T) {
		public void TestFileBackend()
        {
			//~ dir, err := ioutil.TempDir("", "vault")
			//~ if err != nil {
			//~ 	t.Fatalf("err: %s", err)
			//~ }
			//~ defer os.RemoveAll(dir)
			//~ 
			//~ logger := logformat.NewVaultLogger(log.LevelTrace)
			//~ 
			//~ b, err := NewBackend("file", logger, map[string]string{
			//~ 	"path": dir,
			//~ })
			//~ if err != nil {
			//~ 	t.Fatalf("err: %s", err)
			//~ }
			//~ 
			//~ testBackend(t, b)
			//~ testBackend_ListPrefix(t, b)

			using (var defer = new Util.Defer())
			using (var lf = VaultLogger.CreateLoggerFactory(LogLevel.Trace))
			{
				var dir = Util.IO.Directory.CreateTempDir(prefix: "vault");
				defer.Add(() =>
				{
					Debug.WriteLine($"WOULD DELETE ALL AT:  [{dir}]");
					//Directory.Delete(dir, true);
				});

				lf.AddConsole(LogLevel.Trace);
				var logger = lf.CreateLogger<FileBackend>();
				var b = Physical.Global.NewBackend("file", logger,
						new Util.ConfigMap<string>
						{
							["path"] = dir,
						});

				PhysicalTests.TestBackend(b);
				PhysicalTests.TestBackend_ListPrefix(b);

			}
		}
    }
}
