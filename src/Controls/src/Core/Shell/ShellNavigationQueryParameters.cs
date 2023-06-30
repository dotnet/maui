using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Controls
{
	public class ShellNavigationQueryParameters : IDictionary<string, object>
	{
		Dictionary<string, object> _internal = new Dictionary<string, object>();
		bool _isReadonly;

		public ShellNavigationQueryParameters()
		{
		}

		public ShellNavigationQueryParameters(IEnumerable<KeyValuePair<string, object>> collection)
		{
			foreach (var item in collection)
				this.Add(item.Key, item.Value);
		}

		public ShellNavigationQueryParameters(IDictionary<string, object> dictionary)
		{
			foreach (var item in dictionary)
				this.Add(item.Key, item.Value);
		}

		internal ShellNavigationQueryParameters SetToReadOnly()
		{
			_isReadonly = true;
			return this;
		}

		void CheckReadOnlyState()
		{
			if (_isReadonly)
				throw new InvalidOperationException($"ShellNavigationQueryParameters are ReadOnly");
		}

		public object this[string key]
		{
			get => _internal[key];
			set
			{
				CheckReadOnlyState();
				_internal[key] = value;
			}
		}

		public ICollection<string> Keys => _internal.Keys;

		public ICollection<object> Values => _internal.Values;

		public int Count => _internal.Count;

		public bool IsReadOnly => _isReadonly;

		public void Add(string key, object value)
		{
			CheckReadOnlyState();
			_internal.Add(key, value);
		}

		public void Add(KeyValuePair<string, object> item)
		{
			CheckReadOnlyState();
			_internal.Add(item.Key, item.Value);
		}

		public void Clear()
		{
			CheckReadOnlyState();
			_internal.Clear();
		}

		public bool Contains(KeyValuePair<string, object> item) => _internal.ContainsKey(item.Key);

		public bool ContainsKey(string key) => _internal.ContainsKey(key);

		public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
			=> (_internal as ICollection<KeyValuePair<string, object>>)?.CopyTo(array, arrayIndex);

		public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _internal.GetEnumerator();

		public bool Remove(string key)
		{
			CheckReadOnlyState();
			return _internal.Remove(key);
		}

		public bool Remove(KeyValuePair<string, object> item)
		{
			CheckReadOnlyState();
			return (_internal as ICollection<KeyValuePair<string, object>>).Remove(item);
		}

#if NETSTANDARD2_1 || NETSTANDARD2_0
		public bool TryGetValue(string key, out object value)
#else
		public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
#endif
		{
			return _internal.TryGetValue(key, out value);
		}

		IEnumerator IEnumerable.GetEnumerator() =>
			_internal.GetEnumerator();
	}
}