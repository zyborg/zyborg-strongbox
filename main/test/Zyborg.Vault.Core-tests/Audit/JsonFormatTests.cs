using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Zyborg.Util;
using Zyborg.Util.Collections;
using Zyborg.Vault.Helper.Salt;

namespace Zyborg.Vault.Audit
{
	[TestClass]
    public class JsonFormatTests
    {
		public const string testFormatJSONReqBasicStr = @"{""time"":""2015-08-05T13:45:46Z"",""type"":""request"",""auth"":{""display_name"":"""",""policies"":[""root""],""metadata"":null},""request"":{""operation"":""update"",""path"":""/foo"",""data"":null,""wrap_ttl"":60,""remote_address"":""127.0.0.1"",""headers"":{""foo"":[""bar""]}},""error"":""this is an error""}
";

		[TestMethod]
		//~ func TestFormatJSON_formatRequest(t *testing.T) {
		public void TestFormatJSON_formatRequest()
		{
			//~ cases := map[string]struct {
			//~ 	Auth   *logical.Auth
			//~ 	Req    *logical.Request
			//~ 	Err    error
			//~ 	Prefix string
			//~ 	Result string
			//~ }{
			var cases = new OrderedDictionary<string, (
					Logical.Auth auth,
					Logical.Request req,
					Exception err,
					string prefix,
					string result
				)>
			{
				//~ "auth, request": {
				//~ 	&logical.Auth{ClientToken: "foo", Policies: []string{"root"}},
				//~ 	&logical.Request{
				//~ 		Operation: logical.UpdateOperation,
				//~ 		Path:      "/foo",
				//~ 		Connection: &logical.Connection{
				//~ 			RemoteAddr: "127.0.0.1",
				//~ 		},
				//~ 		WrapInfo: &logical.RequestWrapInfo{
				//~ 			TTL: 60 * time.Second,
				//~ 		},
				//~ 		Headers: map[string][]string{
				//~ 			"foo": []string{"bar"},
				//~ 		},
				//~ 	},
				//~ 	errors.New("this is an error"),
				//~ 	"",
				//~ 	testFormatJSONReqBasicStr,
				//~ },
				["auth, request"] = (
					new Logical.Auth { ClientToken = "foo", Policies = new string[] { "root" } },
					new Logical.Request
					{
						Operation = Logical.Operation.UpdateOperation,
						Path = "/foo",
						Connection = new Logical.Connection
						{
							RemoteAddr = "127.0.0.1",
						},
						WrapInfo = new Logical.RequestWrapInfo
						{
							TTL = TimeSpan.FromSeconds(60),
						},
						Headers = new Dictionary<string, string[]>
						{
							["foo"] = new string[] { "bar" },
						},
					},
					new Exception("this is an error"),
					"",
					testFormatJSONReqBasicStr
				),
				//~ "auth, request with prefix": {
				//~ 	&logical.Auth{ClientToken: "foo", Policies: []string{"root"}},
				//~ 	&logical.Request{
				//~ 		Operation: logical.UpdateOperation,
				//~ 		Path:      "/foo",
				//~ 		Connection: &logical.Connection{
				//~ 			RemoteAddr: "127.0.0.1",
				//~ 		},
				//~ 		WrapInfo: &logical.RequestWrapInfo{
				//~ 			TTL: 60 * time.Second,
				//~ 		},
				//~ 		Headers: map[string][]string{
				//~ 			"foo": []string{"bar"},
				//~ 		},
				//~ 	},
				//~ 	errors.New("this is an error"),
				//~ 	"@cee: ",
				//~ 	testFormatJSONReqBasicStr,
				//~ },
				["auth, request with prefix"] = (
					new Logical.Auth { ClientToken = "foo", Policies = new string[] { "root" } },
					new Logical.Request
					{
						Operation = Logical.Operation.UpdateOperation,
						Path = "/foo",
						Connection = new Logical.Connection
						{
							RemoteAddr = "127.0.0.1",
						},
						WrapInfo = new Logical.RequestWrapInfo
						{
							TTL = TimeSpan.FromSeconds(60),
						},
						Headers = new Dictionary<string, string[]>
						{
							["foo"] = new string[] { "bar" },
						},
					},
					new Exception("this is an error"),
					"@cee: ",
					testFormatJSONReqBasicStr
				),
			};

			//~ for name, tc := range cases {
			foreach (var kv in cases)
			{
				var name = kv.Key;
				var tc = kv.Value;

				//~ var buf bytes.Buffer
				//~ formatter := AuditFormatter{
				//~ 	AuditFormatWriter: &JSONFormatWriter{
				//~ 		Prefix: tc.Prefix,
				//~ 	},
				//~ }
				//~ salter, _ := salt.NewSalt(nil, nil)
				//~ config := FormatterConfig{
				//~ 	Salt: salter,
				//~ }
				//~ if err := formatter.FormatRequest(&buf, config, tc.Auth, tc.Req, tc.Err); err != nil {
				//~ 	t.Fatalf("bad: %s\nerr: %s", name, err)
				//~ }
				byte[] buf;
				var formatter = new AuditFormatter(new JsonFormatWriter
				{
					Prefix = tc.prefix,
				});
				var salter = Salt.NewSalt(null, null);
				var config = new FormatterConfig
				{
					Salt = salter,
				};
				using (var ms = new MemoryStream())
				{
					formatter.FormatRequest(ms, config, tc.auth, tc.req, tc.err);
					buf = ms.ToArray();
				}

				//~ if !strings.HasPrefix(buf.String(), tc.Prefix) {
				//~ 	t.Fatalf("no prefix: %s \n log: %s\nprefix: %s", name, tc.Result, tc.Prefix)
				//~ }
				Assert.IsTrue(buf.ToUtf8String().StartsWith(tc.prefix));

				//~ var expectedjson = new(AuditRequestEntry)
				//~ if err := jsonutil.DecodeJSON([]byte(tc.Result), &expectedjson); err != nil {
				//~ 	t.Fatalf("bad json: %s", err)
				//~ }
				var expectedjson = JsonConvert.DeserializeObject<AuditRequestEntry>(tc.result);

				//~ var actualjson = new(AuditRequestEntry)
				//~ if err := jsonutil.DecodeJSON([]byte(buf.String())[len(tc.Prefix):], &actualjson); err != nil {
				//~ 	t.Fatalf("bad json: %s", err)
				//~ }
				var actualjson = JsonConvert.DeserializeObject<AuditRequestEntry>(buf.ToUtf8String().Substring(tc.prefix.Length));

				//~ expectedjson.Time = actualjson.Time
				expectedjson.Time = actualjson.Time;

				//~ expectedBytes, err := json.Marshal(expectedjson)
				//~ if err != nil {
				//~ 	t.Fatalf("unable to marshal json: %s", err)
				//~ }
				var expectedBytes = JsonConvert.SerializeObject(expectedjson);

				//~ if !strings.HasSuffix(strings.TrimSpace(buf.String()), string(expectedBytes)) {
				//~ 	t.Fatalf(
				//~ 		"bad: %s\nResult:\n\n'%s'\n\nExpected:\n\n'%s'",
				//~ 		name, buf.String(), string(expectedBytes))
				//~ }
				Assert.IsTrue(buf.ToUtf8String().Trim().EndsWith(expectedBytes));
			}
		}

    }
}
