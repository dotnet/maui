using System;
using System.Collections;
using System.Collections.Generic;

namespace Mono.Options
{
	public class OptionValueCollection : IList, IList<string>
	{
		readonly OptionContext _c;
		readonly List<string> _values = new List<string>();

		internal OptionValueCollection(OptionContext c)
		{
			_c = c;
		}

		#region IEnumerable

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _values.GetEnumerator();
		}

		#endregion

		#region IEnumerable<T>

		public IEnumerator<string> GetEnumerator()
		{
			return _values.GetEnumerator();
		}

		#endregion

		public List<string> ToList()
		{
			return new List<string>(_values);
		}

		public string[] ToArray()
		{
			return _values.ToArray();
		}

		public override string ToString()
		{
			return string.Join(", ", _values.ToArray());
		}

		#region ICollection

		void ICollection.CopyTo(Array array, int index)
		{
			(_values as ICollection).CopyTo(array, index);
		}

		bool ICollection.IsSynchronized => (_values as ICollection).IsSynchronized;

		object ICollection.SyncRoot => (_values as ICollection).SyncRoot;

		#endregion

		#region ICollection<T>

		public void Add(string item)
		{
			_values.Add(item);
		}

		public void Clear()
		{
			_values.Clear();
		}

		public bool Contains(string item)
		{
			return _values.Contains(item);
		}

		public void CopyTo(string[] array, int arrayIndex)
		{
			_values.CopyTo(array, arrayIndex);
		}

		public bool Remove(string item)
		{
			return _values.Remove(item);
		}

		public int Count => _values.Count;

		public bool IsReadOnly => false;

		#endregion

		#region IList

		int IList.Add(object value)
		{
			return (_values as IList).Add(value);
		}

		bool IList.Contains(object value)
		{
			return (_values as IList).Contains(value);
		}

		int IList.IndexOf(object value)
		{
			return (_values as IList).IndexOf(value);
		}

		void IList.Insert(int index, object value)
		{
			(_values as IList).Insert(index, value);
		}

		void IList.Remove(object value)
		{
			(_values as IList).Remove(value);
		}

		void IList.RemoveAt(int index)
		{
			(_values as IList).RemoveAt(index);
		}

		bool IList.IsFixedSize => false;

		object IList.this[int index]
		{
			get { return this[index]; }
			set { (_values as IList)[index] = value; }
		}

		#endregion

		#region IList<T>

		public int IndexOf(string item)
		{
			return _values.IndexOf(item);
		}

		public void Insert(int index, string item)
		{
			_values.Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			_values.RemoveAt(index);
		}

		void AssertValid(int index)
		{
			if (_c.Option == null)
				throw new InvalidOperationException("OptionContext.Option is null.");
			if (index >= _c.Option.MaxValueCount)
				throw new ArgumentOutOfRangeException("index");
			if (_c.Option.OptionValueType == OptionValueType.Required &&
			    index >= _values.Count)
			{
				throw new OptionException(string.Format(
					_c.OptionSet.MessageLocalizer("Missing required value for option '{0}'."), _c.OptionName),
					_c.OptionName);
			}
		}

		public string this[int index]
		{
			get
			{
				AssertValid(index);
				return index >= _values.Count ? null : _values[index];
			}
			set { _values[index] = value; }
		}

		#endregion
	}
}