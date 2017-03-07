//
// OrderedDictionary.cs
//
// Author:
//   Eric Maupin  <me@ermau.com>
//
// Copyright (c) 2009 Eric Maupin (http://www.ermau.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace Cadenza.Collections
{
	internal sealed class OrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IList<KeyValuePair<TKey, TValue>>
	{
		readonly Dictionary<TKey, TValue> _dict;
		readonly List<TKey> _keyOrder;
		readonly ICollection<KeyValuePair<TKey, TValue>> _kvpCollection;
		readonly ReadOnlyCollection<TKey> _roKeys;

		readonly ReadOnlyValueCollection _roValues;

		public OrderedDictionary() : this(0)
		{
		}

		public OrderedDictionary(int capacity) : this(capacity, EqualityComparer<TKey>.Default)
		{
		}

		public OrderedDictionary(IEqualityComparer<TKey> equalityComparer) : this(0, equalityComparer)
		{
		}

		public OrderedDictionary(int capacity, IEqualityComparer<TKey> equalityComparer)
		{
			_dict = new Dictionary<TKey, TValue>(capacity, equalityComparer);
			_kvpCollection = _dict;
			_keyOrder = new List<TKey>(capacity);
			_roKeys = new ReadOnlyCollection<TKey>(_keyOrder);
			_roValues = new ReadOnlyValueCollection(this);
		}

		public OrderedDictionary(ICollection<KeyValuePair<TKey, TValue>> dictionary) : this(dictionary, EqualityComparer<TKey>.Default)
		{
		}

		public OrderedDictionary(ICollection<KeyValuePair<TKey, TValue>> dictionary, IEqualityComparer<TKey> equalityComparer) : this(dictionary != null ? dictionary.Count : 0, equalityComparer)
		{
			if (dictionary == null)
				throw new ArgumentNullException("dictionary");

			foreach (KeyValuePair<TKey, TValue> kvp in dictionary)
				Add(kvp.Key, kvp.Value);
		}

		/// <summary>
		///     Gets the equality comparer being used for
		///     <typeparam name="TKey" />
		///     .
		/// </summary>
		public IEqualityComparer<TKey> Comparer
		{
			get { return _dict.Comparer; }
		}

		/// <summary>
		///     Gets the value at the specified index.
		/// </summary>
		/// <param name="index">The index to get the value at.</param>
		/// <returns>The value at the specified index.</returns>
		/// <exception cref="IndexOutOfRangeException">
		///     <paramref name="index" /> is less than 0 or greater than
		///     <see cref="Count" />.
		/// </exception>
		public TValue this[int index]
		{
			get { return _dict[_keyOrder[index]]; }
		}

		void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
		{
			Add(item.Key, item.Value);
		}

		/// <summary>
		///     Clears the dictionary.
		/// </summary>
		public void Clear()
		{
			_dict.Clear();
			_keyOrder.Clear();
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
		{
			return _kvpCollection.Contains(item);
		}

		void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			if (array == null)
				throw new ArgumentNullException("array");
			if (Count > array.Length - arrayIndex)
				throw new ArgumentException("Not enough space in array to copy");
			if (arrayIndex < 0)
				throw new ArgumentOutOfRangeException("arrayIndex");

			for (var i = 0; i < _keyOrder.Count; ++i)
			{
				TKey key = _keyOrder[i];
				array[arrayIndex++] = new KeyValuePair<TKey, TValue>(key, _dict[key]);
			}
		}

		/// <summary>
		///     Gets the number of items in the dictionary.
		/// </summary>
		public int Count
		{
			get { return _dict.Count; }
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
		{
			get { return false; }
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
		{
			return _kvpCollection.Remove(item) && _keyOrder.Remove(item.Key);
		}

		/// <summary>
		///     Adds the <paramref name="key" /> and <paramref name="value" /> to the dictionary.
		/// </summary>
		/// <param name="key">The key to associate with the <paramref name="value" />.</param>
		/// <param name="value">The value to add.</param>
		/// <exception cref="ArgumentNullException"><paramref name="key" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentException"><paramref name="key" /> already exists in the dictionary.</exception>
		public void Add(TKey key, TValue value)
		{
			_dict.Add(key, value);
			_keyOrder.Add(key);
		}

		/// <summary>
		///     Gets whether or not <paramref name="key" /> is in the dictionary.
		/// </summary>
		/// <param name="key">The key to look for.</param>
		/// <returns><c>true</c> if the key was found, <c>false</c> if not.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="key" /> is <c>null</c>.</exception>
		public bool ContainsKey(TKey key)
		{
			return _dict.ContainsKey(key);
		}

		/// <summary>
		///     Gets or sets the value associated with <paramref name="key" />.
		/// </summary>
		/// <param name="key">The key to get or set the value for.</param>
		/// <returns>The value associated with the key.</returns>
		/// <exception cref="KeyNotFoundException"><paramref name="key" /> was not found attempting to get.</exception>
		public TValue this[TKey key]
		{
			get { return _dict[key]; }
			set
			{
				if (!_dict.ContainsKey(key))
					_keyOrder.Add(key);

				_dict[key] = value;
			}
		}

		/// <summary>
		///     Gets a read only collection of keys in the dictionary.
		/// </summary>
		public ICollection<TKey> Keys
		{
			get { return _roKeys; }
		}

		/// <summary>
		///     Removes the key and associated value from the dictionary if found.
		/// </summary>
		/// <param name="key">The key to remove.</param>
		/// <returns><c>true</c> if the key was found, <c>false</c> if not.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="key" /> is <c>null</c>.</exception>
		public bool Remove(TKey key)
		{
			return _dict.Remove(key) && _keyOrder.Remove(key);
		}

		/// <summary>
		///     Attempts to get the <paramref name="value" /> for the <paramref name="key" />.
		/// </summary>
		/// <param name="key">The key to search for.</param>
		/// <param name="value">The value, if found.</param>
		/// <returns><c>true</c> if the key was found, <c>false</c> otherwise.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="key" /> is <c>null</c>.</exception>
		public bool TryGetValue(TKey key, out TValue value)
		{
			return _dict.TryGetValue(key, out value);
		}

		/// <summary>
		///     Gets a read only collection of values in the dictionary.
		/// </summary>
		public ICollection<TValue> Values
		{
			get { return _roValues; }
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			foreach (TKey key in _keyOrder)
				yield return new KeyValuePair<TKey, TValue>(key, this[key]);
		}

		int IList<KeyValuePair<TKey, TValue>>.IndexOf(KeyValuePair<TKey, TValue> item)
		{
			return _keyOrder.IndexOf(item.Key);
		}

		void IList<KeyValuePair<TKey, TValue>>.Insert(int index, KeyValuePair<TKey, TValue> item)
		{
			Insert(index, item.Key, item.Value);
		}

		KeyValuePair<TKey, TValue> IList<KeyValuePair<TKey, TValue>>.this[int index]
		{
			get { return new KeyValuePair<TKey, TValue>(_keyOrder[index], this[index]); }
			set
			{
				_keyOrder[index] = value.Key;
				_dict[value.Key] = value.Value;
			}
		}

		/// <summary>
		///     Removes they key and associated value from the dictionary located at <paramref name="index" />.
		/// </summary>
		/// <param name="index">The index at which to remove an item.</param>
		public void RemoveAt(int index)
		{
			TKey key = _keyOrder[index];
			Remove(key);
		}

		/// <summary>
		///     Gets whether or not <paramref name="value" /> is in the dictionary.
		/// </summary>
		/// <param name="value">The value to look for.</param>
		/// <returns><c>true</c> if the value was found, <c>false</c> if not.</returns>
		public bool ContainsValue(TValue value)
		{
			return _dict.ContainsValue(value);
		}

		/// <summary>
		///     Gets the index of <paramref name="key" />.
		/// </summary>
		/// <param name="key">The key to find the index of.</param>
		/// <returns>-1 if the key was not found, the index otherwise.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="key" /> is <c>null</c>.</exception>
		public int IndexOf(TKey key)
		{
			if (key == null)
				throw new ArgumentNullException("key");

			return _keyOrder.IndexOf(key);
		}

		/// <summary>
		///     Gets the index of <paramref name="key" /> starting with <paramref name="startIndex" />.
		/// </summary>
		/// <param name="key">The key to find the index of.</param>
		/// <param name="startIndex">The index to start the search at.</param>
		/// <returns>-1 if the key was not found, the index otherwise.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="key" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="startIndex" /> is not within the valid range of indexes.</exception>
		public int IndexOf(TKey key, int startIndex)
		{
			if (key == null)
				throw new ArgumentNullException("key");

			return _keyOrder.IndexOf(key, startIndex);
		}

		/// <summary>
		///     Gets the index of <paramref name="key" /> between the range given by <paramref name="startIndex" /> and
		///     <paramref name="count" />.
		/// </summary>
		/// <param name="key">The key to find the index of.</param>
		/// <param name="startIndex">The index to start the search at.</param>
		/// <param name="count">How many items to search, including the <paramref name="startIndex" />.</param>
		/// <returns>-1 if the key was not found, the index otherwise.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="key" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="startIndex" /> is not within the valid range of indexes.</exception>
		/// <exception cref="ArgumentOutOfRangeException">
		///     <paramref name="startIndex" /> and <paramref name="count" /> are not a
		///     valid range.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="count" /> is less than 0.</exception>
		public int IndexOf(TKey key, int startIndex, int count)
		{
			if (key == null)
				throw new ArgumentNullException("key");

			return _keyOrder.IndexOf(key, startIndex, count);
		}

		/// <summary>
		///     Inserts the <paramref name="key" /> and <paramref name="value" /> at the specified index.
		/// </summary>
		/// <param name="index">The index to insert the key and value at.</param>
		/// <param name="key">The key to assicate with the <paramref name="value" />.</param>
		/// <param name="value">The value to insert.</param>
		/// <exception cref="ArgumentNullException"><paramref name="key" /> is <c>null</c>.</exception>
		/// <exception cref="ArgumentOutOfRangeException">
		///     <paramref name="index" /> is less than 0 or greater than
		///     <see cref="Count" />
		/// </exception>
		public void Insert(int index, TKey key, TValue value)
		{
			_keyOrder.Insert(index, key);
			_dict.Add(key, value);
		}

		public void Move(int oldIndex, int newIndex)
		{
			TKey key = _keyOrder[oldIndex];
			_keyOrder.RemoveAt(oldIndex);
			_keyOrder.Insert(newIndex, key);
		}

		class ReadOnlyValueCollection : IList<TValue>
		{
			readonly OrderedDictionary<TKey, TValue> _odict;

			public ReadOnlyValueCollection(OrderedDictionary<TKey, TValue> dict)
			{
				_odict = dict;
			}

			public void Add(TValue item)
			{
				throw new NotSupportedException();
			}

			public void Clear()
			{
				throw new NotSupportedException();
			}

			public bool Contains(TValue item)
			{
				return _odict.ContainsValue(item);
			}

			public void CopyTo(TValue[] array, int arrayIndex)
			{
				if (array == null)
					throw new ArgumentNullException("array");
				if (Count > array.Length - arrayIndex)
					throw new ArgumentException("Not enough space in array to copy");
				if (arrayIndex < 0 || arrayIndex > array.Length)
					throw new ArgumentOutOfRangeException("arrayIndex");

				for (var i = 0; i < _odict.Count; ++i)
					array[arrayIndex++] = _odict[i];
			}

			public int Count
			{
				get { return _odict.Count; }
			}

			public bool IsReadOnly
			{
				get { return true; }
			}

			public bool Remove(TValue item)
			{
				throw new NotSupportedException();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			public IEnumerator<TValue> GetEnumerator()
			{
				for (var i = 0; i < _odict._keyOrder.Count; ++i)
					yield return _odict[i];
			}

			public int IndexOf(TValue item)
			{
				return _odict._dict.Values.IndexOf(item);
			}

			public void Insert(int index, TValue item)
			{
				throw new NotSupportedException();
			}

			public TValue this[int index]
			{
				get { return _odict[index]; }
				set { throw new NotSupportedException(); }
			}

			public void RemoveAt(int index)
			{
				throw new NotSupportedException();
			}
		}
	}
}