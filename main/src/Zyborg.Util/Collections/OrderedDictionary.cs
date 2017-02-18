using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace Zyborg.Util.Collections
{
	public class OrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
	{
		private OrderedDictionary _d = new OrderedDictionary();

		public OrderedDictionary()
		{ }

		public OrderedDictionary(IDictionary<TKey, TValue> d)
		{
			foreach (var kv in d)
				this.Add(kv.Key, kv.Value);
		}

		public TValue this[TKey key]
		{
			get => (TValue)_d[key];
			set => _d[key] = value;
		}

		public ICollection<TKey> Keys => new WrappedCollection<TKey>(_d.Keys);

		public ICollection<TValue> Values => new WrappedCollection<TValue>(_d.Values);

		public int Count => _d.Count;

		public bool IsReadOnly => _d.IsReadOnly;

		public void Add(TKey key, TValue value)
		{
			_d.Add(key, value);
		}

		public void Add(KeyValuePair<TKey, TValue> item)
		{
			_d.Add(item.Key, item.Value);
		}

		public void Clear()
		{
			_d.Clear();
		}

		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			// TODO: should this check for both key *AND* value equality?
			return _d.Contains(item.Key);
		}

		public bool ContainsKey(TKey key)
		{
			return _d.Contains(key);
		}

		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			//_d.CopyTo(array, arrayIndex);
			var index = 0;
			foreach (var item in this)
				array[arrayIndex + (index++)] = item;
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			foreach (var kv in _d)
				yield return new KeyValuePair<TKey, TValue>(
					(TKey)((DictionaryEntry)kv).Key,
					(TValue)((DictionaryEntry)kv).Value);
		}

		public bool Remove(TKey key)
		{
			if (_d.Contains(key))
			{
				_d.Remove(key);
				return true;
			}
			return false;
		}

		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			return this.Remove(item.Key);
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			if (_d.Contains(key))
			{
				value = (TValue)_d[key];
				return true;
			}
			value = default(TValue);
			return false;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _d.GetEnumerator();
		}
	}
}
