using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Zyborg.Util.Encoding
{
	[TestClass]
    public class JsonXTests
    {
		public const string testJSONOfficial = @"{
		  ""name"":""John Smith"",
		  ""address"": {
			""streetAddress"": ""21 2nd Street"",
			""city"": ""New York"",
			""state"": ""NY"",
			""postalCode"": 10021
		  },
		  ""phoneNumbers"": [
			""212 555-1111"",
			""212 555-2222""
		  ],
		  ""additionalInfo"": null,
		  ""remote"": false,
		  ""height"": 62.4,
		  ""ficoScore"": "" > 640""
		}";

		public const string resultJSONOfficial = @"<json:null name=""additionalInfo"" /><json:object name=""address""><json:string name=""city"">New York</json:string><json:number name=""postalCode"">10021</json:number><json:string name=""state"">NY</json:string><json:string name=""streetAddress"">21 2nd Street</json:string></json:object><json:string name=""ficoScore""> > 640</json:string><json:number name=""height"">62.4</json:number><json:string name=""name"">John Smith</json:string><json:array name=""phoneNumbers""><json:string>212 555-1111</json:string><json:string>212 555-2222</json:string></json:array><json:boolean name=""remote"">False</json:boolean>";

		public const string testJSONHard = @"{
			""array"": [""true"", ""testing""],
			""arrayofobjects"": [{
				""zip"": ""zap"",
				""foo"": ""bar""
			}, {
				""boop"": ""bop""
			}],
			""bool"": false,
			""float"": 2934.24,
			""int"": 2342,
			""null"": null,
			""quote: \"""": ""\"""",
			""string"": ""234,24""
		}";

		public const string resultJSONHard = @"<json:array name=""array""><json:string>true</json:string><json:string>testing</json:string></json:array><json:array name=""arrayofobjects""><json:object><json:string name=""foo"">bar</json:string><json:string name=""zip"">zap</json:string></json:object><json:object><json:string name=""boop"">bop</json:string></json:object></json:array><json:boolean name=""bool"">False</json:boolean><json:number name=""float"">2934.24</json:number><json:number name=""int"">2342</json:number><json:null name=""null"" /><json:string name=""quote: &#34;"">""</json:string><json:string name=""string"">234,24</json:string>";

		[TestMethod]
		//~ func TestEncodeJSONBytes(t *testing.T) {
		public void TestEncodeJSONBytes()
		{
			//~ jsonxBytes, err := EncodeJSONBytes([]byte(testJSONOfficial))
			//~ if err != nil {
			//~ 	t.Fatal(err)
			//~ }
			//~ if string(jsonxBytes) != resultJSONOfficial {
			//~ 	t.Fatalf("official result mismatch:\ngot:\n%s\nexpected:\n%s\n", string(jsonxBytes), resultJSONOfficial)
			//~ }
			var jsonxBytes = NamedContainer.EncodeJSONBytes(testJSONOfficial.ToUtf8Bytes());
			Assert.AreEqual(resultJSONOfficial, jsonxBytes.ToUtf8String());

			//~ jsonxBytes, err = EncodeJSONBytes([]byte(testJSONHard))
			//~ if err != nil {
			//~ 	t.Fatal(err)
			//~ }
			//~ if string(jsonxBytes) != resultJSONHard {
			//~ 	t.Fatalf("hard result mismatch:\ngot:\n%s\nexpected:\n%s\n", string(jsonxBytes), resultJSONHard)
			//~ }
			jsonxBytes = NamedContainer.EncodeJSONBytes(testJSONHard.ToUtf8Bytes());
			Assert.AreEqual(resultJSONHard, jsonxBytes.ToUtf8String());
		}
	}
}
