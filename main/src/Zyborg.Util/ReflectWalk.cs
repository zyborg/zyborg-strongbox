using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Zyborg.Util
{
    public static class ReflectWalk
    {
		public enum Location
		{
			//~ type Location uint
			//~ 
			//~ const (
			//~ 	None Location = iota
			//~ 	Map
			//~ 	MapKey
			//~ 	MapValue
			//~ 	Slice
			//~ 	SliceElem
			//~ 	Array
			//~ 	ArrayElem
			//~ 	Struct
			//~ 	StructField
			//~ 	WalkLoc
			//~ )
			None,
			Map,
			MapKey,
			MapValue,
			Slice,
			SliceElem,
			Array,
			ArrayElem,
			Struct,
			StructField,
			WalkLoc,
		}

		//~ const _Location_name = "NoneMapMapKeyMapValueSliceSliceElemStructStructFieldWalkLoc"
		//~ 
		//~ var _Location_index = [...]uint8{0, 4, 7, 13, 21, 26, 35, 41, 52, 59}
		//~ 
		//~ func (i Location) String() string {
		//~ 	if i+1 >= Location(len(_Location_index)) {
		//~ 		return fmt.Sprintf("Location(%d)", i)
		//~ 	}
		//~ 	return _Location_name[_Location_index[i]:_Location_index[i+1]]
		//~ }
		public static string ToString(this Location i)
		{
			return Enum.GetName(typeof(Location), i);
		}

		public interface IWalker
		{ }

		// PrimitiveWalker implementations are able to handle primitive values
		// within complex structures. Primitive values are numbers, strings,
		// booleans, funcs, chans.
		//
		// These primitive values are often members of more complex
		// structures (slices, maps, etc.) that are walkable by other interfaces.
		//~ type PrimitiveWalker interface {
		//~ 	Primitive(reflect.Value) error
		//~ }
		public interface IPrimitiveWalker : IWalker
		{
			void Primitive(ref object value);
		}

		// InterfaceWalker implementations are able to handle interface values as they
		// are encountered during the walk.
		//~ type InterfaceWalker interface {
		//~ 	Interface(reflect.Value) error
		//~ }
		public interface IInterfaceWalker : IWalker
		{
			void Interface(object value);
		}

		// MapWalker implementations are able to handle individual elements
		// found within a map structure.
		//~ type MapWalker interface {
		//~ 	Map(m reflect.Value) error
		//~ 	MapElem(m, k, v reflect.Value) error
		//~ }
		public interface IMapWalker : IWalker
		{
			void Map(object m);
			void MapElem(object m, object k, object v);
		}

		// SliceWalker implementations are able to handle slice elements found
		// within complex structures.
		//~ type SliceWalker interface {
		//~ 	Slice(reflect.Value) error
		//~ 	SliceElem(int, reflect.Value) error
		//~ }
		public interface ISliceWalker : IWalker
		{
			void Slice(object value);
			void SliceElem(int index, object value);
		}

		// ArrayWalker implementations are able to handle array elements found
		// within complex structures.
		//~ type ArrayWalker interface {
		//~ 	Array(reflect.Value) error
		//~ 	ArrayElem(int, reflect.Value) error
		//~ }
		public interface IArrayWalker : IWalker
		{
			void Array(object value);
			void ArrayElem(int index, object value);
		}

		// StructWalker is an interface that has methods that are called for
		// structs when a Walk is done.
		//~ type StructWalker interface {
		//~ 	Struct(reflect.Value) error
		//~ 	StructField(reflect.StructField, reflect.Value) error
		//~ }
		public interface IStructWalker : IWalker
		{
			void Struct(object value);
			void StructField(MemberInfo member, object value);
		}

		// EnterExitWalker implementations are notified before and after
		// they walk deeper into complex structures (into struct fields,
		// into slice elements, etc.)
		//~ type EnterExitWalker interface {
		//~ 	Enter(Location) error
		//~ 	Exit(Location) error
		//~ }
		public interface IEnterExitWalker : IWalker
		{
			void Enter(Location loc);
			void Exit(Location loc);
		}

		// PointerWalker implementations are notified when the value they're
		// walking is a pointer or not. Pointer is called for _every_ value whether
		// it is a pointer or not.
		//~ type PointerWalker interface {
		//~ 	PointerEnter(bool) error
		//~ 	PointerExit(bool) error
		//~ }
		public interface IPointerWalker : IWalker
		{
			void PointerEnter(bool isPointer);
			void PointerExit(bool isPointer);
		}

		// SkipEntry can be returned from walk functions to skip walking
		// the value of this field. This is only valid in the following functions:
		//
		//   - StructField: skips walking the struct value
		//
		//~ var SkipEntry = errors.New("skip this entry")
		public class SkipEntryException : Exception
		{
			public SkipEntryException() : base("skip this entry")
			{ }
		}

		// Walk takes an arbitrary value and an interface and traverses the
		// value, calling callbacks on the interface if they are supported.
		// The interface should implement one or more of the walker interfaces
		// in this package, such as PrimitiveWalker, StructWalker, etc.
		//~ func Walk(data, walker interface{}) (err error) {
		public static void Walk(object data, IWalker walker)
		{
			//~ v := reflect.ValueOf(data)
			//~ ew, ok := walker.(EnterExitWalker)
			//~ if ok {
			//~ 	err = ew.Enter(WalkLoc)
			//~ }
			var v = data;
			var ew = walker as IEnterExitWalker;
			if (ew != null)
				ew.Enter(Location.WalkLoc);

			//~ if err == nil {
			//~ 	err = walk(v, walker)
			//~ }
			WalkInternal(ref v, walker);

			//~ if ok && err == nil {
			//~ 	err = ew.Exit(WalkLoc)
			//~ }
			if (ew != null)
				ew.Exit(Location.WalkLoc);
		
			//~ return
		}

		//~ func walk(v reflect.Value, w interface{}) (err error) {
		public static void WalkInternal(ref object v, IWalker w)
		{
			// Determine if we're receiving a pointer and if so notify the walker.
			// The logic here is convoluted but very important (tests will fail if
			// almost any part is changed). I will try to explain here.
			//
			// First, we check if the value is an interface, if so, we really need
			// to check the interface's VALUE to see whether it is a pointer.
			//
			// Check whether the value is then a pointer. If so, then set pointer
			// to true to notify the user.
			//
			// If we still have a pointer or an interface after the indirections, then
			// we unwrap another level
			//
			// At this time, we also set "v" to be the dereferenced value. This is
			// because once we've unwrapped the pointer we want to use that value.
			//~ pointer := false
			//~ pointerV := v
			var pointer = false;
			var pointerV = v;

			//~ for {
			//~ 	if pointerV.Kind() == reflect.Interface {
			//~ 		if iw, ok := w.(InterfaceWalker); ok {
			//~ 			if err = iw.Interface(pointerV); err != nil {
			//~ 				return
			//~ 			}
			//~ 		}
			//~ 
			//~ 		pointerV = pointerV.Elem()
			//~ 	}
			//~ 
			//~ 	if pointerV.Kind() == reflect.Ptr {
			//~ 		pointer = true
			//~ 		v = reflect.Indirect(pointerV)
			//~ 	}
			//~ 	if pw, ok := w.(PointerWalker); ok {
			//~ 		if err = pw.PointerEnter(pointer); err != nil {
			//~ 			return
			//~ 		}
			//~ 
			//~ 		defer func(pointer bool) {
			//~ 			if err != nil {
			//~ 				return
			//~ 			}
			//~ 
			//~ 			err = pw.PointerExit(pointer)
			//~ 		}(pointer)
			//~ 	}
			//~ 
			//~ 	if pointer {
			//~ 		pointerV = v
			//~ 	}
			//~ 	pointer = false
			//~ 
			//~ 	// If we still have a pointer or interface we have to indirect another level.
			//~ 	switch pointerV.Kind() {
			//~ 	case reflect.Ptr, reflect.Interface:
			//~ 		continue
			//~ 	}
			//~ 	break
			//~ }

			// We preserve the original value here because if it is an interface
			// type, we want to pass that directly into the walkPrimitive, so that
			// we can set it.
			//~ originalV := v
			//~ if v.Kind() == reflect.Interface {
			//~ 	v = v.Elem()
			//~ }
			var originalV = v;
			if (v.GetType().GetTypeInfo().IsInterface)
				throw new NotSupportedException("did not expect an interface");

			//~ k := v.Kind()
			//~ if k >= reflect.Int && k <= reflect.Complex128 {
			//~ 	k = reflect.Int
			//~ }

			//~ switch k {
			//~ // Primitives
			//~ case reflect.Bool, reflect.Chan, reflect.Func, reflect.Int, reflect.String, reflect.Invalid:
			//~ 	err = walkPrimitive(originalV, w)
			//~ 	return
			//~ case reflect.Map:
			//~ 	err = walkMap(v, w)
			//~ 	return
			//~ case reflect.Slice:
			//~ 	err = walkSlice(v, w)
			//~ 	return
			//~ case reflect.Struct:
			//~ 	err = walkStruct(v, w)
			//~ 	return
			//~ case reflect.Array:
			//~ 	err = walkArray(v, w)
			//~ 	return
			//~ default:
			//~ 	panic("unsupported type: " + k.String())
			//~ }
			switch (v)
			{
				case object x when x == null || x is bool || x is Delegate || x.GetType().IsNumber() || x is string:
					WalkPrimitive(ref originalV, w);
					break;
				case IDictionary<string, object> x:
					WalkMap(v, w);
					break;
				case IList<object> x:
					WalkArray(v, w);
					break;
				case object x when x.GetType().GetTypeInfo().IsClass || x.GetType().GetTypeInfo().IsValueType:
					WalkStruct(v, w);
					break;
				default:
					throw new Exception("unsupported type: " + v.GetType().FullName);
			}
		}

		//~ func walkMap(v reflect.Value, w interface{}) error {
		public static void WalkMap(object v, IWalker w)
		{
			//~ ew, ewok := w.(EnterExitWalker)
			//~ if ewok {
			//~ 	ew.Enter(Map)
			//~ }
			var ew = w as IEnterExitWalker;
			if (ew != null)
				ew.Enter(Location.Map);

			//~ if mw, ok := w.(MapWalker); ok {
			//~ 	if err := mw.Map(v); err != nil {
			//~ 		return err
			//~ 	}
			//~ }
			var mw = w as IMapWalker;
			if (mw != null)
				mw.Map(v);

			//~ for _, k := range v.MapKeys() {
			foreach (var kv in ((IDictionary<string, object>)v).ToArray())
			{
				//~ kv := v.MapIndex(k)

				//~ if mw, ok := w.(MapWalker); ok {
				//~ 	if err := mw.MapElem(v, k, kv); err != nil {
				//~ 		return err
				//~ 	}
				//~ }
				mw = w as IMapWalker;
				if (mw != null)
					mw.MapElem(v, kv.Key, kv.Value);

				//~ ew, ok := w.(EnterExitWalker)
				//~ if ok {
				//~ 	ew.Enter(MapKey)
				//~ }
				ew = w as IEnterExitWalker;
				if (ew != null)
					ew.Enter(Location.MapKey);

				//~ if err := walk(k, w); err != nil {
				//~ 	return err
				//~ }
				object kvKey = kv.Key;
				WalkInternal(ref kvKey, w);
				if (kv.Key != kvKey as string)
					throw new NotSupportedException("unexpected key manipulation");

				//~ if ok {
				//~ 	ew.Exit(MapKey)
				//~ 	ew.Enter(MapValue)
				//~ }
				if (ew != null)
				{
					ew.Exit(Location.MapKey);
					ew.Enter(Location.MapValue);
				}

				//~ if err := walk(kv, w); err != nil {
				//~ 	return err
				//~ }
				var kvValue = kv.Value;
				WalkInternal(ref kvValue, w);
				if (kv.Value != kvValue)
					((IDictionary<string, object>)v)[kv.Key] = kvValue;

				//~ if ok {
				//~ 	ew.Exit(MapValue)
				//~ }
				if (ew != null)
					ew.Exit(Location.MapValue);
			}

			//~ if ewok {
			//~ 	ew.Exit(Map)
			//~ }
			if (ew != null)
				ew.Exit(Location.Map);

			//~ return nil
		}

		//~ func walkPrimitive(v reflect.Value, w interface{}) error {
		private static void WalkPrimitive(ref object v, IWalker w)
		{
			//~ if pw, ok := w.(PrimitiveWalker); ok {
			//~ 	return pw.Primitive(v)
			//~ }
			var pw = w as IPrimitiveWalker;
			if (pw != null)
				pw.Primitive(ref v);

			//~ return nil
		}

		//~ func walkSlice(v reflect.Value, w interface{}) (err error) {
		public static void WalkSlice(object v, IWalker w)
		{
			//~ ew, ok := w.(EnterExitWalker)
			//~ if ok {
			//~ 	ew.Enter(Slice)
			//~ }
			var ew = w as IEnterExitWalker;
			if (ew != null)
				ew.Enter(Location.Slice);

			//~ if sw, ok := w.(SliceWalker); ok {
			//~ 	if err := sw.Slice(v); err != nil {
			//~ 		return err
			//~ 	}
			//~ }
			var sw = w as ISliceWalker;
			if (sw != null)
				sw.Slice(v);

			//~ for i := 0; i < v.Len(); i++ {
			for (var i = 0; i < ((IList<object>)v).Count; i++)
			{
				//~ elem := v.Index(i)
				var elem = ((IList<object>)v)[i];

				//~ if sw, ok := w.(SliceWalker); ok {
				//~ 	if err := sw.SliceElem(i, elem); err != nil {
				//~ 		return err
				//~ 	}
				//~ }
				sw = w as ISliceWalker;
				if (sw != null)
					sw.SliceElem(i, elem);

				//~ ew, ok := w.(EnterExitWalker)
				//~ if ok {
				//~ 	ew.Enter(SliceElem)
				//~ }
				ew = w as IEnterExitWalker;
				if (ew != null)
					ew.Enter(Location.SliceElem);

				//~ if err := walk(elem, w); err != nil {
				//~ 	return err
				//~ }
				WalkInternal(ref elem, w);

				//~ if ok {
				//~ 	ew.Exit(SliceElem)
				//~ }
				if (ew != null)
					ew.Exit(Location.SliceElem);
			}

			//~ ew, ok = w.(EnterExitWalker)
			//~ if ok {
			//~ 	ew.Exit(Slice)
			//~ }
			ew = w as IEnterExitWalker;
			if (ew != null)
				ew.Exit(Location.Slice);

			//~ return nil
		}

		//~ func walkArray(v reflect.Value, w interface{}) (err error) {
		private static void WalkArray(object v, IWalker w)
		{
			//~ ew, ok := w.(EnterExitWalker)
			//~ if ok {
			//~ 	ew.Enter(Array)
			//~ }
			var ew = w as IEnterExitWalker;
			if (ew != null)
				ew.Enter(Location.Array);

			//~ if aw, ok := w.(ArrayWalker); ok {
			//~ 	if err := aw.Array(v); err != nil {
			//~ 		return err
			//~ 	}
			//~ }
			var aw = w as IArrayWalker;
			if (aw != null)
				aw.Array(v);

			//~ for i := 0; i < v.Len(); i++ {
			for (var i = 0; i < ((Array)v).Length; i++)
			{
				//~ elem := v.Index(i)
				var elem = ((Array)v).GetValue(i);

				//~ if aw, ok := w.(ArrayWalker); ok {
				//~ 	if err := aw.ArrayElem(i, elem); err != nil {
				//~ 		return err
				//~ 	}
				//~ }
				aw = w as IArrayWalker;
				if (aw != null)
					aw.ArrayElem(i, elem);

				//~ ew, ok := w.(EnterExitWalker)
				//~ if ok {
				//~ 	ew.Enter(ArrayElem)
				//~ }
				ew = w as IEnterExitWalker;
				if (ew != null)
					ew.Enter(Location.Array);

				//~ if err := walk(elem, w); err != nil {
				//~ 	return err
				//~ }
				WalkInternal(ref elem, w);

				//~ if ok {
				//~ 	ew.Exit(ArrayElem)
				//~ }
				if (ew != null)
					ew.Exit(Location.ArrayElem);
			}

			//~ ew, ok = w.(EnterExitWalker)
			//~ if ok {
			//~ 	ew.Exit(Array)
			//~ }
			ew = w as IEnterExitWalker;
			if (w != null)
				ew.Exit(Location.Array);

			//~ return nil
		}

		//~ func walkStruct(v reflect.Value, w interface{}) (err error) {
		private static void WalkStruct(object v, IWalker w)
		{
			//~ ew, ewok := w.(EnterExitWalker)
			//~ if ewok {
			//~ 	ew.Enter(Struct)
			//~ }
			var ew = w as IEnterExitWalker;
			if (ew != null)
				ew.Enter(Location.Struct);

			//~ if sw, ok := w.(StructWalker); ok {
			//~ 	if err = sw.Struct(v); err != nil {
			//~ 		return
			//~ 	}
			//~ }
			var sw = w as IStructWalker;
			if (sw != null)
				sw.Struct(v);

			//~ vt := v.Type()
			var vt = v.GetType();
			//~ for i := 0; i < vt.NumField(); i++ {
			foreach (var m in vt.GetMembers(BindingFlags.Instance))
			{
				//~ sf := vt.Field(i)
				//~ f := v.FieldByIndex([]int{i})
				var sf = m;
				object f = null;
				if (m.MemberType == MemberTypes.Field)
				{
					f = ((FieldInfo)m).GetValue(v);
				}
				else if (m.MemberType == MemberTypes.Property)
				{
					f = ((PropertyInfo)m).GetValue(v);
				}
				else
				{
					continue;
				}

				//~ if sw, ok := w.(StructWalker); ok {
				//~ 	err = sw.StructField(sf, f)
				//~ 
				//~ 	// SkipEntry just pretends this field doesn't even exist
				//~ 	if err == SkipEntry {
				//~ 		continue
				//~ 	}
				//~ 
				//~ 	if err != nil {
				//~ 		return
				//~ 	}
				//~ }
				sw = w as IStructWalker;
				if (sw != null)
					sw.StructField(sf, f);

				//~ ew, ok := w.(EnterExitWalker)
				//~ if ok {
				//~ 	ew.Enter(StructField)
				//~ }
				ew = w as IEnterExitWalker;
				if (ew != null)
					ew.Enter(Location.StructField);

				//~ err = walk(f, w)
				//~ if err != nil {
				//~ 	return
				//~ }
				WalkInternal(ref f, w);

				//~ if ok {
				//~ 	ew.Exit(StructField)
				//~ }
				if (ew != null)
					ew.Exit(Location.StructField);
			}

			//~ if ewok {
			//~ 	ew.Exit(Struct)
			//~ }
			if (ew != null)
				ew.Exit(Location.Struct);

			//~ return nil
		}
	}
}
