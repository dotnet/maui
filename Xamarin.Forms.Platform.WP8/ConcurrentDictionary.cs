// ConcurrentDictionary.cs
//
// Copyright (c) 2009 Jérémie "Garuma" Laval
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Xamarin.Forms
{
	internal class ConcurrentDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IDictionary, ICollection, IEnumerable
	{
		readonly IEqualityComparer<TKey> _comparer;

		SplitOrderedList<TKey, KeyValuePair<TKey, TValue>> _internalDictionary;

		public ConcurrentDictionary() : this(EqualityComparer<TKey>.Default)
		{
		}

		public ConcurrentDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection) : this(collection, EqualityComparer<TKey>.Default)
		{
		}

		public ConcurrentDictionary(IEqualityComparer<TKey> comparer)
		{
			_comparer = comparer;
			_internalDictionary = new SplitOrderedList<TKey, KeyValuePair<TKey, TValue>>(comparer);
		}

		public ConcurrentDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer) : this(comparer)
		{
			foreach (KeyValuePair<TKey, TValue> pair in collection)
				Add(pair.Key, pair.Value);
		}

		// Parameters unused
		public ConcurrentDictionary(int concurrencyLevel, int capacity) : this(EqualityComparer<TKey>.Default)
		{
		}

		public ConcurrentDictionary(int concurrencyLevel, IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer) : this(collection, comparer)
		{
		}

		// Parameters unused
		public ConcurrentDictionary(int concurrencyLevel, int capacity, IEqualityComparer<TKey> comparer) : this(comparer)
		{
		}

		public bool IsEmpty
		{
			get { return Count == 0; }
		}

		void ICollection.CopyTo(Array array, int startIndex)
		{
			var arr = array as KeyValuePair<TKey, TValue>[];
			if (arr == null)
				return;

			CopyTo(arr, startIndex, Count);
		}

		bool ICollection.IsSynchronized
		{
			get { return true; }
		}

		object ICollection.SyncRoot
		{
			get { return this; }
		}

		void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> pair)
		{
			Add(pair.Key, pair.Value);
		}

		public void Clear()
		{
			// Pronk
			_internalDictionary = new SplitOrderedList<TKey, KeyValuePair<TKey, TValue>>(_comparer);
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> pair)
		{
			return ContainsKey(pair.Key);
		}

		void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int startIndex)
		{
			CopyTo(array, startIndex);
		}

		public int Count
		{
			get { return _internalDictionary.Count; }
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
		{
			get { return false; }
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> pair)
		{
			return Remove(pair.Key);
		}

		void IDictionary.Add(object key, object value)
		{
			if (!(key is TKey) || !(value is TValue))
				throw new ArgumentException("key or value aren't of correct type");

			Add((TKey)key, (TValue)value);
		}

		bool IDictionary.Contains(object key)
		{
			if (!(key is TKey))
				return false;

			return ContainsKey((TKey)key);
		}

		IDictionaryEnumerator IDictionary.GetEnumerator()
		{
			return new ConcurrentDictionaryEnumerator(GetEnumeratorInternal());
		}

		bool IDictionary.IsFixedSize
		{
			get { return false; }
		}

		bool IDictionary.IsReadOnly
		{
			get { return false; }
		}

		object IDictionary.this[object key]
		{
			get
			{
				if (!(key is TKey))
					throw new ArgumentException("key isn't of correct type", "key");

				return this[(TKey)key];
			}
			set
			{
				if (!(key is TKey) || !(value is TValue))
					throw new ArgumentException("key or value aren't of correct type");

				this[(TKey)key] = (TValue)value;
			}
		}

		ICollection IDictionary.Keys
		{
			get { return (ICollection)Keys; }
		}

		void IDictionary.Remove(object key)
		{
			if (!(key is TKey))
				return;

			Remove((TKey)key);
		}

		ICollection IDictionary.Values
		{
			get { return (ICollection)Values; }
		}

		void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
		{
			Add(key, value);
		}

		public bool ContainsKey(TKey key)
		{
			CheckKey(key);
			KeyValuePair<TKey, TValue> dummy;
			return _internalDictionary.Find(Hash(key), key, out dummy);
		}

		public TValue this[TKey key]
		{
			get { return GetValue(key); }
			set { AddOrUpdate(key, value, value); }
		}

		public ICollection<TKey> Keys
		{
			get { return GetPart(kvp => kvp.Key); }
		}

		bool IDictionary<TKey, TValue>.Remove(TKey key)
		{
			return Remove(key);
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			CheckKey(key);
			KeyValuePair<TKey, TValue> pair;
			bool result = _internalDictionary.Find(Hash(key), key, out pair);
			value = pair.Value;

			return result;
		}

		public ICollection<TValue> Values
		{
			get { return GetPart(kvp => kvp.Value); }
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumeratorInternal();
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return GetEnumeratorInternal();
		}

		public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
		{
			CheckKey(key);
			if (addValueFactory == null)
				throw new ArgumentNullException("addValueFactory");
			if (updateValueFactory == null)
				throw new ArgumentNullException("updateValueFactory");
			return _internalDictionary.InsertOrUpdate(Hash(key), key, () => Make(key, addValueFactory(key)), e => Make(key, updateValueFactory(key, e.Value))).Value;
		}

		public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
		{
			return AddOrUpdate(key, _ => addValue, updateValueFactory);
		}

		public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
		{
			CheckKey(key);
			return _internalDictionary.InsertOrGet(Hash(key), key, Make(key, default(TValue)), () => Make(key, valueFactory(key))).Value;
		}

		public TValue GetOrAdd(TKey key, TValue value)
		{
			CheckKey(key);
			return _internalDictionary.InsertOrGet(Hash(key), key, Make(key, value), null).Value;
		}

		public KeyValuePair<TKey, TValue>[] ToArray()
		{
			// This is most certainly not optimum but there is
			// not a lot of possibilities

			return new List<KeyValuePair<TKey, TValue>>(this).ToArray();
		}

		public bool TryAdd(TKey key, TValue value)
		{
			CheckKey(key);
			return _internalDictionary.Insert(Hash(key), key, Make(key, value));
		}

		public bool TryRemove(TKey key, out TValue value)
		{
			CheckKey(key);
			KeyValuePair<TKey, TValue> data;
			bool result = _internalDictionary.Delete(Hash(key), key, out data);
			value = data.Value;
			return result;
		}

		public bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue)
		{
			CheckKey(key);
			return _internalDictionary.CompareExchange(Hash(key), key, Make(key, newValue), e => e.Value.Equals(comparisonValue));
		}

		void Add(TKey key, TValue value)
		{
			while (!TryAdd(key, value))
				;
		}

		TValue AddOrUpdate(TKey key, TValue addValue, TValue updateValue)
		{
			CheckKey(key);
			return _internalDictionary.InsertOrUpdate(Hash(key), key, Make(key, addValue), Make(key, updateValue)).Value;
		}

		void CheckKey(TKey key)
		{
			if (key == null)
				throw new ArgumentNullException("key");
		}

		void CopyTo(KeyValuePair<TKey, TValue>[] array, int startIndex)
		{
			CopyTo(array, startIndex, Count);
		}

		void CopyTo(KeyValuePair<TKey, TValue>[] array, int startIndex, int num)
		{
			foreach (KeyValuePair<TKey, TValue> kvp in this)
			{
				array[startIndex++] = kvp;

				if (--num <= 0)
					return;
			}
		}

		IEnumerator<KeyValuePair<TKey, TValue>> GetEnumeratorInternal()
		{
			return _internalDictionary.GetEnumerator();
		}

		ICollection<T> GetPart<T>(Func<KeyValuePair<TKey, TValue>, T> extractor)
		{
			var temp = new List<T>();

			foreach (KeyValuePair<TKey, TValue> kvp in this)
				temp.Add(extractor(kvp));

			return new ReadOnlyCollection<T>(temp);
		}

		TValue GetValue(TKey key)
		{
			TValue temp;
			if (!TryGetValue(key, out temp))
				throw new KeyNotFoundException(key.ToString());
			return temp;
		}

		uint Hash(TKey key)
		{
			return (uint)_comparer.GetHashCode(key);
		}

		static KeyValuePair<T, V> Make<T, V>(T key, V value)
		{
			return new KeyValuePair<T, V>(key, value);
		}

		bool Remove(TKey key)
		{
			TValue dummy;

			return TryRemove(key, out dummy);
		}

		class ConcurrentDictionaryEnumerator : IDictionaryEnumerator
		{
			readonly IEnumerator<KeyValuePair<TKey, TValue>> _internalEnum;

			public ConcurrentDictionaryEnumerator(IEnumerator<KeyValuePair<TKey, TValue>> internalEnum)
			{
				_internalEnum = internalEnum;
			}

			public DictionaryEntry Entry
			{
				get
				{
					KeyValuePair<TKey, TValue> current = _internalEnum.Current;
					return new DictionaryEntry(current.Key, current.Value);
				}
			}

			public object Key
			{
				get { return _internalEnum.Current.Key; }
			}

			public object Value
			{
				get { return _internalEnum.Current.Value; }
			}

			public object Current
			{
				get { return Entry; }
			}

			public bool MoveNext()
			{
				return _internalEnum.MoveNext();
			}

			public void Reset()
			{
				_internalEnum.Reset();
			}
		}
	}
}