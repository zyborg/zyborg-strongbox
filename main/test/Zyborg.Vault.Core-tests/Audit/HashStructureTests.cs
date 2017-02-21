using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Zyborg.Util;
using Zyborg.Vault.Helper.Salt;

namespace Zyborg.Vault.Audit
{
	[TestClass]
    public class HashStructureTests
    {
		[TestMethod]
		//~ func TestCopy_auth(t *testing.T) {
		public void TestCopy_auth()
		{
			// Make a non-pointer one so that it can't be modified directly
			//~ expected := logical.Auth{
			//~ 	LeaseOptions: logical.LeaseOptions{
			//~ 		TTL:       1 * time.Hour,
			//~ 		IssueTime: time.Now(),
			//~ 	},
			//~ 
			//~ 	ClientToken: "foo",
			//~ }
			//~ auth := expected
			var expected = new Logical.Auth
			{
				TTL = TimeSpan.FromHours(1),
				IssueTime = DateTime.Now,
				ClientToken = "foo",
			};
			var auth = expected;

			// Copy it
			//~ dup, err := copystructure.Copy(&auth)
			//~ if err != nil {
			//~ 	t.Fatalf("err: %s", err)
			//~ }
			var dup = auth.DeepCopy();

			// Check equality
			//~ auth2 := dup.(*logical.Auth)
			//~ if !reflect.DeepEqual(*auth2, expected) {
			//~ 	t.Fatalf("bad:\n\n%#v\n\n%#v", *auth2, expected)
			//~ }
			var auth2 = dup as Logical.Auth;
			var compare = new KellermanSoftware.CompareNetObjects.CompareLogic();
			Assert.IsTrue(compare.Compare(auth2, expected).AreEqual);
		}

		[TestMethod]
		//~ func TestCopy_request(t *testing.T) {
		public void TestCopy_request()
		{
			// Make a non-pointer one so that it can't be modified directly
			//~ expected := logical.Request{
			//~ 	Data: map[string]interface{}{
			//~ 		"foo": "bar",
			//~ 	},
			//~ 	WrapInfo: &logical.RequestWrapInfo{
			//~ 		TTL: 60 * time.Second,
			//~ 	},
			//~ }
			//~ arg := expected
			var expected = new Logical.Request
			{
				Data = new Dictionary<string, object>
				{
					["foo"] = "bar",
				},
				WrapInfo = new Logical.RequestWrapInfo
				{
					TTL = TimeSpan.FromSeconds(60),
				},
			};
			var arg = expected;

			// Copy it
			//~ dup, err := copystructure.Copy(&arg)
			//~ if err != nil {
			//~ 	t.Fatalf("err: %s", err)
			//~ }
			var dup = arg.DeepCopy();

			// Check equality
			//~ arg2 := dup.(*logical.Request)
			var arg2 = dup as Logical.Request;
			//~ if !reflect.DeepEqual(*arg2, expected) {
			//~ 	t.Fatalf("bad:\n\n%#v\n\n%#v", *arg2, expected)
			//~ }
			var compare = new KellermanSoftware.CompareNetObjects.CompareLogic();
			Assert.IsTrue(compare.Compare(expected, arg2).AreEqual);
		}

		[TestMethod]
		//~ func TestCopy_response(t *testing.T) {
		public void TestCopy_response()
		{
			// Make a non-pointer one so that it can't be modified directly
			//~ expected := logical.Response{
			//~ 	Data: map[string]interface{}{
			//~ 		"foo": "bar",
			//~ 	},
			//~ 	WrapInfo: &logical.ResponseWrapInfo{
			//~ 		TTL:             60,
			//~ 		Token:           "foo",
			//~ 		CreationTime:    time.Now(),
			//~ 		WrappedAccessor: "abcd1234",
			//~ 	},
			//~ }
			//~ arg := expected
			var expected = new Logical.Response
			{
				Data = new Dictionary<string, object>
				{
					["foo"] = "bar"
				},
				WrapInfo = new Logical.ResponseWrapInfo
				{
					TTL = TimeSpan.FromSeconds(60),
					Token = "foo",
					CreationTime = DateTime.Now,
					WrappedAccessor = "abcd1234",
				},
			};
			var arg = expected;

			// Copy it
			//~ dup, err := copystructure.Copy(&arg)
			//~ if err != nil {
			//~ 	t.Fatalf("err: %s", err)
			//~ }
			var dup = arg.DeepCopy();

			// Check equality
			//~ arg2 := dup.(*logical.Response)
			//~ if !reflect.DeepEqual(*arg2, expected) {
			//~ 	t.Fatalf("bad:\n\n%#v\n\n%#v", *arg2, expected)
			//~ }
			var arg2 = dup as Logical.Response;
			var compare = new KellermanSoftware.CompareNetObjects.CompareLogic();
			Assert.IsTrue(compare.Compare(expected, arg2).AreEqual);
		}

		[TestMethod]
		//~ func TestHashString(t *testing.T) {
		public void TestHashString()
		{
			//~ inmemStorage := &logical.InmemStorage{}
			//~ inmemStorage.Put(&logical.StorageEntry{
			//~ 	Key:   "salt",
			//~ 	Value: []byte("foo"),
			//~ })
			var inmemStorage = new Logical.InmemStorage();
			inmemStorage.Put(new Logical.StorageEntry
			{
				Key = "salt",
				Value = "foo".ToUtf8Bytes(),
			});
			//~ localSalt, err := salt.NewSalt(inmemStorage, &salt.Config{
			//~ 	HMAC:     sha256.New,
			//~ 	HMACType: "hmac-sha256",
			//~ })
			var localSalt = Salt.NewSalt(inmemStorage, new Config
			{
				HMAC = () => new HMACSHA256(),
				HMACType = "hmac-sha256",

			});
			//~ if err != nil {
			//~ 	t.Fatalf("Error instantiating salt: %s", err)
			//~ }
			//~ out := HashString(localSalt, "foo")
			//~ if out != "hmac-sha256:08ba357e274f528065766c770a639abf6809b39ccfd37c2a3157c7f51954da0a" {
			//~ 	t.Fatalf("err: HashString output did not match expected")
			//~ }
			var @out = HashStructure.HashString(localSalt, "foo");
			Assert.AreEqual("hmac-sha256:08ba357e274f528065766c770a639abf6809b39ccfd37c2a3157c7f51954da0a", @out.ToLower());
		}

		[TestMethod]
		//~ func TestHash(t *testing.T) {
		public void TestHash()
		{
			//~ now := time.Now()
			var now = DateTime.Now;

			//~ cases := []struct {
			//~ 	Input  interface{}
			//~ 	Output interface{}
			//~ }{
			var cases = new(object input, object output)[]
			{
				(
					//~ &logical.Auth{ClientToken: "foo"},
					//~ &logical.Auth{ClientToken: "hmac-sha256:08ba357e274f528065766c770a639abf6809b39ccfd37c2a3157c7f51954da0a"},
					new Logical.Auth { ClientToken = "foo" },
					new Logical.Auth { ClientToken = "hmac-sha256:08ba357e274f528065766c770a639abf6809b39ccfd37c2a3157c7f51954da0a" }
				),(
					//~ &logical.Request{
					//~ 	Data: map[string]interface{}{
					//~ 		"foo":              "bar",
					//~ 		"private_key_type": certutil.PrivateKeyType("rsa"),
					//~ 	},
					//~ },
					//~ &logical.Request{
					//~ 	Data: map[string]interface{}{
					//~ 		"foo":              "hmac-sha256:f9320baf0249169e73850cd6156ded0106e2bb6ad8cab01b7bbbebe6d1065317",
					//~ 		"private_key_type": "hmac-sha256:995230dca56fffd310ff591aa404aab52b2abb41703c787cfa829eceb4595bf1",
					//~ 	},
					//~ },
					new Logical.Request
					{
						Data = new Dictionary<string, object>
						{
							["foo"] =              "bar",
						// IMPL:  since Go's version of PrivateKeyType is essentially an alias for
						// a string and our version is a class following the Enum pattern, the only
						// way we can match the test results is to fake it out as a string
						//	["private_key_type"] = Helper.CertUtil.PrivateKeyType.From("rsa"),
							["private_key_type"] = "rsa",
						},
					},
					new Logical.Request
					{
						Data = new Dictionary<string, object>
						{
							["foo"] = "hmac-sha256:f9320baf0249169e73850cd6156ded0106e2bb6ad8cab01b7bbbebe6d1065317",
							["private_key_type"] = "hmac-sha256:995230dca56fffd310ff591aa404aab52b2abb41703c787cfa829eceb4595bf1",
						},
					}
				),(
					//~ &logical.Response{
					//~ 	Data: map[string]interface{}{
					//~ 		"foo": "bar",
					//~ 	},
					//~ 	WrapInfo: &logical.ResponseWrapInfo{
					//~ 		TTL:             60,
					//~ 		Token:           "bar",
					//~ 		CreationTime:    now,
					//~ 		WrappedAccessor: "bar",
					//~ 	},
					//~ },
					//~ &logical.Response{
					//~ 	Data: map[string]interface{}{
					//~ 		"foo": "hmac-sha256:f9320baf0249169e73850cd6156ded0106e2bb6ad8cab01b7bbbebe6d1065317",
					//~ 	},
					//~ 	WrapInfo: &logical.ResponseWrapInfo{
					//~ 		TTL:             60,
					//~ 		Token:           "hmac-sha256:f9320baf0249169e73850cd6156ded0106e2bb6ad8cab01b7bbbebe6d1065317",
					//~ 		CreationTime:    now,
					//~ 		WrappedAccessor: "hmac-sha256:f9320baf0249169e73850cd6156ded0106e2bb6ad8cab01b7bbbebe6d1065317",
					//~ 	},
					//~ },
					new Logical.Response
					{
						Data = new Dictionary<string, object>
						{
							["foo"] = "bar",
						},
						WrapInfo = new Logical.ResponseWrapInfo
						{
							TTL =             TimeSpan.FromSeconds(60),
							Token =           "bar",
							CreationTime =    now,
							WrappedAccessor = "bar",
						},
					},
					new Logical.Response{
						Data = new Dictionary<string, object>
						{
							["foo"] = "hmac-sha256:f9320baf0249169e73850cd6156ded0106e2bb6ad8cab01b7bbbebe6d1065317",
						},
						WrapInfo = new Logical.ResponseWrapInfo
						{
							TTL =             TimeSpan.FromSeconds(60),
							Token =           "hmac-sha256:f9320baf0249169e73850cd6156ded0106e2bb6ad8cab01b7bbbebe6d1065317",
							CreationTime =    now,
							WrappedAccessor = "hmac-sha256:f9320baf0249169e73850cd6156ded0106e2bb6ad8cab01b7bbbebe6d1065317",
						},
					}
				),(
					//~ "foo",
					//~ "foo",
					"foo",
					"foo"
				),(
					//~ &logical.Auth{
					//~ 	LeaseOptions: logical.LeaseOptions{
					//~ 		TTL:       1 * time.Hour,
					//~ 		IssueTime: now,
					//~ 	},
					//~ 
					//~ 	ClientToken: "foo",
					//~ },
					//~ &logical.Auth{
					//~ 	LeaseOptions: logical.LeaseOptions{
					//~ 		TTL:       1 * time.Hour,
					//~ 		IssueTime: now,
					//~ 	},
					//~ 
					//~ 	ClientToken: "hmac-sha256:08ba357e274f528065766c770a639abf6809b39ccfd37c2a3157c7f51954da0a",
					//~ },
					new Logical.Auth
					{
						// LeaseOptions
						TTL =        TimeSpan.FromHours(1),
						IssueTime = now,

						ClientToken = "foo",
					},
					new Logical.Auth
					{
						// LeaseOptions
						TTL =       TimeSpan.FromHours(1),
						IssueTime = now,
						
						ClientToken = "hmac-sha256:08ba357e274f528065766c770a639abf6809b39ccfd37c2a3157c7f51954da0a",
					}
				),
			};

			//~ inmemStorage := &logical.InmemStorage{}
			//~ inmemStorage.Put(&logical.StorageEntry{
			//~ 	Key:   "salt",
			//~ 	Value: []byte("foo"),
			//~ })
			//~ localSalt, err := salt.NewSalt(inmemStorage, &salt.Config{
			//~ 	HMAC:     sha256.New,
			//~ 	HMACType: "hmac-sha256",
			//~ })
			//~ if err != nil {
			//~ 	t.Fatalf("Error instantiating salt: %s", err)
			//~ }
			var inmemStorage = new Logical.InmemStorage();
			inmemStorage.Put(new Logical.StorageEntry
			{
				Key = "salt",
				Value = "foo".ToUtf8Bytes(),
			});
			var localSalt = Salt.NewSalt(inmemStorage, new Config
			{
				HMAC = () => new HMACSHA256(),
				HMACType = "hmac-sha256",
			});
			//~ for _, tc := range cases {
			foreach (var tc in cases)
			{
				//~ input := fmt.Sprintf("%#v", tc.Input)
				//~ if err := Hash(localSalt, tc.Input); err != nil {
				//~ 	t.Fatalf("err: %s\n\n%s", err, input)
				//~ }
				//~ if !reflect.DeepEqual(tc.Input, tc.Output) {
				//~ 	t.Fatalf("bad:\nInput:\n%s\nTest case input:\n%#v\nTest case output\n%#v", input, tc.Input, tc.Output)
				//~ }
				var input = $"{tc.input}";
				HashStructure.Hash(localSalt, tc.input);
				var compare = new KellermanSoftware.CompareNetObjects.CompareLogic();
				Assert.IsTrue(compare.Compare(tc.input, tc.output).AreEqual);
			}
		}
	}
}
