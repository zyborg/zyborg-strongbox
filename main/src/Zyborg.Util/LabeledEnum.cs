using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Zyborg.Util
{
	//[TypeConverter(typeof(LabeledEnumConverter))]
	[Newtonsoft.Json.JsonConverter(typeof(LabeledEnumJsonConverter))]
	public class LabeledEnum<T, L> where T : LabeledEnum<T, L>
	{
		protected static readonly ConcurrentDictionary<L, T> _byLabel =
				new ConcurrentDictionary<L, T>();

		private L _Label;

		protected LabeledEnum()
		{ }

		public L Label
		{
			get => _Label;
		}

		public static T From(L label)
		{
			if (_byLabel.TryGetValue(label, out var value))
				return value;

			value = Activator.CreateInstance<T>();
			value._Label = label;
			_byLabel.TryAdd(label, value);

			return value;
		}

		public static IEnumerable<L> Labels()
		{
			return _byLabel.Keys;
		}

		public static IEnumerable<T> Values()
		{
			return _byLabel.Values;
		}
	}

	public class LabeledEnumJsonConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType.GetGenericTypeDefinition() == typeof(LabeledEnum<,>);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var s = (object)reader.Value;
			var m = objectType.GetTypeInfo().GetMethod("From",
						BindingFlags.FlattenHierarchy | BindingFlags.Static | BindingFlags.Public);
			var p = m.GetParameters()[0].ParameterType;
			if (p != typeof(string))
				s = TypeDescriptor.GetConverter(p).ConvertFrom(s);
			return m.Invoke(null, new object[] { s });
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var label = value.GetType().GetTypeInfo().GetProperty("Label").GetValue(value);
			writer.WriteValue(label.ToString());
		}
	}
}
