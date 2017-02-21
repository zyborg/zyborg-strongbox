using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Zyborg.Util;
using Zyborg.Vault.Helper.Salt;
using static Zyborg.Util.ReflectWalk;

namespace Zyborg.Vault.Audit
{
	// HashCallback is the callback called for HashStructure to hash
	// a value.
	//~ type HashCallback func(string) string
	public delegate string HashCallback(string s);

	public static class HashStructure
    {

		// HashString hashes the given opaque string and returns it
		//~ func HashString(salter *salt.Salt, data string) string {
		public static string HashString(Salt salter, string data)
		{
			//~ return salter.GetIdentifiedHMAC(data)
			return salter.GetIdentifiedHMAC(data);
		}

		// Hash will hash the given type. This has built-in support for auth,
		// requests, and responses. If it is a type that isn't recognized, then
		// it will be passed through.
		//
		// The structure is modified in-place.
		//~ func Hash(salter *salt.Salt, raw interface{}) error {
		public static void Hash(Salt salter, object raw)
		{
			HashCallback fn = salter.GetIdentifiedHMAC;

			//~ switch s := raw.(type) {
			switch (raw)
			{
				//~ case *logical.Auth:
				//~ 	if s == nil {
				//~ 		return nil
				//~ 	}
				//~ 	if s.ClientToken != "" {
				//~ 		s.ClientToken = fn(s.ClientToken)
				//~ 	}
				//~ 	if s.Accessor != "" {
				//~ 		s.Accessor = fn(s.Accessor)
				//~ 	}
				case Logical.Auth s:
					if (s == null)
						return;
					if (!string.IsNullOrEmpty(s.ClientToken))
						s.ClientToken = fn(s.ClientToken);
					if (!string.IsNullOrEmpty(s.Accessor))
						s.Accessor = fn(s.Accessor);
					break;

				//~ case *logical.Request:
				case Logical.Request s:
					//~ if s == nil {
					//~ 	return nil
					//~ }
					//~ if s.Auth != nil {
					//~ 	if err := Hash(salter, s.Auth); err != nil {
					//~ 		return err
					//~ 	}
					//~ }
					if (s == null)
						return;
					if (s.Auth != null)
						Hash(salter, s.Auth);

					//~ if s.ClientToken != "" {
					//~ 	s.ClientToken = fn(s.ClientToken)
					//~ }
					if (!string.IsNullOrEmpty(s.ClientToken))
						s.ClientToken = fn(s.ClientToken);

					//~ if s.ClientTokenAccessor != "" {
					//~ 	s.ClientTokenAccessor = fn(s.ClientTokenAccessor)
					//~ }
					if (!string.IsNullOrEmpty(s.ClientTokenAccessor))
						s.ClientTokenAccessor = fn(s.ClientTokenAccessor);

					//~ data, err := HashStructure(s.Data, fn)
					//~ if err != nil {
					//~ 	return err
					//~ }
					//~
					//~ s.Data = data.(map[string]interface{})
					s.Data = Hash(s.Data, fn);
					break;

				//~ case *logical.Response:
				case Logical.Response s:
					//~ if s == nil {
					//~ 	return nil
					//~ }
					if (s == null)
						return;

					//~ if s.Auth != nil {
					//~ 	if err := Hash(salter, s.Auth); err != nil {
					//~ 		return err
					//~ 	}
					//~ }
					if (s.Auth != null)
						Hash(salter, s.Auth);

					//~ if s.WrapInfo != nil {
					//~ 	if err := Hash(salter, s.WrapInfo); err != nil {
					//~ 		return err
					//~ 	}
					//~ }
					if (s.WrapInfo != null)
						Hash(salter, s.WrapInfo);

					//~ data, err := HashStructure(s.Data, fn)
					//~ if err != nil {
					//~ 	return err
					//~ }
					//~
					//~ s.Data = data.(map[string]interface{})
					s.Data = Hash(s.Data, fn);
					break;

				//~ case *logical.ResponseWrapInfo:
				case Logical.ResponseWrapInfo s:
					//~ if s == nil {
					//~ 	return nil
					//~ }
					//~ 
					//~ s.Token = fn(s.Token)
					//~ 
					//~ if s.WrappedAccessor != "" {
					//~ 	s.WrappedAccessor = fn(s.WrappedAccessor)
					//~ }
					if (s == null)
						return;
					s.Token = fn(s.Token);
					if (!string.IsNullOrEmpty(s.WrappedAccessor))
						s.WrappedAccessor = fn(s.WrappedAccessor);
					break;
			}

			//~ return nil
		}

		// HashStructure takes an interface and hashes all the values within
		// the structure. Only _values_ are hashed: keys of objects are not.
		//
		// For the HashCallback, see the built-in HashCallbacks below.
		//~ func HashStructure(s interface{}, cb HashCallback) (interface{}, error) {
		public static T Hash<T>(T s, HashCallback cb)
		{
			//~ s, err:= copystructure.Copy(s)
			//~ if err != nil {
			//~ 	return nil, err
			//~ }

			switch (s)
			{
				case IDictionary<string, object> d:
					s = (T)Activator.CreateInstance(typeof(Dictionary<string, object>), d);
					break;

				default:
					throw new NotSupportedException($"unhandled structure type [{typeof(T).FullName}]");
			}

			//~ walker := &hashWalker{Callback: cb}
			//~ if err := reflectwalk.Walk(s, walker); err != nil {
			//~ 	return nil, err
			//~ }
			var walker = new HashWalker { Callback = cb, };
			ReflectWalk.Walk(s, walker);

			return s;
		}
	}

	// hashWalker implements interfaces for the reflectwalk package
	// (github.com/mitchellh/reflectwalk) that can be used to automatically
	// replace primitives with a hashed value.
	//type hashWalker struct {
	public class HashWalker : ReflectWalk.IWalker
			, ReflectWalk.IEnterExitWalker
			, ReflectWalk.IMapWalker
			, ReflectWalk.ISliceWalker
			, ReflectWalk.IPrimitiveWalker
	{
		// Callback is the function to call with the primitive that is
		// to be hashed. If there is an error, walking will be halted
		// immediately and the error returned.
		//~ Callback HashCallback
		public HashCallback Callback
		{ get; set; }

		//~ key         []string
		//~ lastValue   reflect.Value
		//~ loc         reflectwalk.Location
		//~ cs          []reflect.Value
		//~ csKey       []reflect.Value
		//~ csData      interface{}
		//~ sliceIndex  int
		//~ unknownKeys []string
		private List<string> key = new List<string>();
		private object lastValue;
		private ReflectWalk.Location loc;
		private List<object> cs = new List<object>();
		private List<object> csKey = new List<object>();
		private object csData;
		private int sliceIndex;
		private List<string> unknownKeys = new List<string>();


		//~ func (w *hashWalker) Enter(loc reflectwalk.Location) error {
		//~ 	w.loc = loc
		//~ 	return nil
		//~ }
		public void Enter(Location loc)
		{
			this.loc = loc;
		}

		//~ func (w *hashWalker) Exit(loc reflectwalk.Location) error {
		//~ 	w.loc = reflectwalk.None
		//~
		//~ 	switch loc {
		//~ 	case reflectwalk.Map:
		//~ 		w.cs = w.cs[:len(w.cs)-1]
		//~ 	case reflectwalk.MapValue:
		//~ 		w.key = w.key[:len(w.key)-1]
		//~ 		w.csKey = w.csKey[:len(w.csKey)-1]
		//~ 	case reflectwalk.Slice:
		//~ 		w.cs = w.cs[:len(w.cs)-1]
		//~ 	case reflectwalk.SliceElem:
		//~ 		w.csKey = w.csKey[:len(w.csKey)-1]
		//~ 	}
		//~
		//~ 	return nil
		//~ }
		public void Exit(Location loc)
		{
			this.loc = Location.None;
			switch (loc)
			{
				case Location.Map:
					cs.RemoveAt(cs.Count - 1);
					break;
				case Location.MapValue:
					key.RemoveAt(key.Count - 1);
					break;
				case Location.Slice:
					cs.RemoveAt(cs.Count - 1);
					break;
				case Location.SliceElem:
					csKey.RemoveAt(csKey.Count - 1);
					break;
			}
		}

		//~ func (w *hashWalker) Map(m reflect.Value) error {
		//~ 	w.cs = append(w.cs, m)
		//~ 	return nil
		//~ }
		public void Map(object m)
		{
			cs.Add(m);
		}

		//~ func (w *hashWalker) MapElem(m, k, v reflect.Value) error {
		//~ 	w.csData = k
		//~ 	w.csKey = append(w.csKey, k)
		//~ 	w.key = append(w.key, k.String())
		//~ 	w.lastValue = v
		//~ 	return nil
		//~ }
		public void MapElem(object m, object k, object v)
		{
			csData = k;
			csKey.Add(k);
			key.Add(k.ToString());
			lastValue = v;
		}

		//~ func (w *hashWalker) Slice(s reflect.Value) error {
		//~ 	w.cs = append(w.cs, s)
		//~ 	return nil
		//~ }
		public void Slice(object s)
		{
			cs.Add(s);
		}

		//~ func (w *hashWalker) SliceElem(i int, elem reflect.Value) error {
		//~ 	w.csKey = append(w.csKey, reflect.ValueOf(i))
		//~ 	w.sliceIndex = i
		//~ 	return nil
		//~ }
		public void SliceElem(int index, object value)
		{
			csKey.Add(index);
			sliceIndex = index;
		}

		//~ func (w *hashWalker) Primitive(v reflect.Value) error {
		public void Primitive(ref object v)
		{
			//~ if w.Callback == nil {
			//~ 	return nil
			//~ }
			if (Callback == null)
				return;

			// We don't touch map keys
			//~ if w.loc == reflectwalk.MapKey {
			//~ 	return nil
			//~ }
			if (loc == Location.MapKey)
				return;

			//~ setV := v

			// We only care about strings
			//~ if v.Kind() == reflect.Interface {
			//~ 	setV = v
			//~ 	v = v.Elem()
			//~ }
			//~ if v.Kind() != reflect.String {
			//~ 	return nil
			//~ }
			if (v.GetType().GetTypeInfo().IsInterface)
				throw new NotSupportedException("we should never see an interface");
			if (!(v is string))
				return;

			//~ replaceVal := w.Callback(v.String())
			var replaceVal = Callback((string)v);

			//~ resultVal := reflect.ValueOf(replaceVal)
			var resultVal = replaceVal;

			//~ switch w.loc {
			switch (loc)
			{
				//~ case reflectwalk.MapKey:
				//~ 	m := w.cs[len(w.cs)-1]
				case Location.MapKey:
					var m = cs[cs.Count - 1] as IDictionary<string, object>;
					
					// Delete the old value
					//~ var zero reflect.Value
					//~ m.SetMapIndex(w.csData.(reflect.Value), zero)
					m.Remove(csData as string);

					// Set the new key with the existing value
					//~ m.SetMapIndex(resultVal, w.lastValue)
					m[resultVal] = lastValue;

					// Set the key to be the new key
					//~ w.csData = resultVal
					csData = resultVal;
					break;
				//~ case reflectwalk.MapValue:
				case Location.MapValue:
					// If we're in a map, then the only way to set a map value is
					// to set it directly.
					//~ m := w.cs[len(w.cs)-1]
					//~ mk := w.csData.(reflect.Value)
					//~ m.SetMapIndex(mk, resultVal)
					m = cs[cs.Count - 1] as IDictionary<string, object>;
					var mk = csData as string;
					m[mk] = resultVal;
					break;
				//~ default:
				default:
					// Otherwise, we should be addressable
					//~ setV.Set(resultVal)
					v = resultVal;
					break;
			}

			//~ return nil
		}

		//~ func (w *hashWalker) removeCurrent() {
		private void removeCurrent()
		{
			// Append the key to the unknown keys
			//~ w.unknownKeys = append(w.unknownKeys, strings.Join(w.key, "."))
			unknownKeys.Add(string.Join(".", key));

			//~ for i := 1; i <= len(w.cs); i++ {
			for (var i = 1; i < cs.Count; i++)
			{
				//~ c := w.cs[len(w.cs)-i]
				var c = cs[cs.Count - 1];

				//~ switch c.Kind() {
				switch (c)
				{
					//~ case reflect.Map:
					case IDictionary<string, object> d:
						// Zero value so that we delete the map key
						//~ var val reflect.Value

						// Get the key and delete it
						//~ k := w.csData.(reflect.Value)
						//~ c.SetMapIndex(k, val)
						//~ return
						var k = csData as string;
						d.Remove(k);
						return;
				}
			}

			//~ panic("No container found for removeCurrent")
			throw new Exception("No container found for removeCurrent");
		}

		//~ func (w *hashWalker) replaceCurrent(v reflect.Value) {
		public void replaceCurrent(object v)
		{
			//~ c := w.cs[len(w.cs)-2]
			//~ switch c.Kind() {
			//~ case reflect.Map:
			//~ 	// Get the key and delete it
			//~ 	k := w.csKey[len(w.csKey)-1]
			//~ 	c.SetMapIndex(k, v)
			//~ }
			var c = cs[cs.Count - 2];
			switch (c)
			{
				case IDictionary<string, object> d:
					var k = csKey[csKey.Count - 1] as string;
					d[k] = v;
					break;
			}
		}
	}
}
