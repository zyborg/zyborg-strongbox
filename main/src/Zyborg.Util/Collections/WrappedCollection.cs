using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Zyborg.Util.Collections
{
	public class WrappedCollection<T> : ICollection<T>
	{
		ICollection _c;

		public WrappedCollection(ICollection c)
		{
			_c = c;
		}

		public int Count => _c.Count;

		public bool IsReadOnly => true;

		public void Add(T item)
		{
			throw new NotImplementedException();
		}

		public void Clear()
		{
			throw new NotImplementedException();
		}

		public bool Contains(T item)
		{
			throw new NotImplementedException();
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			//_c.CopyTo(array, arrayIndex);
			var index = 0;
			foreach (var item in _c)
				array[arrayIndex + (index++)] = (T)item;
		}

		public IEnumerator<T> GetEnumerator()
		{
			foreach (var v in _c)
				yield return (T)v;
		}

		public bool Remove(T item)
		{
			throw new NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _c.GetEnumerator();
		}
	}
}
