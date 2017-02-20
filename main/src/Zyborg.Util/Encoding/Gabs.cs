using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Zyborg.Util.JSON;
using System.Collections;
using System.Collections.Specialized;
using Zyborg.Util.Collections;

namespace Zyborg.Util.Encoding
{
	// Container - an internal structure that holds a reference to the core interface map of the parsed
	// json. Use this container to move context.
	//~ type Container struct {
	//~ 	object interface{}
	//~ }
	public class Container
	{

		//var (
		//	// ErrOutOfBounds - Index out of bounds.
		//	ErrOutOfBounds = errors.New("out of bounds")
		public class ErrOutOfBounds : Exception
		{
			public ErrOutOfBounds() : base("out of bounds")
			{ }
		}

		//	// ErrNotObjOrArray - The target is not an object or array type.
		//	ErrNotObjOrArray = errors.New("not an object or array")
		public class ErrNotObjOrArray : Exception
		{
			public ErrNotObjOrArray() : base("not an object or array")
			{ }
		}

		//	// ErrNotObj - The target is not an object type.
		//	ErrNotObj = errors.New("not an object")
		public class ErrNotObj : Exception
		{
			public ErrNotObj() : base("not an object")
			{ }
		}

		//	// ErrNotArray - The target is not an array type.
		//	ErrNotArray = errors.New("not an array")
		public class ErrNotArray : Exception
		{
			public ErrNotArray() : base("not an array")
			{ }
		}

		//	// ErrPathCollision - Creating a path failed because an element collided with an existing value.
		//	ErrPathCollision = errors.New("encountered value collision whilst building path")
		public class ErrPathCollision : Exception
		{
			public ErrPathCollision() : base("encountered value collision whilst building path")
			{ }
		}

		//	// ErrInvalidInputObj - The input value was not a map[string]interface{}.
		//	ErrInvalidInputObj = errors.New("invalid input object")
		public class ErrInvalidInputObj : Exception
		{
			public ErrInvalidInputObj() : base("invalid input object")
			{ }
		}

		//	// ErrInvalidInputText - The input data could not be parsed.
		//	ErrInvalidInputText = errors.New("input text could not be parsed")
		public class ErrInvalidInputText : Exception
		{
			public ErrInvalidInputText() : base("input text could not be parsed")
			{ }
		}

		//	// ErrInvalidPath - The filepath was not valid.
		//	ErrInvalidPath = errors.New("invalid file path")
		public class ErrInvalidPath : Exception
		{
			public ErrInvalidPath() : base("invalid file path")
			{ }
		}

		//	// ErrInvalidBuffer - The input buffer contained an invalid JSON string
		//	ErrInvalidBuffer = errors.New("input buffer contained invalid JSON")
		//)
		public class ErrInvalidBuffer : Exception
		{
			public ErrInvalidBuffer() : base("input buffer contained invalid JSON")
			{ }
		}

		//--------------------------------------------------------------------------------------------------

		public Container()
		{ }

		public Container(object obj)
		{
			Object = obj;
		}

		public object Object
		{ get; private set; }

		// Data - Return the contained data as an interface{}.
		//~ func (g *Container) Data() interface{} {
		// IMPL:  don't need this, just access the Object

		//--------------------------------------------------------------------------------------------------

		// Path - Search for a value using dot notation.
		//~ func (g *Container) Path(path string) *Container {
		//~ 	return g.Search(strings.Split(path, ".")...)
		//~ }
		public Container Path(string path)
		{
			return Search(path.Split('.'));
		}

		// Search - Attempt to find and return an object within the JSON structure by specifying the
		// hierarchy of field names to locate the target. If the search encounters an array and has not
		// reached the end target then it will iterate each object of the array for the target and return
		// all of the results in a JSON array.
		//~ func (g *Container) Search(hierarchy ...string) *Container {
		public Container Search(params string[] hierarchy)
		{
			//~ var object interface{}
			//~ 
			//~ object = g.object
			object obj = this.Object;

			//~for target := 0; target < len(hierarchy); target++ {
			for (var target = 0; target < hierarchy.Length; target++)
			{
				//~if mmap, ok := object.(map[string]interface{}); ok {
				var nmap = obj as IDictionary<string, object>;
				if (nmap != null)
				{
					//~ object = mmap[hierarchy[target]]
					nmap.TryGetValue(hierarchy[target], out obj);
				}
				//~ } else if marray, ok := object.([]interface{}); ok {
				else if (obj is IList<object>)
				{
					var marray = (IList<object>)obj;
					//~ tmpArray := []interface{}{}
					var tmpArray = new List<object>();
					//~ for _, val := range marray {
					foreach (var val in marray)
					{
						//~ tmpGabs := &Container{val}
						//~ res := tmpGabs.Search(hierarchy[target:]...).Data()
						//~ if res != nil {
						//~ 	tmpArray = append(tmpArray, res)
						//~ }
						var tmpGabs = new Container(val);
						var res = tmpGabs.Search(hierarchy.Skip(target).ToArray()).Object;
						if (res != null)
							tmpArray.Add(res);
					}
					//~ if len(tmpArray) == 0 {
					//~ 	return &Container{nil}
					//~ }
					if (tmpArray.Count == 0)
						return new Container();

					//~ return &Container{tmpArray}
					return new Container(tmpArray);
				}
				else
				{
					//~ return &Container{nil}
					return new Container();
				}
			}
			//~ return &Container{object}
			return new Container(obj);
		}

		// S - Shorthand method, does the same thing as Search.
		//~ func (g *Container) S(hierarchy ...string) *Container {
		public Container S(params string[] hierarchy)
		{
			//~ return g.Search(hierarchy...)
			return Search(hierarchy);
		}

		// Exists - Checks whether a path exists.
		//~ func (g *Container) Exists(hierarchy ...string) bool {
		public bool Exists(params string[] hierarchy)
		{
			//~ return g.Search(hierarchy...).Data() != nil
			return Search(hierarchy).Object != null;
		}

		// ExistsP - Checks whether a dot notation path exists.
		//~ func (g *Container) ExistsP(path string) bool {
		public bool ExistsP(string path)
		{
			//~ return g.Exists(strings.Split(path, ".")...)
			return Exists(path.Split('.'));
		}

		// Index - Attempt to find and return an object within a JSON array by index.
		//~ func (g *Container) Index(index int) *Container {
		public Container Index(int index)
		{
			//~ if array, ok := g.Data().([]interface{}); ok {
			//~ 	if index >= len(array) {
			//~ 		return &Container{nil}
			//~ 	}
			//~ 	return &Container{array[index]}
			//~ }
			//~ return &Container{nil}
			var array = Object as IList<object>;
			if (array != null)
			{
				if (index >= array.Count)
					return new Container();
				return new Container(array[index]);
			}
			return new Container();
		}

		// Children - Return a slice of all the children of the array. This also works for objects, however,
		// the children returned for an object will NOT be in order and you lose the names of the returned
		// objects this way.
		//~ func (g *Container) Children() ([]*Container, error) {
		public Container[] Children()
		{
			//~ if array, ok := g.Data().([]interface{}); ok {
			//~ 	children := make([]*Container, len(array))
			//~ 	for i := 0; i < len(array); i++ {
			//~ 		children[i] = &Container{array[i]}
			//~ 	}
			//~ 	return children, nil
			//~ }
			var array = Object as IList<object>;
			if (array != null)
			{
				var children = new Container[array.Count];
				for (var i = 0; i < array.Count; i++)
					children[i] = new Container(array[i]);
				return children;
			}

			//~ if mmap, ok := g.Data().(map[string]interface{}); ok {
			//~ 	children := []*Container{}
			//~ 	for _, obj := range mmap {
			//~ 		children = append(children, &Container{obj})
			//~ 	}
			//~ 	return children, nil
			//~ }
			//~ return nil, ErrNotObjOrArray
			var nmap = Object as IDictionary<string, object>;
			if (nmap != null)
			{
				var children = new List<Container>();
				foreach (var obj in nmap.Values)
					children.Add(new Encoding.Container(obj));
				return children.ToArray();
			}
			throw new ErrNotObjOrArray();
		}

		// ChildrenMap - Return a map of all the children of an object.
		//~ func (g *Container) ChildrenMap() (map[string]*Container, error) {
		public IDictionary<string, Container> ChildrenMap()
		{
			//~ if mmap, ok := g.Data().(map[string]interface{}); ok {
			var nmap = Object as IDictionary<string, object>;
			if (nmap != null)
			{
				//~ children := map[string]*Container{}
				//~ for name, obj := range mmap {
				//~ 	children[name] = &Container{obj}
				//~ }
				//~ return children, nil
				var children = new SortedDictionary<string, Container>();
				foreach (var kv in nmap)
					children[kv.Key] = new Container(kv.Value);
				return children;
			}
			//~ return nil, ErrNotObj
			throw new ErrNotObj();
		}

		//--------------------------------------------------------------------------------------------------

		// Set - Set the value of a field at a JSON path, any parts of the path that do not exist will be
		// constructed, and if a collision occurs with a non object type whilst iterating the path an error
		// is returned.
		//~ func (g *Container) Set(value interface{}, path ...string) (*Container, error) {
		public Container Set(object value, params string[] path)
		{
			//~ if len(path) == 0 {
			//~ 	g.object = value
			//~ 	return g, nil
			//~ }
			if (path.Length == 0)
			{
				this.Object = value;
				return this;
			}
			//~ var object interface{}
			//~ if g.object == nil {
			//~ 	g.object = map[string]interface{}{}
			//~ }
			//~ object = g.object
			if (this.Object == null)
				this.Object = new SortedDictionary<string, object>();
			var obj = this.Object;

			//~ for target := 0; target < len(path); target++ {
			for (var target = 0; target < path.Length; target++)
			{
				//~ if mmap, ok := object.(map[string]interface{}); ok {
				//~ 	if target == len(path)-1 {
				//~ 		mmap[path[target]] = value
				//~ 	} else if mmap[path[target]] == nil {
				//~ 		mmap[path[target]] = map[string]interface{}{}
				//~ 	}
				//~ 	object = mmap[path[target]]
				//~ } else {
				//~ 	return &Container{nil}, ErrPathCollision
				//~ }
				var nmap = obj as IDictionary<string, object>;
				if (nmap != null)
				{
					if (target == path.Length - 1)
						nmap[path[target]] = value;
					else if (!nmap.ContainsKey(path[target]))
						nmap[path[target]] = new SortedDictionary<string, object>();

					obj = nmap[path[target]];
				}
				else
					throw new ErrPathCollision();
			}
			//~ return &Container{object}, nil
			return new Container(obj);
		}

		// SetP - Does the same as Set, but using a dot notation JSON path.
		//~ func (g *Container) SetP(value interface{}, path string) (*Container, error) {
		public Container SetP(object value, string path)
		{
			//~ return g.Set(value, strings.Split(path, ".")...)
			return Set(value, path.Split('.'));
		}

		//~ // SetIndex - Set a value of an array element based on the index.
		//~ func (g *Container) SetIndex(value interface{}, index int) (*Container, error) {
		public Container SetIndex(object value, int index)
		{
			//~ if array, ok := g.Data().([]interface{}); ok {
			//~ 	if index >= len(array) {
			//~ 		return &Container{nil}, ErrOutOfBounds
			//~ 	}
			//~ 	array[index] = value
			//~ 	return &Container{array[index]}, nil
			//~ }
			//~ return &Container{nil}, ErrNotArray
			var array = this.Object as IList<object>;
			if (array != null)
			{
				if (index >= array.Count)
					throw new ErrOutOfBounds();
				array[index] = value;
				return new Container(array[index]);
			}
			throw new ErrNotArray();
		}

		// Object - Create a new JSON object at a path. Returns an error if the path contains a collision
		// with a non object type.
		//~ func (g *Container) Object(path ...string) (*Container, error) {
		public Container ObjectJ(params string[] path)
		{
			//~ return g.Set(map[string]interface{}{}, path...)
			return Set(new SortedDictionary<string, object>(), path);
		}

		// ObjectP - Does the same as Object, but using a dot notation JSON path.
		//~ func (g *Container) ObjectP(path string) (*Container, error) {
		public Container ObjectP(string path)
		{
			//return g.Object(strings.Split(path, ".")...)
			return ObjectJ(path.Split('.'));
		}

		// ObjectI - Create a new JSON object at an array index. Returns an error if the object is not an
		// array or the index is out of bounds.
		//~ func (g *Container) ObjectI(index int) (*Container, error) {
		public Container ObjectI(int index)
		{
			//~ return g.SetIndex(map[string]interface{}{}, index)
			return SetIndex(new SortedDictionary<string, object>(), index);
		}

		// Array - Create a new JSON array at a path. Returns an error if the path contains a collision with
		// a non object type.
		//~ func (g *Container) Array(path ...string) (*Container, error) {
		public Container Array(params string[] path)
		{
			//~return g.Set([]interface{}{}, path...)
			return Set(new List<object>(), path);
		}

		// ArrayP - Does the same as Array, but using a dot notation JSON path.
		//~ func (g *Container) ArrayP(path string) (*Container, error) {
		public Container ArrayP(string path)
		{
			//~ return g.Array(strings.Split(path, ".")...)
			return Array(path.Split('.'));
		}

		// ArrayI - Create a new JSON array at an array index. Returns an error if the object is not an
		// array or the index is out of bounds.
		//~ func (g *Container) ArrayI(index int) (*Container, error) {
		public Container ArrayI(int index)
		{
			//~ return g.SetIndex([]interface{}{}, index)
			return SetIndex(new List<object>(), index);
		}

		// ArrayOfSize - Create a new JSON array of a particular size at a path. Returns an error if the
		// path contains a collision with a non object type.
		//~ func (g *Container) ArrayOfSize(size int, path ...string) (*Container, error) {
		public Container ArrayOfSize(int size, params string[] path)
		{
			//~ a := make([]interface{}, size)
			//~ return g.Set(a, path...)
			return Set(new List<object>(new object[size]), path);
		}

		// ArrayOfSizeP - Does the same as ArrayOfSize, but using a dot notation JSON path.
		//~ func (g *Container) ArrayOfSizeP(size int, path string) (*Container, error) {
		public Container ArrayOfSizeP(int size, string path)
		{
			//~ return g.ArrayOfSize(size, strings.Split(path, ".")...)
			return ArrayOfSize(size, path.Split('.'));
		}

		// ArrayOfSizeI - Create a new JSON array of a particular size at an array index. Returns an error
		// if the object is not an array or the index is out of bounds.
		//~ func (g *Container) ArrayOfSizeI(size, index int) (*Container, error) {
		public Container ArrayOfSizeI(int size, int index)
		{
			//~ a := make([]interface{}, size)
			//~ return g.SetIndex(a, index)
			return SetIndex(new List<object>(new object[size]), index);
		}

		// Delete - Delete an element at a JSON path, an error is returned if the element does not exist.
		//~ func (g *Container) Delete(path ...string) error {
		public void Delete(params string[] path)
		{
			//~ var object interface{}
			//~ 
			//~ if g.object == nil {
			//~ 	return ErrNotObj
			//~ }
			//~ object = g.object
			if (this.Object == null)
				throw new ErrNotObj();
			var obj = this.Object;

			//~ for target := 0; target < len(path); target++ {
			for (var target = 0; target < path.Length; target++)
			{
				//~ if mmap, ok := object.(map[string]interface{}); ok {
				var nmap = obj as IDictionary<string, object>;
				if (nmap != null)
				{
					//~ if target == len(path)-1 {
					//~ 	if _, ok := mmap[path[target]]; ok {
					//~ 		delete(mmap, path[target])
					//~ 	} else {
					//~ 		return ErrNotObj
					//~ 	}
					//~ }
					//~ object = mmap[path[target]]
					if (target == path.Length - 1)
					{
						if (nmap.ContainsKey(path[target]))
							nmap.Remove(path[target]);
						else
							throw new ErrNotObj();
					}
					nmap.TryGetValue(path[target], out obj);
				}
				else
				{
					//~ return ErrNotObj
					throw new ErrNotObj();
				}
			}
			//~ return nil
		}

		// DeleteP - Does the same as Delete, but using a dot notation JSON path.
		//~ func (g *Container) DeleteP(path string) error {
		public void DeleteP(string path)
		{
			//~ return g.Delete(strings.Split(path, ".")...)
			this.Delete(path.Split('.'));
		}

		//--------------------------------------------------------------------------------------------------

		/*
		Array modification/search - Keeping these options simple right now, no need for anything more
		complicated since you can just cast to []interface{}, modify and then reassign with Set.
		*/

		// ArrayAppend - Append a value onto a JSON array.
		//~ func (g *Container) ArrayAppend(value interface{}, path ...string) error {
		public void ArrayAppend(object value, params string[] path)
		{
			//~ array, ok := g.Search(path...).Data().([]interface{})
			//~ if !ok {
			//~ 	return ErrNotArray
			//~ }
			//~ array = append(array, value)
			//~ _, err := g.Set(array, path...)
			//~ return err
			var array = Search(path).Object as IList<object>;
			if (array == null)
				throw new ErrNotArray();
			array.Add(value);
			Set(array, path);
		}

		// ArrayAppendP - Append a value onto a JSON array using a dot notation JSON path.
		//~ func (g *Container) ArrayAppendP(value interface{}, path string) error {
		public void ArrayAppendP(object value, string path)
		{
			//~ return g.ArrayAppend(value, strings.Split(path, ".")...)
			ArrayAppend(value, path.Split('.'));
		}

		// ArrayRemove - Remove an element from a JSON array.
		//~ func (g *Container) ArrayRemove(index int, path ...string) error {
		public void ArrayRemove(int index, params string[] path)
		{
			//~ if index < 0 {
			//~ 	return ErrOutOfBounds
			//~ }
			if (index < 0)
				throw new ArgumentOutOfRangeException(nameof(index));

			//~ array, ok := g.Search(path...).Data().([]interface{})
			//~ if !ok {
			//~ 	return ErrNotArray
			//~ }
			var array = Search(path).Object as IList<object>;
			if (array == null)
				throw new ErrNotArray();

			//~ if index < len(array) {
			//~ 	array = append(array[:index], array[index+1:]...)
			//~ } else {
			//~ 	return ErrOutOfBounds
			//~ }
			if (index < array.Count)
				array.RemoveAt(index);
			else
				throw new ErrOutOfBounds();

			//~ _, err := g.Set(array, path...)
			//~ return err
			Set(array, path);
		}

		// ArrayRemoveP - Remove an element from a JSON array using a dot notation JSON path.
		//~ func (g *Container) ArrayRemoveP(index int, path string) error {
		public void ArrayRemoveP(int index, string path)
		{
			//~ return g.ArrayRemove(index, strings.Split(path, ".")...)
			ArrayRemove(index, path.Split('.'));
		}

		// ArrayElement - Access an element from a JSON array.
		//~ func (g *Container) ArrayElement(index int, path ...string) (*Container, error) {
		public Container ArrayElement(int index, params string[] path)
		{
			//~ if index < 0 {
			//~ 	return &Container{nil}, ErrOutOfBounds
			//~ }
			if (index < 0)
				throw new ArgumentOutOfRangeException(nameof(index));

			//~ array, ok := g.Search(path...).Data().([]interface{})
			//~ if !ok {
			//~ 	return &Container{nil}, ErrNotArray
			//~ }
			var array = Search(path).Object as IList<object>;
			if (array == null)
				throw new ErrNotArray();

			//~ if index < len(array) {
			//~ 	return &Container{array[index]}, nil
			//~ }
			if (index < array.Count)
				return new Container(array[index]);

			//~ return &Container{nil}, ErrOutOfBounds
			throw new ErrOutOfBounds();
		}

		// ArrayElementP - Access an element from a JSON array using a dot notation JSON path.
		//~ func (g *Container) ArrayElementP(index int, path string) (*Container, error) {
		public Container ArrayElementP(int index, string path)
		{
			//~ return g.ArrayElement(index, strings.Split(path, ".")...)
			return ArrayElement(index, path.Split('.'));
		}

		// ArrayCount - Count the number of elements in a JSON array.
		//~ func (g *Container) ArrayCount(path ...string) (int, error) {
		public int ArrayCount(params string[] path)
		{
			//~ if array, ok := g.Search(path...).Data().([]interface{}); ok {
			//~ 	return len(array), nil
			//~ }
			var array = Search(path).Object as IList<object>;
			if (array != null)
				return array.Count;

			//~ return 0, ErrNotArray
			throw new ErrNotArray();
		}

		// ArrayCountP - Count the number of elements in a JSON array using a dot notation JSON path.
		//~ func (g *Container) ArrayCountP(path string) (int, error) {
		public int ArrayCountP(string path)
		{
			//~ return g.ArrayCount(strings.Split(path, ".")...)
			return ArrayCount(path.Split('.'));
		}

		//--------------------------------------------------------------------------------------------------

		// Bytes - Converts the contained object back to a JSON []byte blob.
		//~ func (g *Container) Bytes() []byte {
		public byte[] Bytes()
		{
			//~ if g.object != nil {
			//~ 	if bytes, err := json.Marshal(g.object); err == nil {
			//~ 		return bytes
			//~ 	}
			//~ }
			if (Object != null)
				return JsonConvert.SerializeObject(Object).ToUtf8Bytes();

			//~ return []byte("{}")
			return "{}".ToUtf8Bytes();
		}

		// BytesIndent - Converts the contained object to a JSON []byte blob formatted with prefix, indent.
		//~ func (g *Container) BytesIndent(prefix string, indent string) []byte {
		public byte[] BytesIndent(string prefix, string indent)
		{
			//~ if g.object != nil {
			//~ 	if bytes, err := json.MarshalIndent(g.object, prefix, indent); err == nil {
			//~ 		return bytes
			//~ 	}
			//~ }
			if (Object != null)
				return JsonHelper.SerializeObject(Object, prefix, indent).ToUtf8Bytes();

			//~ return []byte("{}")
			return "{}".ToUtf8Bytes();
		}

		// String - Converts the contained object to a JSON formatted string.
		//~ func (g *Container) String() string {
		public override string ToString()
		{
			//~ return string (g.Bytes())
			return this.Bytes().ToUtf8String();
		}

		// StringIndent - Converts the contained object back to a JSON formatted string with prefix, indent.
		//~ func (g *Container) StringIndent(prefix string, indent string) string {
		public string ToStringIndented(string prefix, string indent)
		{
			//~ return string(g.BytesIndent(prefix, indent))
			return BytesIndent(prefix, indent).ToUtf8String();
		}

		// New - Create a new gabs JSON object.
		//~ func New() *Container {
		public static Container New()
		{
			//~ return &Container{map[string]interface{}{}}
			return new Container(new SortedDictionary<string, object>());
		}

		// Consume - Gobble up an already converted JSON object, or a fresh map[string]interface{} object.
		//~ func Consume(root interface{}) (*Container, error) {
		public static Container Consume(object root)
		{
			//~ return &Container{root}, nil
			return new Container(root);
		}

		// ParseJSON - Convert a string into a representation of the parsed JSON.
		//~ func ParseJSON(sample []byte) (*Container, error) {
		public static Container ParseJSON(byte[] sample)
		{
			//~ var gabs Container
			//~ 
			//~ if err := json.Unmarshal(sample, &gabs.object); err != nil {
			//~ 	return nil, err
			//~ }
			//~ 
			//~ return &gabs, nil
			return new Container(JsonLinqHelper.Deserialize(sample.ToUtf8String()));
			//return new Container(JsonConvert.DeserializeObject(sample.ToUtf8String()));
		}

		// ParseJSONDecoder - Convert a json.Decoder into a representation of the parsed JSON.
		//~ func ParseJSONDecoder(decoder *json.Decoder) (*Container, error) {
		public static Container ParseJSONDecoderXXX(JsonSerializer decoder)
		{
			//~ var gabs Container
			//~ 
			//~ if err := decoder.Decode(&gabs.object); err != nil {
			//~ 	return nil, err
			//~ }
			//~ 
			//~ return &gabs, nil

			// TODO: we'll have to see if we need this...
			throw new NotImplementedException();
		}

		// ParseJSONFile - Read a file and convert into a representation of the parsed JSON.
		//~ func ParseJSONFile(path string) (*Container, error) {
		public static Container ParseJSONFile(string path)
		{
			//~ if len(path) > 0 {
			if (!string.IsNullOrEmpty(path))
			{
				//~ cBytes, err := ioutil.ReadFile(path)
				//~ if err != nil {
				//~ 	return nil, err
				//~ }
				var cBytes = File.ReadAllBytes(path);

				//~ container, err := ParseJSON(cBytes)
				//~ if err != nil {
				//~ 	return nil, err
				//~ }
				//~ 
				//~ return container, nil
				return ParseJSON(cBytes);
			}
			//~return nil, ErrInvalidPath
			throw new ErrInvalidPath();
		}

		// ParseJSONBuffer - Read the contents of a buffer into a representation of the parsed JSON.
		//~ func ParseJSONBuffer(buffer io.Reader) (*Container, error) {
		public static Container ParseJSONBuffer(Stream s)
		{
			//~ var gabs Container
			//~ jsonDecoder := json.NewDecoder(buffer)
			//~ if err := jsonDecoder.Decode(&gabs.object); err != nil {
			//~ 	return nil, err
			//~ }
			//~ 
			//~ return &gabs, nil
			using (var ms = new MemoryStream())
			{
				s.CopyTo(ms);
				return ParseJSON(ms.ToArray());
			}
		}

		// From:
		//  http://stackoverflow.com/a/19140420/5428506
		public static class JsonLinqHelper
		{
			public static object Deserialize(string json)
			{
				return ToObject(JToken.Parse(json));
			}

			private static object ToObject(JToken token)
			{
				switch (token.Type)
				{
					case JTokenType.Object:
						return ToSortedDictionary(token.Children<JProperty>());

					case JTokenType.Array:
						//return new List<object>(token.Select(ToObject));
						return token.Select(ToObject).ToList();

					default:
						return ((JValue)token).Value;
				}
			}

			private static IDictionary<string, object> ToOrderedDictionary(IEnumerable<JProperty> en)
			{
				// Original, unordered
				//return en.ToDictionary(
				//		prop => prop.Name,
				//		prop => ToObject(prop.Value));

				var d = new SortedDictionary<string, object>();
				foreach (var p in en)
					d.Add(p.Name, ToObject(p.Value));
				return d;
			}

			private static IDictionary<string, object> ToSortedDictionary(IEnumerable<JProperty> en)
			{
				var d = new SortedDictionary<string, object>();
				foreach (var p in en)
					d.Add(p.Name, ToObject(p.Value));
				return d;
			}
		}
	}
}
