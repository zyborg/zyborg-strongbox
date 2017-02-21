using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Zyborg.Util
{
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
}
