// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*=============================================================================
**
**
** Purpose: A circular-array implementation of a generic queue.
**
**
=============================================================================*/

/*
 * Copied from https://raw.githubusercontent.com/dotnet/corefx/9cf92cbef7cf5fcf46a1b556f9c6250e67d421ab/src/System.Collections/src/System/Collections/Generic/Queue.cs
 * Pre C# 8.0 features
 * */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Xamarin.Forms
{
	// A simple Queue of generic objects.  Internally it is implemented as a 
	// circular buffer, so Enqueue can be O(n).  Dequeue is O(1).
	[DebuggerDisplay("Count = {Count}")]
	internal sealed class FormsQueue<T> : IEnumerable<T>,
		System.Collections.ICollection,
		IReadOnlyCollection<T>
	{
		private T[] _array;
		private int _head;       // The index from which to dequeue if the queue isn't empty.
		private int _tail;       // The index at which to enqueue if the queue isn't full.
		private int _size;       // Number of elements.
		private int _version;
		bool _isReference;

		private const int MinimumGrow = 4;
		private const int GrowFactor = 200;  // double each time

		// Creates a queue with room for capacity objects. The default initial
		// capacity and grow factor are used.
		public FormsQueue()
		{
			Init();
			_array = new T[0];
		}

		// Creates a queue with room for capacity objects. The default grow factor
		// is used.
		public FormsQueue(int capacity)
		{
			Init();
			if (capacity < 0)
				throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "ArgumentOutOfRange_NeedNonNegNum");
			_array = new T[capacity];
		}

		// Fills a Queue with the elements of an ICollection.  Uses the enumerator
		// to get each of the elements.
		public FormsQueue(IEnumerable<T> collection)
		{
			Init();
			if (collection == null)
				throw new ArgumentNullException(nameof(collection));

			_array = FormsEnumerableHelpers.ToArray(collection, out _size);
			if (_size != _array.Length)
				_tail = _size;
		}
		void Init()
		{

#if NETSTANDARD1_0
			_isReference = !typeof(T).GetTypeInfo().IsValueType;
#else
			_isReference = !typeof(T).IsValueType;
#endif
		}
		// The original version of this is IsReferenceOrContainsReferences
		// But I don't think we have to satisfy the Contains reference portion of the check
		public bool IsReference()
		{
			return _isReference;
		}

		public int Count
		{
			get { return _size; }
		}

		bool ICollection.IsSynchronized
		{
			get { return false; }
		}

		object ICollection.SyncRoot => this;

		// Removes all Objects from the queue.
		public void Clear()
		{
			if (_size != 0)
			{
				if (IsReference())
				{
					if (_head < _tail)
					{
						Array.Clear(_array, _head, _size);
					}
					else
					{
						Array.Clear(_array, _head, _array.Length - _head);
						Array.Clear(_array, 0, _tail);
					}
				}

				_size = 0;
			}

			_head = 0;
			_tail = 0;
			_version++;
		}

		// CopyTo copies a collection into an Array, starting at a particular
		// index into the array.
		public void CopyTo(T[] array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException(nameof(array));
			}

			if (arrayIndex < 0 || arrayIndex > array.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(arrayIndex), arrayIndex, "ArgumentOutOfRange_Index");
			}

			int arrayLen = array.Length;
			if (arrayLen - arrayIndex < _size)
			{
				throw new ArgumentException("Argument_InvalidOffLen");
			}

			int numToCopy = _size;
			if (numToCopy == 0)
				return;

			int firstPart = Math.Min(_array.Length - _head, numToCopy);
			Array.Copy(_array, _head, array, arrayIndex, firstPart);
			numToCopy -= firstPart;
			if (numToCopy > 0)
			{
				Array.Copy(_array, 0, array, arrayIndex + _array.Length - _head, numToCopy);
			}
		}

		void ICollection.CopyTo(Array array, int index)
		{
			if (array == null)
			{
				throw new ArgumentNullException(nameof(array));
			}

			if (array.Rank != 1)
			{
				throw new ArgumentException("Arg_RankMultiDimNotSupported", nameof(array));
			}

			if (array.GetLowerBound(0) != 0)
			{
				throw new ArgumentException("Arg_NonZeroLowerBound", nameof(array));
			}

			int arrayLen = array.Length;
			if (index < 0 || index > arrayLen)
			{
				throw new ArgumentOutOfRangeException(nameof(index), index, "ArgumentOutOfRange_Index");
			}

			if (arrayLen - index < _size)
			{
				throw new ArgumentException("Argument_InvalidOffLen");
			}

			int numToCopy = _size;
			if (numToCopy == 0)
				return;

			try
			{
				int firstPart = (_array.Length - _head < numToCopy) ? _array.Length - _head : numToCopy;
				Array.Copy(_array, _head, array, index, firstPart);
				numToCopy -= firstPart;

				if (numToCopy > 0)
				{
					Array.Copy(_array, 0, array, index + _array.Length - _head, numToCopy);
				}
			}
			catch (ArrayTypeMismatchException)
			{
				throw new ArgumentException("Argument_InvalidArrayType", nameof(array));
			}
		}

		// Adds item to the tail of the queue.
		public void Enqueue(T item)
		{
			if (_size == _array.Length)
			{
				int newcapacity = (int)((long)_array.Length * (long)GrowFactor / 100);
				if (newcapacity < _array.Length + MinimumGrow)
				{
					newcapacity = _array.Length + MinimumGrow;
				}
				SetCapacity(newcapacity);
			}

			_array[_tail] = item;
			MoveNext(ref _tail);
			_size++;
			_version++;
		}

		// GetEnumerator returns an IEnumerator over this Queue.  This
		// Enumerator will support removing.
		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		/// <internalonly/>
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return new Enumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new Enumerator(this);
		}

		// Removes the object at the head of the queue and returns it. If the queue
		// is empty, this method throws an 
		// InvalidOperationException.
		public T Dequeue()
		{
			int head = _head;
			T[] array = _array;

			if (_size == 0)
			{
				ThrowForEmptyQueue();
			}

			T removed = array[head];
			if (IsReference())
			{
				array[head] = default;
			}
			MoveNext(ref _head);
			_size--;
			_version++;
			return removed;
		}

		public bool TryDequeue(out T result)
		{
			int head = _head;
			T[] array = _array;

			if (_size == 0)
			{
				result = default;
				return false;
			}

			result = array[head];
			if (IsReference())
			{
				array[head] = default;
			}
			MoveNext(ref _head);
			_size--;
			_version++;
			return true;
		}

		// Returns the object at the head of the queue. The object remains in the
		// queue. If the queue is empty, this method throws an 
		// InvalidOperationException.
		public T Peek()
		{
			if (_size == 0)
			{
				ThrowForEmptyQueue();
			}

			return _array[_head];
		}

		public bool TryPeek(out T result)
		{
			if (_size == 0)
			{
				result = default(T);
				return false;
			}

			result = _array[_head];
			return true;
		}

		// Returns true if the queue contains at least one object equal to item.
		// Equality is determined using EqualityComparer<T>.Default.Equals().
		public bool Contains(T item)
		{
			if (_size == 0)
			{
				return false;
			}

			if (_head < _tail)
			{
				return Array.IndexOf(_array, item, _head, _size) >= 0;
			}

			// We've wrapped around. Check both partitions, the least recently enqueued first.
			return
				Array.IndexOf(_array, item, _head, _array.Length - _head) >= 0 ||
				Array.IndexOf(_array, item, 0, _tail) >= 0;
		}

		// Iterates over the objects in the queue, returning an array of the
		// objects in the Queue, or an empty array if the queue is empty.
		// The order of elements in the array is first in to last in, the same
		// order produced by successive calls to Dequeue.
		public T[] ToArray()
		{
			if (_size == 0)
			{
				return new T[0];
			}

			T[] arr = new T[_size];

			if (_head < _tail)
			{
				Array.Copy(_array, _head, arr, 0, _size);
			}
			else
			{
				Array.Copy(_array, _head, arr, 0, _array.Length - _head);
				Array.Copy(_array, 0, arr, _array.Length - _head, _tail);
			}

			return arr;
		}

		// PRIVATE Grows or shrinks the buffer to hold capacity objects. Capacity
		// must be >= _size.
		private void SetCapacity(int capacity)
		{
			T[] newarray = new T[capacity];
			if (_size > 0)
			{
				if (_head < _tail)
				{
					Array.Copy(_array, _head, newarray, 0, _size);
				}
				else
				{
					Array.Copy(_array, _head, newarray, 0, _array.Length - _head);
					Array.Copy(_array, 0, newarray, _array.Length - _head, _tail);
				}
			}

			_array = newarray;
			_head = 0;
			_tail = (_size == capacity) ? 0 : _size;
			_version++;
		}

		// Increments the index wrapping it if necessary.
		private void MoveNext(ref int index)
		{
			// It is tempting to use the remainder operator here but it is actually much slower
			// than a simple comparison and a rarely taken branch.
			// JIT produces better code than with ternary operator ?:
			int tmp = index + 1;
			if (tmp == _array.Length)
			{
				tmp = 0;
			}
			index = tmp;
		}

		private void ThrowForEmptyQueue()
		{
			Debug.Assert(_size == 0);
			throw new InvalidOperationException("InvalidOperation_EmptyQueue");
		}

		public void TrimExcess()
		{
			int threshold = (int)(((double)_array.Length) * 0.9);
			if (_size < threshold)
			{
				SetCapacity(_size);
			}
		}

		// Implements an enumerator for a Queue.  The enumerator uses the
		// internal version number of the list to ensure that no modifications are
		// made to the list while an enumeration is in progress.
		[SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "not an expected scenario")]
		public struct Enumerator : IEnumerator<T>,
			System.Collections.IEnumerator
		{
			private readonly FormsQueue<T> _q;
			private readonly int _version;
			private int _index;   // -1 = not started, -2 = ended/disposed
			private T _currentElement;

			internal Enumerator(FormsQueue<T> q)
			{
				_q = q;
				_version = q._version;
				_index = -1;
				_currentElement = default(T);
			}

			public void Dispose()
			{
				_index = -2;
				_currentElement = default(T);
			}

			public bool MoveNext()
			{
				if (_version != _q._version)
					throw new InvalidOperationException("InvalidOperation_EnumFailedVersion");

				if (_index == -2)
					return false;

				_index++;

				if (_index == _q._size)
				{
					// We've run past the last element
					_index = -2;
					_currentElement = default(T);
					return false;
				}

				// Cache some fields in locals to decrease code size
				T[] array = _q._array;
				int capacity = array.Length;

				// _index represents the 0-based index into the queue, however the queue
				// doesn't have to start from 0 and it may not even be stored contiguously in memory.

				int arrayIndex = _q._head + _index; // this is the actual index into the queue's backing array
				if (arrayIndex >= capacity)
				{
					// NOTE: Originally we were using the modulo operator here, however
					// on Intel processors it has a very high instruction latency which
					// was slowing down the loop quite a bit.
					// Replacing it with simple comparison/subtraction operations sped up
					// the average foreach loop by 2x.

					arrayIndex -= capacity; // wrap around if needed
				}

				_currentElement = array[arrayIndex];
				return true;
			}

			public T Current
			{
				get
				{
					if (_index < 0)
						ThrowEnumerationNotStartedOrEnded();
					return _currentElement;
				}
			}

			private void ThrowEnumerationNotStartedOrEnded()
			{
				Debug.Assert(_index == -1 || _index == -2);
				throw new InvalidOperationException(_index == -1 ? "InvalidOperation_EnumNotStarted" : "InvalidOperation_EnumEnded");
			}

			object IEnumerator.Current
			{
				get { return Current; }
			}

			void IEnumerator.Reset()
			{
				if (_version != _q._version)
					throw new InvalidOperationException("InvalidOperation_EnumFailedVersion");
				_index = -1;
				_currentElement = default(T);
			}
		}
	}
}
