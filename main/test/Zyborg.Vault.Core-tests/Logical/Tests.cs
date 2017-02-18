using KellermanSoftware.CompareNetObjects;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zyborg.Util;
using Zyborg.Vault.Helper.Logging;

namespace Zyborg.Vault.Logical
{
	[TestClass]
    public class Tests
    {
		private static readonly ILoggerFactory LF = VaultLogger.CreateLoggerFactory(LogLevel.Trace);

		// TestRequest is a helper to create a purely in-memory Request struct.
		//~ func TestRequest(t *testing.T, op Operation, path string) *Request {
		public static Request TestRequet(Operation op, string path)
		{
			//~ return &Request{
			//~ 	Operation: op,
			//~ 	Path:      path,
			//~ 	Data:      make(map[string]interface{}),
			//~ 	Storage:   new(InmemStorage),
			//~ }
			return new Request
			{
				Operation = op,
				Path = path,
				Data = new Dictionary<string, object>(),
				Storage = new InmemStorage(),
			};
		}

		// TestStorage is a helper that can be used from unit tests to verify
		// the behavior of a Storage impl.
		//~ func TestStorage(t *testing.T, s Storage) {
		public static void TestStorage(IStorage s)
		{
			//~ keys, err := s.List("")
			//~ if err != nil {
			//~ 	t.Fatalf("list error: %s", err)
			//~ }
			//~ if len(keys) > 0 {
			//~ 	t.Fatalf("should have no keys to start: %#v", keys)
			//~ }
			var keys = s.List("");
			Assert.AreEqual(0, keys.Count());

			//~ entry := &StorageEntry{Key: "foo", Value: []byte("bar")}
			//~ if err := s.Put(entry); err != nil {
			//~ 	t.Fatalf("put error: %s", err)
			//~ }
			var entry = new StorageEntry { Key = "foo", Value = "bar".ToUtf8Bytes() };
			s.Put(entry);

			//~ actual, err := s.Get("foo")
			//~ if err != nil {
			//~ 	t.Fatalf("get error: %s", err)
			//~ }
			//~ if !reflect.DeepEqual(actual, entry) {
			//~ 	t.Fatalf("wrong value. Expected: %#v\nGot: %#v", entry, actual)
			//~ }
			var actual = s.Get("foo");
			var compare = new CompareLogic();
			Assert.IsTrue(compare.Compare(entry, actual).AreEqual);

			//~ keys, err = s.List("")
			//~ if err != nil {
			//~ 	t.Fatalf("list error: %s", err)
			//~ }
			//~ if !reflect.DeepEqual(keys, []string{"foo"}) {
			//~ 	t.Fatalf("bad keys: %#v", keys)
			//~ }
			keys = s.List("");
			CollectionAssert.AreEqual(keys.ToArray(), new string[] { "foo" });

			//~ if err := s.Delete("foo"); err != nil {
			//~ 	t.Fatalf("put error: %s", err)
			//~ }
			s.Delete("foo");

			//~ keys, err = s.List("")
			//~ if err != nil {
			//~ 	t.Fatalf("list error: %s", err)
			//~ }
			//~ if len(keys) > 0 {
			//~ 	t.Fatalf("should have no keys to start: %#v", keys)
			//~ }
			keys = s.List("");
			Assert.AreEqual(0, keys.Count());
		}

		//~ func TestSystemView() *StaticSystemView {
		public static StaticSystemView TestSystemView()
		{
			//~ defaultLeaseTTLVal := time.Hour * 24
			//~ maxLeaseTTLVal := time.Hour * 24 * 2
			var defaultLeastTTLVal = TimeSpan.FromHours(24);
			var maxLeaseTTLVal = TimeSpan.FromHours(24 * 2);

			//~ return &StaticSystemView{
			//~ 	DefaultLeaseTTLVal: defaultLeaseTTLVal,
			//~ 	MaxLeaseTTLVal:     maxLeaseTTLVal,
			//~ }
			return new StaticSystemView
			{
				DefaultLeaseTTL = defaultLeastTTLVal,
				MaxLeaseTTL = maxLeaseTTLVal,
			};
		}

		//~ func TestBackendConfig() *BackendConfig {
		public static BackendConfig TestBackendConfig()
		{
			//~ bc := &BackendConfig{
			//~ 	Logger: logformat.NewVaultLogger(log.LevelTrace),
			//~ 	System: TestSystemView(),
			//~ }
			//~ bc.Logger.SetLevel(log.LevelTrace)
			return new BackendConfig
			{
				Logger = LF.CreateLogger<BackendConfig>(),
				System = TestSystemView(),
			};
		}		
    }
}
