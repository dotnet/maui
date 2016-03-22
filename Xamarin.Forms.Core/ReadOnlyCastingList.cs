using System.Collections;
using System.Collections.Generic;

namespace Xamarin.Forms
{
	internal class ReadOnlyCastingList<T, TFrom> : IReadOnlyList<T> where T : class where TFrom : class
	{
		readonly IList<TFrom> _list;

		public ReadOnlyCastingList(IList<TFrom> list)
		{
			_list = list;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)_list).GetEnumerator();
		}

		public IEnumerator<T> GetEnumerator()
		{
			return new CastingEnumerator<T, TFrom>(_list.GetEnumerator());
		}

		public int Count
		{
			get { return _list.Count; }
		}

		public T this[int index]
		{
			get { return _list[index] as T; }
		}
	}
}