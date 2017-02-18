using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Zyborg.Util
{
    public class ConfigMap<TKey, TValue> : Dictionary<TKey, TValue>
    {
		public ConfigMap()
		{ }

		public ConfigMap(IEqualityComparer<TKey> comparer)
			: base(comparer)
		{ }
	}

	public class ConfigMap<TValue> : ConfigMap<string, TValue>
	{
		public ConfigMap()
			: base(StringComparer.CurrentCultureIgnoreCase)
		{ }
	}
}
