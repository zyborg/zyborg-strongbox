using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Zyborg.Util.Encoding
{
	[TestClass]
	public class GabsTests
	{
		[TestMethod]
		//~ func TestBasic(t *testing.T) {
		public void TestBasic()
		{
			//~ sample := []byte(`{"test":{"value":10},"test2":20}`)
			var sample = @"{""test"":{""value"":10},""test2"":20}".ToUtf8Bytes();

			//~ val, err := ParseJSON(sample)
			//~ if err != nil {
			//~ 	t.Errorf("Failed to parse: %v", err)
			//~ 	return
			//~ }
			var val = Container.ParseJSON(sample);

			//~ if result, ok := val.Search([]string{"test", "value"}...).Data().(float64); ok {
			//~ 	if result != 10 {
			//~ 		t.Errorf("Wrong value of result: %v", result)
			//~ 	}
			//~ } else {
			//~ 	t.Errorf("Didn't find test.value")
			//~ }
			var result = val.Search("test", "value").Object;
			Assert.IsNotNull(result);
			Assert.AreEqual(10, Convert.ToDouble(result));

			//~ if _, ok := val.Search("test2", "value").Data().(string); ok {
			//~ 	t.Errorf("Somehow found a field that shouldn't exist")
			//~ }
			Assert.IsNull(val.Search("test2", "value").Object);

			//~ if result, ok := val.Search("test2").Data().(float64); ok {
			//~ 	if result != 20 {
			//~ 		t.Errorf("Wrong value of result: %v", result)
			//~ 	}
			//~ } else {
			//~ 	t.Errorf("Didn't find test2")
			//~ }
			result = val.Search("test2").Object;
			Assert.IsNotNull(result);
			Assert.AreEqual(20, Convert.ToDouble(result));

			//~ if result := val.Bytes(); string(result) != string(sample) {
			//~ 	t.Errorf("Wrong []byte conversion: %s != %s", result, sample)
			//~ }
			Assert.AreEqual(sample.ToUtf8String(), val.Bytes().ToUtf8String());
		}

		[TestMethod]
		//~ func TestExists(t *testing.T) {
		public void TestExists()
		{
			//~ sample := []byte(`{"test":{"value":10},"test2":20}`)
			var sample = @"{""test"":{""value"":10},""test2"":20}".ToUtf8Bytes();

			//~ val, err := ParseJSON(sample)
			//~ if err != nil {
			//~ 	t.Errorf("Failed to parse: %v", err)
			//~ 	return
			//~ }
			var val = Container.ParseJSON(sample);


			//~ paths := []struct {
			//~ 	Path   []string
			//~ 	Exists bool
			//~ }{
			//~ 	{[]string{"one", "two", "three"}, false},
			//~ 	{[]string{"test"}, true},
			//~ 	{[]string{"test", "value"}, true},
			//~ 	{[]string{"test2"}, true},
			//~ 	{[]string{"test2", "value"}, false},
			//~ 	{[]string{"test", "value2"}, false},
			//~ 	{[]string{"test", "VALUE"}, false},
			//~ }
			var paths = new(string[] Path, bool Exists)[]
			{
				(new string[] { "one", "two", "three" }, false),
				(new string[] { "test" }, true ),
				(new string[] { "test", "value" }, true),
				(new string[] { "test2" }, true ),
				(new string[] { "test2", "value" }, false),
				(new string[] { "test", "value2"}, false),
				(new string[] { "test", "VALUE" }, false),
			};

			//~ for _, p := range paths {
			//~ 	if exp, actual := p.Exists, val.Exists(p.Path...); exp != actual {
			//~ 		t.Errorf("Wrong result from Exists: %v != %v, for path: %v", exp, actual, p.Path)
			//~ 	}
			//~ 	if exp, actual := p.Exists, val.ExistsP(strings.Join(p.Path, ".")); exp != actual {
			//~ 		t.Errorf("Wrong result from ExistsP: %v != %v, for path: %v", exp, actual, p.Path)
			//~ 	}
			foreach (var p in paths)
			{
				Assert.AreEqual(p.Exists, val.Exists(p.Path));
				Assert.AreEqual(p.Exists, val.ExistsP(string.Join(".", p.Path)));
			}
		}

		[TestMethod]
		//~ func TestExistsWithArrays(t *testing.T) {
		public void TestExistsWithArrays()
		{
			//~ sample := []byte(`{"foo":{"bar":{"baz":[10, 2, 3]}}}`)
			var sample = @"{""foo"":{""bar"":{""baz"":[10, 2, 3]}}}".ToUtf8Bytes();

			//~ val, err := ParseJSON(sample)
			//~ if err != nil {
			//~ 	t.Errorf("Failed to parse: %v", err)
			//~ 	return
			//~ }
			var val = Container.ParseJSON(sample);

			//~ if exp, actual := true, val.Exists("foo", "bar", "baz"); exp != actual {
			//~ 	t.Errorf("Wrong result from array based Exists: %v != %v", exp, actual)
			//~ }
			Assert.IsTrue(val.Exists("foo", "bar", "baz"));

			//~ sample = []byte(`{"foo":{"bar":[{"baz":10},{"baz":2},{"baz":3}]}}`)
			sample = @"{""foo"":{""bar"":[{""baz"":10},{""baz"":2},{""baz"":3}]}}".ToUtf8Bytes();

			//~ if val, err = ParseJSON(sample); err != nil {
			//~ 	t.Errorf("Failed to parse: %v", err)
			//~ 	return
			//~ }
			val = Container.ParseJSON(sample);

			//~ if exp, actual := true, val.Exists("foo", "bar", "baz"); exp != actual {
			//~ 	t.Errorf("Wrong result from array based Exists: %v != %v", exp, actual)
			//~ }
			//~ if exp, actual := false, val.Exists("foo", "bar", "baz_NOPE"); exp != actual {
			//~ 	t.Errorf("Wrong result from array based Exists: %v != %v", exp, actual)
			//~ }
			Assert.IsTrue(val.Exists("foo", "bar", "baz"));
			Assert.IsFalse(val.Exists("foo", "bar", "baz_NOPE"));

			//~ sample = []byte(`{"foo":[{"bar":{"baz":10}},{"bar":{"baz":2}},{"bar":{"baz":3}}]}`)
			sample = @"{""foo"":[{""bar"":{""baz"":10}},{""bar"":{ ""baz"":2}},{""bar"":{ ""baz"":3}}]}".ToUtf8Bytes();

			//~ if val, err = ParseJSON(sample); err != nil {
			//~ 	t.Errorf("Failed to parse: %v", err)
			//~ 	return
			//~ }
			val = Container.ParseJSON(sample);

			//~ if exp, actual := true, val.Exists("foo", "bar", "baz"); exp != actual {
			//~ 	t.Errorf("Wrong result from array based Exists: %v != %v", exp, actual)
			//~ }
			//~ if exp, actual := false, val.Exists("foo", "bar", "baz_NOPE"); exp != actual {
			//~ 	t.Errorf("Wrong result from array based Exists: %v != %v", exp, actual)
			//~ }
			Assert.IsTrue(val.Exists("foo", "bar", "baz"));
			Assert.IsFalse(val.Exists("foo", "bar", "baz_NOPE"));

			//~ sample =
			//~ 	[]byte(`[{ "foo":{ "bar":{ "baz":10} } },{ "foo":{ "bar":{ "baz":2} } },{ "foo":{ "bar":{ "baz":3} } }]`);
			sample = @"[{""foo"":{""bar"":{""baz"":10}}},{""foo"":{""bar"":{""baz"":2}}},{""foo"":{""bar"":{""baz"":3}}}]".ToUtf8Bytes();

			//~ if val, err = ParseJSON(sample); err != nil {
			//~ 	t.Errorf("Failed to parse: %v", err)
			//~ 	return
			//~ }
			val = Container.ParseJSON(sample);

			//~ if exp, actual := true, val.Exists("foo", "bar", "baz"); exp != actual {
			//~ 	t.Errorf("Wrong result from array based Exists: %v != %v", exp, actual)
			//~ }
			//~ if exp, actual := false, val.Exists("foo", "bar", "baz_NOPE"); exp != actual {
			//~ 	t.Errorf("Wrong result from array based Exists: %v != %v", exp, actual)
			//~ }
			Assert.IsTrue(val.Exists("foo", "bar", "baz"));
			Assert.IsFalse(val.Exists("foo", "bar", "baz_NOPE"));
		}

		[TestMethod]
		//~ func TestBasicWithBuffer(t *testing.T) {
		public void TestBasicWithBuffer()
		{
			//~ sample := bytes.NewReader([]byte(`{"test":{"value":10},"test2":20}`))
			var sample = @"{""test"":{""value"":10},""test2"":20}".ToUtf8Bytes();

			//~ _, err := ParseJSONBuffer(sample)
			//~ if err != nil {
			//~ 	t.Errorf("Failed to parse: %v", err)
			//~ 	return
			//~ }
			using (var ms = new MemoryStream(sample))
			{
				Container.ParseJSONBuffer(ms);
			}
		}

		////	[TestMethod]
		////	//~ func TestBasicWithDecoder(t *testing.T) {
		////	public void TestBasicWithDecoder()
		////	{
		////		//~ sample := []byte(`{"test":{"int":10, "float":6.66}}`)
		////		//~ dec := json.NewDecoder(bytes.NewReader(sample))
		////		//~ dec.UseNumber()
		////		var sample = @"{""test"":{""int"":10, ""float"":6.66}}".ToUtf8Bytes();
		////
		////		val, err := ParseJSONDecoder(dec)
		////		if err != nil {
		////			t.Errorf("Failed to parse: %v", err)
		////			return
		////		}
		////
		////		checkNumber := func(path string, expectedVal json.Number) {
		////			data := val.Path(path).Data()
		////			asNumber, isNumber := data.(json.Number)
		////			if !isNumber {
		////				t.Error("Failed to parse using decoder UseNumber policy")
		////			}
		////			if expectedVal != asNumber {
		////				t.Errorf("Expected[%s] but got [%s]", expectedVal, asNumber)
		////			}
		////		}
		////
		////		checkNumber("test.int", "10")
		////		checkNumber("test.float", "6.66")
		////	}
		////
		////	func TestFailureWithDecoder(t *testing.T) {
		////		sample := []byte(`{"test":{" "invalidCrap":.66}}`)
		////		dec := json.NewDecoder(bytes.NewReader(sample))
		////
		////		_, err := ParseJSONDecoder(dec)
		////		if err == nil {
		////			t.Fatal("Expected parsing error")
		////		}
		////	}

		[TestMethod]
		//~ func TestFindArray(t *testing.T) {
		public void TestFindArray()
		{
			//~ for i, this := range []struct {
			//~ 	input  string
			//~ 	target string
			//~ 	expect string
			//~ }{
			var i = 0;
			foreach (var @this in new(string input, string target, string expect)[]
				{
				//~ {
				//~ 	`{"test":{"array":[{"value":1}, {"value":2}, {"value":3}]}}`,
				//~ 	"",
				//~ 	"[1,2,3]",
				//~ },
					(
						@"{""test"":{""array"":[{""value"":1}, {""value"":2}, {""value"":3}]}}",
						@"test.array.value",
						@"[1,2,3]"
					),
				//~ {
				//~ 	`{
				//~ 	"test":{
				//~ 		"array":[
				//~ 				{
				//~ 					"values":[
				//~ 						{"more":1},
				//~ 						{"more":2},
				//~ 						{"more":3}
				//~ 					]
				//~ 				},
				//~ 				{
				//~ 					"values":[
				//~ 						{"more":4},
				//~ 						{"more":5},
				//~ 						{"more":6}
				//~ 					]
				//~ 				},
				//~ 				{
				//~ 					"values":[
				//~ 						{"more":7},
				//~ 						{"more":8},
				//~ 						{"more":9}
				//~ 					]
				//~ 				}
				//~ 			]
				//~ 		}
				//~ 	}`,
				//~ 	"test.array.values.more",
				//~ 	"[[1,2,3],[4,5,6],[7,8,9]]",
				//~ },
					(
						@"{
						""test"":{
							""array"":[
									{
										""values"":[
											{""more"":1},
											{""more"":2},
											{""more"":3}
										]
									},
									{
										""values"":[
											{""more"":4},
											{""more"":5},
											{""more"":6}
										]
									},
									{
										""values"":[
											{""more"":7},
											{""more"":8},
											{""more"":9}
										]
									}
								]
							}
						}",
						"test.array.values.more",
						"[[1,2,3],[4,5,6],[7,8,9]]"
					)
				})
			{
				//~ val, err := ParseJSON([]byte(this.input))
				//~ if err != nil {
				//~ 	t.Errorf("[%d] Failed to parse: %s", i, err)
				//~ 	return
				//~ }
				var val = Container.ParseJSON(@this.input.ToUtf8Bytes());

				//~ target := val.Path(this.target)
				//~ result := target.String()
				var target = val.Path(@this.target);
				var result = target.ToString();

				//~ if this.expect != result {
				//~ 	t.Errorf("[%d] Expected %v, received %v", i, this.expect, result)
				//~ }
				Assert.AreEqual(@this.expect, result);

				i++;
			}
		}

		[TestMethod]
		//~ func TestDeletes(t *testing.T) {
		public void TestDeletes()
		{
			//~ jsonParsed, _ := ParseJSON([]byte(`{
			//~ 	"outter":{
			//~ 		"inner":{
			//~ 			"value1":10,
			//~ 			"value2":22,
			//~ 			"value3":32
			//~ 		},
			//~ 		"alsoInner":{
			//~ 			"value1":20,
			//~ 			"value2":42,
			//~ 			"value3":92
			//~ 		},
			//~ 		"another":{
			//~ 			"value1":null,
			//~ 			"value2":null,
			//~ 			"value3":null
			//~ 		}
			//~ 	}
			//~ }`))
			var jsonParsed = Container.ParseJSON(@"{
			 	""outter"":{
			 		""inner"":{
			 			""value1"":10,
			 			""value2"":22,
			 			""value3"":32
			 		},
			 		""alsoInner"":{
			 			""value1"":20,
			 			""value2"":42,
			 			""value3"":92
			 		},
			 		""another"":{
			 			""value1"":null,
			 			""value2"":null,
			 			""value3"":null
			 		}
			 	}
			}".ToUtf8Bytes());

			//~ if err := jsonParsed.Delete("outter", "inner", "value2"); err != nil {
			//~ 	t.Error(err)
			//~ }
			jsonParsed.Delete("outter", "inner", "value2");
			//~ if err := jsonParsed.Delete("outter", "inner", "value4"); err == nil {
			//~ 	t.Error(fmt.Errorf("value4 should not have been found in outter.inner"))
			//~ }
			Assert.ThrowsException<Container.ErrNotObj>(() =>
				jsonParsed.Delete("outter", "inner", "value4"));
			//~ if err := jsonParsed.Delete("outter", "another", "value1"); err != nil {
			//~ 	t.Error(err)
			//~ }
			jsonParsed.Delete("outter", "another", "value1");
			//~ if err := jsonParsed.Delete("outter", "another", "value4"); err == nil {
			//~ 	t.Error(fmt.Errorf("value4 should not have been found in outter.another"))
			//~ }
			Assert.ThrowsException<Container.ErrNotObj>(() =>
				jsonParsed.Delete("outter", "another", "value4"));
			//~ if err := jsonParsed.DeleteP("outter.alsoInner.value1"); err != nil {
			//~ 	t.Error(err)
			//~ }
			jsonParsed.DeleteP("outter.alsoInner.value1");
			//~ if err := jsonParsed.DeleteP("outter.alsoInner.value4"); err == nil {
			//~ 	t.Error(fmt.Errorf("value4 should not have been found in outter.alsoInner"))
			//~ }
			Assert.ThrowsException<Container.ErrNotObj>(() =>
				jsonParsed.DeleteP("outter.alsoInner.value4"));
			//~ if err := jsonParsed.DeleteP("outter.another.value2"); err != nil {
			//~ 	t.Error(err)
			//~ }
			jsonParsed.DeleteP("outter.another.value2");
			//~ if err := jsonParsed.Delete("outter.another.value4"); err == nil {
			//~ 	t.Error(fmt.Errorf("value4 should not have been found in outter.another"))
			//~ }
			Assert.ThrowsException<Container.ErrNotObj>(() =>
				jsonParsed.Delete("outter.another.value4"));

			//~ expected := `{"outter":{"alsoInner":{"value2":42,"value3":92},"another":{"value3":null},"inner":{"value1":10,"value3":32}}}`
			//~ if actual := jsonParsed.String(); actual != expected {
			//~ 	t.Errorf("Unexpected result from deletes: %v != %v", actual, expected)
			//~ }
			var expected = @"{""outter"":{""alsoInner"":{""value2"":42,""value3"":92},""another"":{""value3"":null},""inner"":{""value1"":10,""value3"":32}}}";
			Assert.AreEqual(expected, jsonParsed.ToString());
		}

		[TestMethod]
		//~ func TestExamples(t *testing.T) {
		public void TestExamples()
		{
			//~ jsonParsed, _ := ParseJSON([]byte(`{
			//~ 	"outter":{
			//~ 		"inner":{
			//~ 			"value1":10,
			//~ 			"value2":22
			//~ 		},
			//~ 		"alsoInner":{
			//~ 			"value1":20
			//~ 		}
			//~ 	}
			//~ }`))
			var jsonParsed = Container.ParseJSON(@"{
				""outter"":{
					""inner"":{
						""value1"":10,
						""value2"":22
					},
					""alsoInner"":{
						""value1"":20
					}
				}
			}".ToUtf8Bytes());

			//~ var value float64
			//~ var ok bool

			//~ value, ok = jsonParsed.Path("outter.inner.value1").Data().(float64)
			//~ if value != 10.0 || !ok {
			//~ 	t.Errorf("wrong value: %v, %v", value, ok)
			//~ }
			Assert.AreEqual(10.0, Convert.ToDouble(jsonParsed.Path("outter.inner.value1").Object));

			//~ value, ok = jsonParsed.Search("outter", "inner", "value1").Data().(float64)
			//~ if value != 10.0 || !ok {
			//~ 	t.Errorf("wrong value: %v, %v", value, ok)
			//~ }
			Assert.AreEqual(10.0, Convert.ToDouble(jsonParsed.Search("outter", "inner", "value1").Object));

			//~ value, ok = jsonParsed.Path("does.not.exist").Data().(float64)
			//~ if value != 0.0 || ok {
			//~ 	t.Errorf("wrong value: %v, %v", value, ok)
			//~ }
			Assert.AreEqual(0.0, Convert.ToDouble(jsonParsed.Path("does.not.exist").Object));

			//~ jsonParsed, _ = ParseJSON([]byte(`{"array":[ "first", "second", "third" ]}`))
			jsonParsed = Container.ParseJSON(@"{""array"":[ ""first"", ""second"", ""third"" ]}".ToUtf8Bytes());

			//~ expected := []string{"first", "second", "third"}
			var expected = new string[] { "first", "second", "third" };

			//~ children, err := jsonParsed.S("array").Children()
			//~ if err != nil {
			//~ 	t.Errorf("Error: %v", err)
			//~ 	return
			//~ }
			var children = jsonParsed.S("array").Children();
			//~ for i, child := range children {
			//~ 	if expected[i] != child.Data().(string) {
			//~ 		t.Errorf("Child unexpected: %v != %v", expected[i], child.Data().(string))
			//~ 	}
			//~ }
			var i = 0;
			foreach (var child in children)
				Assert.AreEqual(expected[i++], child.Object);
		}

		[TestMethod]
		//~ func TestExamples2(t *testing.T) {
		public void TestExamples2()
		{
			//~ var err error

			//~ jsonObj := New()
			var jsonObj = Container.New();

			//~ _, err = jsonObj.Set(10, "outter", "inner", "value")
			//~ if err != nil {
			//~ 	t.Errorf("Error: %v", err)
			//~ 	return
			//~ }
			jsonObj.Set(10, "outter", "inner", "value");
			//~ _, err = jsonObj.SetP(20, "outter.inner.value2")
			//~ if err != nil {
			//~ 	t.Errorf("Error: %v", err)
			//~ 	return
			//~ }
			jsonObj.SetP(20, "outter.inner.value2");
			//~ _, err = jsonObj.Set(30, "outter", "inner2", "value3")
			//~ if err != nil {
			//~ 	t.Errorf("Error: %v", err)
			//~ 	return
			//~ }
			jsonObj.Set(30, "outter", "inner2", "value3");

			//~ expected := `{"outter":{"inner":{"value":10,"value2":20},"inner2":{"value3":30}}}`
			//~ if jsonObj.String() != expected {
			//~ 	t.Errorf("Non matched output: %v != %v", expected, jsonObj.String())
			//~ }
			var expected = @"{""outter"":{""inner"":{""value"":10,""value2"":20},""inner2"":{""value3"":30}}}";
			Assert.AreEqual(expected, jsonObj.ToString());

			//~ jsonObj, _ = Consume(map[string]interface{}{})
			jsonObj = Container.Consume(new Dictionary<string, object>());

			//~ jsonObj.Array("array")
			jsonObj.Array("array");

			//~ jsonObj.ArrayAppend(10, "array")
			//~ jsonObj.ArrayAppend(20, "array")
			//~ jsonObj.ArrayAppend(30, "array")
			jsonObj.ArrayAppend(10, "array");
			jsonObj.ArrayAppend(20, "array");
			jsonObj.ArrayAppend(30, "array");

			//~ expected = `{
			//~   "array": [
			//~ 	10,
			//~ 	20,
			//~ 	30
			//~   ]
			//~ }`
			expected = @"{
        ""array"": [
          10,
          20,
          30
        ]
      }";
			//~ result := jsonObj.StringIndent("    ", "  ")
			//~ if result != expected {
			//~ 	t.Errorf("Non matched output: %v != %v", expected, result)
			//~ }
			var result = jsonObj.ToStringIndented("    ", "  ");
			Assert.AreEqual(expected, result);
		}

		[TestMethod]
		//~ func TestExamples3(t *testing.T) {
		public void TestExamples3()
		{
			//~ jsonObj := New()
			var jsonObj = Container.New();

			//~ jsonObj.ArrayP("foo.array")
			jsonObj.ArrayP("foo.array");

			//~ jsonObj.ArrayAppend(10, "foo", "array")
			//~ jsonObj.ArrayAppend(20, "foo", "array")
			//~ jsonObj.ArrayAppend(30, "foo", "array")
			jsonObj.ArrayAppend(10, "foo", "array");
			jsonObj.ArrayAppend(20, "foo", "array");
			jsonObj.ArrayAppend(30, "foo", "array");

			//~ result:= jsonObj.String()
			//~ expected := `{"foo":{"array":[10,20,30]}}`
			var result = jsonObj.ToString();
			var expected = @"{""foo"":{""array"":[10,20,30]}}";

			//~ if result != expected {
			//~ 	t.Errorf("Non matched output: %v != %v", result, expected)
			//~ }
			Assert.AreEqual(expected, result);
		}

		[TestMethod]
		//~ func TestDotNotation(t *testing.T) {
		public void TestDotNotation()
		{
			//~ sample := []byte(`{"test":{"inner":{"value":10}},"test2":20}`)
			var sample = @"{""test"":{""inner"":{""value"":10}},""test2"":20}".ToUtf8Bytes();

			//~ val, err := ParseJSON(sample)
			//~ if err != nil {
			//~ 	t.Errorf("Failed to parse: %v", err)
			//~ 	return
			//~ }
			var val = Container.ParseJSON(sample);

			//~ if result, _ := val.Path("test.inner.value").Data().(float64); result != 10 {
			//~ 	t.Errorf("Expected 10, received: %v", result)
			//~ }
			var result = val.Path("test.inner.value");
			Assert.AreEqual(10, Convert.ToDouble(result.Object));
		}
		[TestMethod]
		//~ func TestModify(t *testing.T) {
		public void TestModify()
		{
			//~ sample := []byte(`{"test":{"value":10},"test2":20}`)
			var sample = @"{""test"":{""value"":10},""test2"":20}".ToUtf8Bytes();

			//~ val, err := ParseJSON(sample)
			//~ if err != nil {
			//~ 	t.Errorf("Failed to parse: %v", err)
			//~ 	return
			//~ }
			var val = Container.ParseJSON(sample);

			//~ if _, err := val.S("test").Set(45.0, "value"); err != nil {
			//~ 	t.Errorf("Failed to set field")
			//~ }
			val.S("test").Set(45.0, "value");

			//~ if result, ok := val.Search([]string{"test", "value"}...).Data().(float64); ok {
			//~ 	if result != 45 {
			//~ 		t.Errorf("Wrong value of result: %v", result)
			//~ 	}
			//~ } else {
			//~ 	t.Errorf("Didn't find test.value")
			//~ }
			var result = val.Search("test", "value").Object;
			Assert.AreEqual(45.0, Convert.ToDouble(result));

			//~ if out := val.String(); `{"test":{"value":45},"test2":20}` != out {
			//~ 	t.Errorf("Incorrectly serialized: %v", out)
			//~ }
			Assert.AreEqual(@"{""test"":{""value"":45.0},""test2"":20}", val.ToString());

			//~ if out := val.Search("test").String(); `{"value":45}` != out {
			//~ 	t.Errorf("Incorrectly serialized: %v", out)
			//~ }
			Assert.AreEqual(@"{""value"":45.0}", val.Search("test").ToString());
		}

		[TestMethod]
		//~ func TestChildren(t *testing.T) {
		public void TestChildren()
		{
			//~ json1, _ := ParseJSON([]byte(`{
			//~ 	"objectOne":{
			//~ 	},
			//~ 	"objectTwo":{
			//~ 	},
			//~ 	"objectThree":{
			//~ 	}
			//~ }`))
			var json1 = Container.ParseJSON(@"{
				""objectOne"":{
				},
				""objectTwo"":{
				},
				""objectThree"":{
				}
			}".ToUtf8Bytes());

			//~ objects, _ := json1.Children()
			//~ for _, object := range objects {
			//~ 	object.Set("hello world", "child")
			//~ }
			var objects = json1.Children();
			foreach (var obj in objects)
				obj.Set("hello world", "child");

			//~ expected := `{"objectOne":{"child":"hello world"},"objectThree":{"child":"hello world"}` +
			//~ 	`,"objectTwo":{"child":"hello world"}}`
			var expected = @"{""objectOne"":{""child"":""hello world""},""objectThree"":{""child"":""hello world""}"
				+ @",""objectTwo"":{""child"":""hello world""}}";
			//~ received := json1.String()
			//~ if expected != received {
			//~ 	t.Errorf("json1: expected %v, received %v", expected, received)
			//~ }
			var received = json1.ToString();
			Assert.AreEqual(expected, received);

			//~ json2, _ := ParseJSON([]byte(`{
			//~ 	"values":[
			//~ 		{
			//~ 			"objectOne":{
			//~ 			}
			//~ 		},
			//~ 		{
			//~ 			"objectTwo":{
			//~ 			}
			//~ 		},
			//~ 		{
			//~ 			"objectThree":{
			//~ 			}
			//~ 		}
			//~ 	]
			//~ }`))
			var json2 = Container.ParseJSON(@"{
				""values"":[
					{
						""objectOne"":{
						}
					},
					{
						""objectTwo"":{
						}
					},
					{
						""objectThree"":{
						}
					}
				]
			}".ToUtf8Bytes());

			//~ json3, _ := ParseJSON([]byte(`{
			//~ 	"values":[
			//~ 	]
			//~ }`))
			var json3 = Container.ParseJSON(@"{
				""values"":[
				]
			}".ToUtf8Bytes());

			//~ numChildren1, _ := json2.ArrayCount("values")
			//~ numChildren2, _ := json3.ArrayCount("values")
			//~ if _, err := json3.ArrayCount("valuesNOTREAL"); err == nil {
			//~ 	t.Errorf("expected numChildren3 to fail")
			//~ }
			var numChildren1 = json2.ArrayCount("values");
			var numChildren2 = json3.ArrayCount("values");
			Assert.ThrowsException<Container.ErrNotArray>(() => json3.ArrayCount("valuesNOTREAL"));

			//~ if numChildren1 != 3 || numChildren2 != 0 {
			//~ 	t.Errorf("CountElements, expected 3 and 0, received %v and %v",
			//~ 		numChildren1, numChildren2)
			//~ }
			Assert.AreEqual(3, numChildren1);
			Assert.AreEqual(0, numChildren2);

			//~ objects, _ = json2.S("values").Children()
			//~ for _, object := range objects {
			//~ 	object.Set("hello world", "child")
			//~ 	json3.ArrayAppend(object.Data(), "values")
			//~ }
			var objs = json2.S("values").Children();
			foreach (var obj in objs)
			{
				obj.Set("hello world", "child");
				json3.ArrayAppend(obj.Object, "values");
			}

			//~ expected = `{"values":[{"child":"hello world","objectOne":{}},{"child":"hello world",` +
			//~ 	`"objectTwo":{}},{"child":"hello world","objectThree":{}}]}`
			//~ received = json2.String()
			//~ if expected != received {
			//~ 	t.Errorf("json2: expected %v, received %v", expected, received)
			//~ }
			expected = @"{""values"":[{""child"":""hello world"",""objectOne"":{}},{""child"":""hello world"","
					+ @"""objectTwo"":{}},{""child"":""hello world"",""objectThree"":{}}]}";
			received = json2.ToString();
			Assert.AreEqual(expected, received);

			//~ received = json3.String()
			//~ if expected != received {
			//~ 	t.Errorf("json3: expected %v, received %v", expected, received)
			//~ }
			received = json3.ToString();
			Assert.AreEqual(expected, received);
		}

		[TestMethod]
		//~ func TestChildrenMap(t *testing.T) {
		public void TestChildrenMap()
		{
			//~ json1, _:= ParseJSON([]byte(`{
			//~ 	"objectOne":{ "num":1},
			//~ 	"objectTwo":{ "num":2},
			//~ 	"objectThree":{ "num":3}
			//~ }`))
			var json1 = Container.ParseJSON(@"{
				""objectOne"":{ ""num"":1},
				""objectTwo"":{ ""num"":2},
				""objectThree"":{ ""num"":3}
			}".ToUtf8Bytes());

			//~ objectMap, err := json1.ChildrenMap()
			//~ if err != nil {
			//~ 	t.Error(err)
			//~ 	return
			//~ }
			var objectMap = json1.ChildrenMap();

			//~ if len(objectMap) != 3 {
			//~ 	t.Errorf("Wrong num of elements in objectMap: %v != %v", len(objectMap), 3)
			//~ 	return
			//~ }
			Assert.AreEqual(3, objectMap.Count);

			//~ for key, val := range objectMap {
			foreach (var kv in objectMap)
			{
				//~ if "objectOne" == key {
				//~ 	if val := val.S("num").Data().(float64); val != 1 {
				//~ 		t.Errorf("%v != %v", val, 1)
				//~ 	}
				//~ } else if "objectTwo" == key {
				//~ 	if val := val.S("num").Data().(float64); val != 2 {
				//~ 		t.Errorf("%v != %v", val, 2)
				//~ 	}
				//~ } else if "objectThree" == key {
				//~ 	if val := val.S("num").Data().(float64); val != 3 {
				//~ 		t.Errorf("%v != %v", val, 3)
				//~ 	}
				//~ } else {
				//~ 	t.Errorf("Unexpected key: %v", key)
				//~ }
				switch (kv.Key)
				{
					case "objectOne":
						Assert.AreEqual(1, Convert.ToDouble(kv.Value.S("num").Object));
						break;
					case "objectTwo":
						Assert.AreEqual(2, Convert.ToDouble(kv.Value.S("num").Object));
						break;
					case "objectThree":
						Assert.AreEqual(3, Convert.ToDouble(kv.Value.S("num").Object));
						break;
					default:
						Assert.Fail();
						break;
				}
			}

			//~ objectMap["objectOne"].Set(500, "num")
			//~ if val := json1.Path("objectOne.num").Data().(int); val != 500 {
			//~ 	t.Errorf("set objectOne failed: %v != %v", val, 500)
			//~ }
			objectMap["objectOne"].Set(500, "num");
			Assert.AreEqual(500, json1.Path("objectOne.num").Object);
		}

		[TestMethod]
		//~ func TestNestedAnonymousArrays(t *testing.T) {
		public void TestNestedAnonymousArrays()
		{
			//~ json1, _ := ParseJSON([]byte(`{
			//~ 	"array":[
			//~ 		[ 1, 2, 3 ],
			//~ 		[ 4, 5, 6 ],
			//~ 		[ 7, 8, 9 ],
			//~ 		[{ "test" : 50 }]
			//~ 	]
			//~ }`))
			var json1 = Container.ParseJSON(@"{
				""array"":[
					[ 1, 2, 3 ],
					[ 4, 5, 6 ],
					[ 7, 8, 9 ],
					[{ ""test"" : 50 }]
				]
			}".ToUtf8Bytes());

			//~ childTest, err := json1.S("array").Index(0).Children()
			//~ if err != nil {
			//~ 	t.Error(err)
			//~ 	return
			//~ }
			var childTest = json1.S("array").Index(0).Children();

			//~ if val := childTest[0].Data().(float64); val != 1 {
			//~ 	t.Errorf("child test: %v != %v", val, 1)
			//~ }
			//~ if val := childTest[1].Data().(float64); val != 2 {
			//~ 	t.Errorf("child test: %v != %v", val, 2)
			//~ }
			//~ if val := childTest[2].Data().(float64); val != 3 {
			//~ 	t.Errorf("child test: %v != %v", val, 3)
			//~ }
			Assert.AreEqual(1, Convert.ToDouble(childTest[0].Object));
			Assert.AreEqual(2, Convert.ToDouble(childTest[1].Object));
			Assert.AreEqual(3, Convert.ToDouble(childTest[2].Object));

			//~ if val := json1.Path("array").Index(1).Index(1).Data().(float64); val != 5 {
			//~ 	t.Errorf("nested child test: %v != %v", val, 5)
			//~ }
			//~ 
			//~ if val := json1.Path("array").Index(3).Index(0).S("test").Data().(float64); val != 50 {
			//~ 	t.Errorf("nested child object test: %v != %v", val, 50)
			//~ }
			Assert.AreEqual(5, Convert.ToDouble(json1.Path("array").Index(1).Index(1).Object));
			Assert.AreEqual(50, Convert.ToDouble(json1.Path("array").Index(3).Index(0).S("test").Object));

			//~ json1.Path("array").Index(3).Index(0).Set(200, "test")
			//~ 
			//~ if val := json1.Path("array").Index(3).Index(0).S("test").Data().(int); val != 200 {
			//~ 	t.Errorf("set nested child object: %v != %v", val, 200)
			//~ }
			json1.Path("array").Index(3).Index(0).Set(200, "test");
			Assert.AreEqual(200, json1.Path("array").Index(3).Index(0).S("test").Object);
		}

		[TestMethod]
		//~ func TestArrays(t *testing.T) {
		public void TestArrays()
		{
			//~ json1, _ := ParseJSON([]byte(`{
			//~ 	"languages":{
			//~ 		"english":{
			//~ 			"places":0
			//~ 		},
			//~ 		"french": {
			//~ 			"places": [
			//~ 				"france",
			//~ 				"belgium"
			//~ 			]
			//~ 		}
			//~ 	}
			//~ }`))
			//~ 
			//~ json2, _ := ParseJSON([]byte(`{
			//~ 	"places":[
			//~ 		"great_britain",
			//~ 		"united_states_of_america",
			//~ 		"the_world"
			//~ 	]
			//~ }`))
			var json1 = Container.ParseJSON(@"{
				""languages"":{
					""english"":{
						""places"":0
					},
					""french"": {
						""places"": [
							""france"",
							""belgium""
						]
					}
				}
			}".ToUtf8Bytes());

			var json2 = Container.ParseJSON(@"{
				""places"":[
					""great_britain"",
					""united_states_of_america"",
					""the_world""
				]
			}".ToUtf8Bytes());


			//~ if englishPlaces := json2.Search("places").Data(); englishPlaces != nil {
			//~ 	json1.Path("languages.english").Set(englishPlaces, "places")
			//~ } else {
			//~ 	t.Errorf("Didn't find places in json2")
			//~ }
			var englishPlaces = json2.Search("places").Object;
			Assert.IsNotNull(englishPlaces);
			json1.Path("languages.english").Set(englishPlaces, "places");

			//~ if englishPlaces := json1.Search("languages", "english", "places").Data(); englishPlaces != nil {
			//~
			//~	englishArray, ok := englishPlaces.([]interface{})
			//~	if !ok {
			//~		t.Errorf("places in json1 (%v) was not an array", englishPlaces)
			//~	}
			//~
			//~	if len(englishArray) != 3 {
			//~		t.Errorf("wrong length of array: %v", len(englishArray))
			//~	}
			//~
			//~} else {
			//~	t.Errorf("Didn't find places in json1")
			//~}
			englishPlaces = json1.Search("languages", "english", "places").Object;
			Assert.IsNotNull(englishPlaces);
			var englishArray = (IList<object>)englishPlaces;
			Assert.AreEqual(3, englishArray.Count);

			//~ for i := 0; i < 3; i++ {
			//~ 	if err := json2.ArrayRemove(0, "places"); err != nil {
			//~ 		t.Errorf("Error removing element: %v", err)
			//~ 	}
			//~ }
			for (int i = 0; i < 3; i++)
				json2.ArrayRemove(0, "places");

			//~ json2.ArrayAppend(map[string]interface{}{}, "places")
			//~ json2.ArrayAppend(map[string]interface{}{}, "places")
			//~ json2.ArrayAppend(map[string]interface{}{}, "places")
			json2.ArrayAppend(new Dictionary<string, object>(), "places");
			json2.ArrayAppend(new Dictionary<string, object>(), "places");
			json2.ArrayAppend(new Dictionary<string, object>(), "places");

			// Using float64 for this test even though it's completely inappropriate because
			// later on the API might do something clever with types, in which case all numbers
			// will become float64.
			//~ for i := 0; i < 3; i++ {
			//~ 	obj, _ := json2.ArrayElement(i, "places")
			//~ 	obj2, _ := obj.Object(fmt.Sprintf("object%v", i))
			//~ 	obj2.Set(float64(i), "index")
			//~ }
			for (int i = 0; i < 3; i++)
			{
				var obj = json2.ArrayElement(i, "places");
				var obj2 = obj.ObjectJ($"object{i}");
				obj2.Set(Convert.ToDouble(i), "index");
			}

			//! children, _ := json2.S("places").Children()
			//! for i, obj := range children {
			//! 	if id, ok := obj.S(fmt.Sprintf("object%v", i)).S("index").Data().(float64); ok {
			//! 		if id != float64(i) {
			//! 			t.Errorf("Wrong index somehow, expected %v, received %v", i, id)
			//! 		}
			//! 	} else {
			//! 		t.Errorf("Failed to find element %v from %v", i, obj)
			//! 	}
			//! }
			var children = json2.S("places").Children();
			for (int i = 0; i < children.Length; i++)
			{
				var obj = children[i];
				var id = Convert.ToDouble(obj.S($"object{i}").S("index").Object);
				Assert.IsNotNull(i);
				Assert.AreEqual(Convert.ToDouble(i), id);
			}

			//~ if err := json2.ArrayRemove(1, "places"); err != nil {
			//~ 	t.Errorf("Error removing element: %v", err)
			//~ }
			json2.ArrayRemove(1, "places");

			//~ expected := `{"places":[{"object0":{"index":0}},{"object2":{"index":2}}]}`
			//~ received := json2.String()
			var expected = @"{""places"":[{""object0"":{""index"":0.0}},{""object2"":{""index"":2.0}}]}";
			var received = json2.ToString();

			//~ if expected != received {
			//~ 	t.Errorf("Wrong output, expected: %v, received: %v", expected, received)
			//~ }
			Assert.AreEqual(expected, received);
		}

		[TestMethod]
		//~ func TestArraysTwo(t *testing.T) {
		public void TestArraysTwo()
		{
			//~ json1 := New()
			var json1 = Container.New();

			//~ test1, err := json1.ArrayOfSize(4, "test1")
			//~ if err != nil {
			//~ 	t.Error(err)
			//~ }
			var test1 = json1.ArrayOfSize(4, "test1");

			//~ if _, err = test1.ArrayOfSizeI(2, 0); err != nil {
			//~ 	t.Error(err)
			//~ }
			//~ if _, err = test1.ArrayOfSizeI(2, 1); err != nil {
			//~ 	t.Error(err)
			//~ }
			//~ if _, err = test1.ArrayOfSizeI(2, 2); err != nil {
			//~ 	t.Error(err)
			//~ }
			//~ if _, err = test1.ArrayOfSizeI(2, 3); err != nil {
			//~ 	t.Error(err)
			//~ }
			test1.ArrayOfSizeI(2, 0);
			test1.ArrayOfSizeI(2, 1);
			test1.ArrayOfSizeI(2, 2);
			test1.ArrayOfSizeI(2, 3);

			//~ if _, err = test1.ArrayOfSizeI(2, 4); err != ErrOutOfBounds {
			//~ 	t.Errorf("Index should have been out of bounds")
			//~ }
			Assert.ThrowsException<Container.ErrOutOfBounds>(() => test1.ArrayOfSizeI(2, 4));

			//~ if _, err = json1.S("test1").Index(0).SetIndex(10, 0); err != nil {
			//~ 	t.Error(err)
			//~ }
			//~ if _, err = json1.S("test1").Index(0).SetIndex(11, 1); err != nil {
			//~ 	t.Error(err)
			//~ }
			//~ 
			//~ if _, err = json1.S("test1").Index(1).SetIndex(12, 0); err != nil {
			//~ 	t.Error(err)
			//~ }
			//~ if _, err = json1.S("test1").Index(1).SetIndex(13, 1); err != nil {
			//~ 	t.Error(err)
			//~ }
			json1.S("test1").Index(0).SetIndex(10, 0);
			json1.S("test1").Index(0).SetIndex(11, 1);
			json1.S("test1").Index(1).SetIndex(12, 0);
			json1.S("test1").Index(1).SetIndex(13, 1);

			//~ if _, err = json1.S("test1").Index(2).SetIndex(14, 0); err != nil {
			//~ 	t.Error(err)
			//~ }
			//~ if _, err = json1.S("test1").Index(2).SetIndex(15, 1); err != nil {
			//~ 	t.Error(err)
			//~ }
			json1.S("test1").Index(2).SetIndex(14, 0);
			json1.S("test1").Index(2).SetIndex(15, 1);

			//~ if _, err = json1.S("test1").Index(3).SetIndex(16, 0); err != nil {
			//~ 	t.Error(err)
			//~ }
			//~ if _, err = json1.S("test1").Index(3).SetIndex(17, 1); err != nil {
			//~ 	t.Error(err)
			//~ }
			json1.S("test1").Index(3).SetIndex(16, 0);
			json1.S("test1").Index(3).SetIndex(17, 1);

			//~ if val := json1.S("test1").Index(0).Index(0).Data().(int); val != 10 {
			//~ 	t.Errorf("create array: %v != %v", val, 10)
			//~ }
			//~ if val := json1.S("test1").Index(0).Index(1).Data().(int); val != 11 {
			//~ 	t.Errorf("create array: %v != %v", val, 11)
			//~ }
			Assert.AreEqual(10, json1.S("test1").Index(0).Index(0).Object);
			Assert.AreEqual(11, json1.S("test1").Index(0).Index(1).Object);

			//~ if val := json1.S("test1").Index(1).Index(0).Data().(int); val != 12 {
			//~ 	t.Errorf("create array: %v != %v", val, 12)
			//~ }
			//~ if val := json1.S("test1").Index(1).Index(1).Data().(int); val != 13 {
			//~ 	t.Errorf("create array: %v != %v", val, 13)
			//~ }
			Assert.AreEqual(12, json1.S("test1").Index(1).Index(0).Object);
			Assert.AreEqual(13, json1.S("test1").Index(1).Index(1).Object);

			//~ if val := json1.S("test1").Index(2).Index(0).Data().(int); val != 14 {
			//~ 	t.Errorf("create array: %v != %v", val, 14)
			//~ }
			//~ if val := json1.S("test1").Index(2).Index(1).Data().(int); val != 15 {
			//~ 	t.Errorf("create array: %v != %v", val, 15)
			//~ }
			Assert.AreEqual(14, json1.S("test1").Index(2).Index(0).Object);
			Assert.AreEqual(15, json1.S("test1").Index(2).Index(1).Object);

			//~ if val := json1.S("test1").Index(3).Index(0).Data().(int); val != 16 {
			//~ 	t.Errorf("create array: %v != %v", val, 16)
			//~ }
			//~ if val := json1.S("test1").Index(3).Index(1).Data().(int); val != 17 {
			//~ 	t.Errorf("create array: %v != %v", val, 17)
			//~ }
			Assert.AreEqual(16, json1.S("test1").Index(3).Index(0).Object);
			Assert.AreEqual(17, json1.S("test1").Index(3).Index(1).Object);
		}

		[TestMethod]
		//~ func TestArraysThree(t *testing.T) {
		public void TestArraysThree()
		{
			//~ json1 := New()
			var json1 = Container.New();

			//~ test, err := json1.ArrayOfSizeP(1, "test1.test2")
			//~ if err != nil {
			//~ 	t.Error(err)
			//~ }
			var test = json1.ArrayOfSizeP(1, "test1.test2");

			//~ test.SetIndex(10, 0)
			//~ if val := json1.S("test1", "test2").Index(0).Data().(int); val != 10 {
			//~ 	t.Error(err)
			//~ }
			test.SetIndex(10, 0);
			Assert.AreEqual(10, json1.S("test1", "test2").Index(0).Object);
		}

		[TestMethod]
		//~ func TestArraysRoot(t *testing.T) {
		public void TestArraysRoot()
		{
			//~ sample := []byte(`["test1"]`)
			var sample = @"[""test1""]".ToUtf8Bytes();

			//~ val, err := ParseJSON(sample)
			//~ if err != nil {
			//~ 	t.Errorf("Failed to parse: %v", err)
			//~ 	return
			//~ }
			var val = Container.ParseJSON(sample);

			//~ val.ArrayAppend("test2")
			//~ val.ArrayAppend("test3")
			//~ if obj, err := val.ObjectI(2); err != nil {
			//~ 	t.Error(err)
			//~ } else {
			//~ 	obj.Set("bar", "foo")
			//~ }
			val.ArrayAppend("test2");
			val.ArrayAppend("test3");
			var obj = val.ObjectI(2);
			Assert.IsNotNull(obj);
			obj.Set("bar", "foo");

			//~ if expected, actual := `["test1","test2",{"foo":"bar"}]`, val.String(); expected != actual {
			//~ 	t.Errorf("expected %v, received: %v", expected, actual)
			//~ }
			var expected = @"[""test1"",""test2"",{""foo"":""bar""}]";
			var actual = val.ToString();
			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		//~ func TestLargeSample(t *testing.T) {
		public void TestLargeSample()
		{
			//~ sample := []byte(`{
			//~ 	"test":{
			//~ 		"innerTest":{
			//~ 			"value":10,
			//~ 			"value2":22,
			//~ 			"value3":{
			//~ 				"moreValue":45
			//~ 			}
			//~ 		}
			//~ 	},
			//~ 	"test2":20
			//~ }`)
			var sample = @"{
				""test"":{
					""innerTest"":{
						""value"":10,
						""value2"":22,
						""value3"":{
							""moreValue"":45
						}
					}
				},
				""test2"":20
			}".ToUtf8Bytes();

			//~ val, err := ParseJSON(sample)
			//~ if err != nil {
			//~ 	t.Errorf("Failed to parse: %v", err)
			//~ 	return
			//~ }
			var val = Container.ParseJSON(sample);

			//~ if result, ok := val.Search("test", "innerTest", "value3", "moreValue").Data().(float64); ok {
			//~ 	if result != 45 {
			//~ 		t.Errorf("Wrong value of result: %v", result)
			//~ 	}
			//~ } else {
			//~ 	t.Errorf("Didn't find value")
			//~ }
			var result = Convert.ToDouble(val.Search("test", "innerTest", "value3", "moreValue").Object);
			Assert.IsNotNull(result);
			Assert.AreEqual(45, result);
		}

		[TestMethod]
		//~ func TestShorthand(t *testing.T) {
		public void TestShorthand()
		{
			//~ json, _ := ParseJSON([]byte(`{
			//~ 	"outter":{
			//~ 		"inner":{
			//~ 			"value":5,
			//~ 			"value2":10,
			//~ 			"value3":11
			//~ 		},
			//~ 		"inner2":{
			//~ 		}
			//~ 	},
			//~ 	"outter2":{
			//~ 		"inner":0
			//~ 	}
			//~ }`))
			var json = Container.ParseJSON(@"{
				""outter"":{
					""inner"":{
						""value"":5,
						""value2"":10,
						""value3"":11
					},
					""inner2"":{
					}
				},
				""outter2"":{
					""inner"":0
				}
			}".ToUtf8Bytes());

			//~ missingValue:= json.S("outter").S("doesntexist").S("alsodoesntexist").S("inner").S("value").Data()
			//~ if missingValue != nil {
			//~ 	t.Errorf("missing value was actually found: %v\n", missingValue)
			//~ }
			var missingValue = json.S("outter").S("doesntexist").S("alsodoesntexist").S("inner").S("value").Object;
			Assert.IsNull(missingValue);

			//~ realValue := json.S("outter").S("inner").S("value2").Data().(float64)
			//~ if realValue != 10 {
			//~ 	t.Errorf("real value was incorrect: %v\n", realValue)
			//~ }
			var realValue = Convert.ToDouble(json.S("outter").S("inner").S("value2").Object);
			Assert.AreEqual(10, realValue);

			//~ _, err := json.S("outter2").Set(json.S("outter").S("inner").Data(), "inner")
			//~ if err != nil {
			//~ 	t.Errorf("error setting outter2: %v\n", err)
			//~ }
			json.S("outter2").Set(json.S("outter").S("inner").Object, "inner");

			//~ compare := `{"outter":{"inner":{"value":5,"value2":10,"value3":11},"inner2":{}}` +
			//~ 	`,"outter2":{"inner":{"value":5,"value2":10,"value3":11}}}`
			//~ out := json.String()
			//~ if out != compare {
			//~ 	t.Errorf("wrong serialized structure: %v\n", out)
			//~ }
			var compare = @"{""outter"":{""inner"":{""value"":5,""value2"":10,""value3"":11},""inner2"":{}}"
					+ @",""outter2"":{""inner"":{""value"":5,""value2"":10,""value3"":11}}}";

			//~ compare2 := `{"outter":{"inner":{"value":6,"value2":10,"value3":11},"inner2":{}}` +
			//~ 	`,"outter2":{"inner":{"value":6,"value2":10,"value3":11}}}`
			var compare2 = @"{""outter"":{""inner"":{""value"":6,""value2"":10,""value3"":11},""inner2"":{}}"
					+ @",""outter2"":{""inner"":{""value"":6,""value2"":10,""value3"":11}}}";

			//~ json.S("outter").S("inner").Set(6, "value")
			//~ out = json.String()
			//~ if out != compare2 {
			//~ 	t.Errorf("wrong serialized structure: %v\n", out)
			//~ }
			json.S("outter").S("inner").Set(6, "value");
			var @out = json.ToString();
			Assert.AreEqual(compare2, @out);
		}

		[TestMethod]
		//~ func TestInvalid(t *testing.T) {
		public void TestInvalid()
		{
			//~ invalidJSONSamples := []string{
			//~ 	`{dfads"`,
			//~ 	``,
			//~ 	// `""`,
			//~ 	// `"hello"`,
			//~ 	"{}\n{}",
			//~ }
			var invalidJSONSamples = new string[]
			{
				"{dfads\"",
				"",
				// `""`,
				// `"hello"`,
				"{}\n{}",
			};

			//~ for _, sample := range invalidJSONSamples {
			//~ 	if _, err := ParseJSON([]byte(sample)); err == nil {
			//~ 		t.Errorf("parsing invalid JSON '%v' did not return error", sample)
			//~ 	}
			//~ }
			foreach (var sample in invalidJSONSamples)
			{
				Assert.ThrowsException<Newtonsoft.Json.JsonReaderException>(() =>
					  Container.ParseJSON(sample.ToUtf8Bytes()));
			}

			//~ if _, err := ParseJSON(nil); err == nil {
			//~ 	t.Errorf("parsing nil did not return error")
			//~ }
			Assert.ThrowsException<ArgumentNullException>(() =>
				Container.ParseJSON(null));

			//~ validObj, err := ParseJSON([]byte(`{}`))
			//~ if err != nil {
			//~ 	t.Errorf("failed to parse '{}'")
			//~ }
			var validObj = Container.ParseJSON("{}".ToUtf8Bytes());

			//~ invalidStr := validObj.S("Doesn't exist").String()
			//~ if "{}" != invalidStr {
			//~ 	t.Errorf("expected '{}', received: %v", invalidStr)
			//~ }
			var invalidStr = validObj.S("Doesn't exist").ToString();
			Assert.AreEqual("{}", invalidStr);
		}

		[TestMethod]
		//~ func TestCreation(t *testing.T) {
		public void TestCreation()
		{
			//~ json, _ := ParseJSON([]byte(`{}`))
			//~ inner, err := json.ObjectP("test.inner")
			//~ if err != nil {
			//~ 	t.Errorf("Error: %v", err)
			//~ 	return
			//~ }
			var json = Container.ParseJSON("{}".ToUtf8Bytes());
			var inner = json.ObjectP("test.inner");

			inner.Set(10, "first");
			inner.Set(20, "second");

			inner.Array("array");
			inner.ArrayAppend("first element of the array", "array");
			inner.ArrayAppend(2, "array");
			inner.ArrayAppend("three", "array");

			//~ expected := `{"test":{"inner":{"array":["first element of the array",2,"three"],` +
			//~ 	`"first":10,"second":20}}}`
			var expected = @"{""test"":{""inner"":{""array"":[""first element of the array"",2,""three""],"
					+ @"""first"":10,""second"":20}}}";
			//~ actual := json.String()
			//~ if actual != expected {
			//~ 	t.Errorf("received incorrect output from json object: %v\n", actual)
			//~ }
			var actual = json.ToString();
			Assert.AreEqual(expected, actual);
		}

		//~ type outterJSON struct {
		//~ 	FirstInner  innerJSON
		//~ 	SecondInner innerJSON
		//~ 	ThirdInner  innerJSON
		//~ }
		public class OutterJSON
		{
			public InnerJSON FirstInner
			{ get; set; }

			public InnerJSON SecondInner
			{ get; set; }

			public InnerJSON ThirdInner
			{ get; set; }
		}

		//~ type innerJSON struct {
		//~ 	NumberType float64
		//~ 	StringType string
		//~ }
		public class InnerJSON
		{
			public double NumberType
			{ get; set; }

			public string StringType
			{ get; set; }
		}

		//~ type jsonStructure struct {
		//~ 	FirstOutter  outterJSON
		//~ 	SecondOutter outterJSON
		//~ }
		public class JsonStructure
		{
			public OutterJSON FirstOutter
			{ get; set; }

			public OutterJSON SecondOutter
			{ get; set; }
		}

		//~ var jsonContent = []byte(`{
		//~ 	"firstOutter":{
		//~ 		"firstInner":{
		//~ 			"numberType":11,
		//~ 			"stringType":"hello world, first first"
		//~ 		},
		//~ 		"secondInner":{
		//~ 			"numberType":12,
		//~ 			"stringType":"hello world, first second"
		//~ 		},
		//~ 		"thirdInner":{
		//~ 			"numberType":13,
		//~ 			"stringType":"hello world, first third"
		//~ 		}
		//~ 	},
		//~ 	"secondOutter":{
		//~ 		"firstInner":{
		//~ 			"numberType":21,
		//~ 			"stringType":"hello world, second first"
		//~ 		},
		//~ 		"secondInner":{
		//~ 			"numberType":22,
		//~ 			"stringType":"hello world, second second"
		//~ 		},
		//~ 		"thirdInner":{
		//~ 			"numberType":23,
		//~ 			"stringType":"hello world, second third"
		//~ 		}
		//~ 	}
		//~ }`)
		byte[] JsonContent = @"{
			""firstOutter"":{
				""firstInner"":{
					""numberType"":11,
					""stringType"":""hello world, first first""
				},
				""secondInner"":{
					""numberType"":12,
					""stringType"":""hello world, first second""
				},
				""thirdInner"":{
					""numberType"":13,
					""stringType"":""hello world, first third""
				}
			},
			""secondOutter"":{
				""firstInner"":{
					""numberType"":21,
					""stringType"":""hello world, second first""
				},
				""secondInner"":{
					""numberType"":22,
					""stringType"":""hello world, second second""
				},
				""thirdInner"":{
					""numberType"":23,
					""stringType"":""hello world, second third""
				}
			}
		}".ToUtf8Bytes();

		/*
		Simple use case, compares unmarshalling declared structs vs dynamically searching for
		the equivalent hierarchy. Hopefully we won't see too great a performance drop from the
		dynamic approach.
		*/

		[TestMethod]
		// Poor approximation of:
		//   https://golang.org/pkg/testing/#hdr-Benchmarks
		[DataRow(10)]
		[DataRow(1000)]
		[DataRow(100000)]
		//~ func BenchmarkStatic(b *testing.B) {
		public void BenchmarkStatic(int b_N)
		{
			//~ for i := 0; i < b.N; i++ {
			for (int i = 0; i < b_N; i++)
			{
				//~ var jsonObj jsonStructure
				//~ json.Unmarshal(jsonContent, &jsonObj)
				var jsonObj = JsonConvert.DeserializeObject<JsonStructure>(JsonContent.ToUtf8String());

				//~ if val := jsonObj.FirstOutter.SecondInner.NumberType; val != 12 {
				//~ 	b.Errorf("Wrong value of FirstOutter.SecondInner.NumberType: %v\n", val)
				//~ }
				Assert.AreEqual(12, jsonObj.FirstOutter.SecondInner.NumberType);
				//~ expected := "hello world, first second"
				//~ if val := jsonObj.FirstOutter.SecondInner.StringType; val != expected {
				//~ 	b.Errorf("Wrong value of FirstOutter.SecondInner.StringType: %v\n", val)
				//~ }
				//~ if val := jsonObj.SecondOutter.ThirdInner.NumberType; val != 23 {
				//~ 	b.Errorf("Wrong value of SecondOutter.ThirdInner.NumberType: %v\n", val)
				//~ }
				Assert.AreEqual("hello world, first second", jsonObj.FirstOutter.SecondInner.StringType);
				//~ expected = "hello world, second second"
				//~ if val := jsonObj.SecondOutter.SecondInner.StringType; val != expected {
				//~ 	b.Errorf("Wrong value of SecondOutter.SecondInner.StringType: %v\n", val)
				//~ }
				Assert.AreEqual("hello world, second second", jsonObj.SecondOutter.SecondInner.StringType);
			}
		}

		[TestMethod]
		// Poor approximation of:
		//   https://golang.org/pkg/testing/#hdr-Benchmarks
		[DataRow(10)]
		[DataRow(1000)]
		[DataRow(100000)]
		//~ func BenchmarkDynamic(b *testing.B) {
		public void BenchmarkDynamic(int b_N)
		{
			//~ for i := 0; i < b.N; i++ {
			for (int i = 0; i < b_N; i++)
			{
				//~ jsonObj, err := ParseJSON(jsonContent)
				//~ if err != nil {
				//~ 	b.Errorf("Error parsing json: %v\n", err)
				//~ }
				var jsonObj = Container.ParseJSON(JsonContent);

				//~ FOSI := jsonObj.S("firstOutter", "secondInner")
				//~ SOSI := jsonObj.S("secondOutter", "secondInner")
				//~ SOTI := jsonObj.S("secondOutter", "thirdInner")
				var FOSI = jsonObj.S("firstOutter", "secondInner");
				var SOSI = jsonObj.S("secondOutter", "secondInner");
				var SOTI = jsonObj.S("secondOutter", "thirdInner");

				//~ if val := FOSI.S("numberType").Data().(float64); val != 12 {
				//~ 	b.Errorf("Wrong value of FirstOutter.SecondInner.NumberType: %v\n", val)
				//~ }
				Assert.AreEqual(12, Convert.ToDouble(FOSI.S("numberType").Object));
				//~ expected := "hello world, first second"
				//~ if val := FOSI.S("stringType").Data().(string); val != expected {
				//~ 	b.Errorf("Wrong value of FirstOutter.SecondInner.StringType: %v\n", val)
				//~ }
				Assert.AreEqual("hello world, first second", FOSI.S("stringType").Object);
				//~ if val := SOTI.S("numberType").Data().(float64); val != 23 {
				//~ 	b.Errorf("Wrong value of SecondOutter.ThirdInner.NumberType: %v\n", val)
				//~ }
				Assert.AreEqual(23, Convert.ToDouble(SOTI.S("numberType").Object));
				//~ expected = "hello world, second second"
				//~ if val := SOSI.S("stringType").Data().(string); val != expected {
				//~ 	b.Errorf("Wrong value of SecondOutter.SecondInner.StringType: %v\n", val)
				//~ }
				Assert.AreEqual("hello world, second second", SOSI.S("stringType").Object);
			}
		}

		[TestMethod]
		//~ func TestNoTypeChildren(t *testing.T) {
		public void TestNoTypeChildren()
		{
			//~ jsonObj, err := ParseJSON([]byte(`{"not_obj_or_array":1}`))
			//~ if err != nil {
			//~ 	t.Error(err)
			//~ }
			var jsonObj = Container.ParseJSON(@"{""not_obj_or_array"":1}".ToUtf8Bytes());
			//~ exp := ErrNotObjOrArray
			//~ if _, act := jsonObj.S("not_obj_or_array").Children(); act != exp {
			//~ 	t.Errorf("Unexpected value returned: %v != %v", exp, act)
			//~ }
			Assert.ThrowsException<Container.ErrNotObjOrArray>(() =>
				jsonObj.S("not_obj_or_array").Children());
			//~ exp = ErrNotObj
			//~ if _, act := jsonObj.S("not_obj_or_array").ChildrenMap(); act != exp {
			//~ 	t.Errorf("Unexpected value returned: %v != %v", exp, act)
			//~ }
			Assert.ThrowsException<Container.ErrNotObj>(() =>
				jsonObj.S("not_obj_or_array").ChildrenMap());
		}

		[TestMethod]
		//~ func TestBadIndexes(t *testing.T) {
		public void TestBadIndexes()
		{
			//~ jsonObj, err := ParseJSON([]byte(`{"array":[1,2,3]}`))
			//~ if err != nil {
			//~ 	t.Error(err)
			//~ }
			var jsonObj = Container.ParseJSON(@"{""array"":[1,2,3]}".ToUtf8Bytes());
			//~ if act := jsonObj.Index(0).Data(); nil != act {
			//~ 	t.Errorf("Unexpected value returned: %v != %v", nil, act)
			//~ }
			Assert.IsNull(jsonObj.Index(0).Object);
			//~ if act := jsonObj.S("array").Index(4).Data(); nil != act {
			//~ 	t.Errorf("Unexpected value returned: %v != %v", nil, act)
			//~ }
			Assert.IsNull(jsonObj.S("array").Index(4).Object);
		}

		[TestMethod]
		//~ func TestNilSet(t *testing.T) {
		public void TestNilSet()
		{
			//~ obj := Container{nil}
			//~ if _, err := obj.Set("bar", "foo"); err != nil {
			//~ 	t.Error(err)
			//~ }
			var obj = new Container(null);
			//~ if _, err := obj.Set("new", "foo", "bar"); err != ErrPathCollision {
			//~ 	t.Errorf("Expected ErrPathCollision: %v, %s", err, obj.Data())
			//~ }
			Assert.ThrowsException<Container.ErrPathCollision>(() =>
				obj.Search("new", "foo", "bar"));
			//~ if _, err := obj.SetIndex("new", 0); err != ErrNotArray {
			//~ 	t.Errorf("Expected ErrNotArray: %v, %s", err, obj.Data())
			//~ }
			Assert.ThrowsException<Container.ErrNotArray>(() =>
				obj.SetIndex("new", 0));
		}
	}
}
