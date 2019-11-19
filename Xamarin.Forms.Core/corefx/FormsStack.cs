// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*=============================================================================
**
**
** Purpose: An array implementation of a generic stack.
**
**
=============================================================================*/

/*
 * Copied from https://raw.githubusercontent.com/dotnet/corefx/9cf92cbef7cf5fcf46a1b556f9c6250e67d421ab/src/System.Collections/src/System/Collections/Generic/Stack.cs
 * Pre C# 8.0 features
 * */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Xamarin.Forms
{
	[DebuggerDisplay("Count = {Count}")]
	internal sealed class FormsStack<T> : IEnumerable<T>,
			System.Collections.ICollection,
			IReadOnlyCollection<T>
	{
		T[] _array; // Storage for stack elements. Do not rename (binary serialization)
		int _size; // Number of items in the stack. Do not rename (binary serialization)
		int _version; // Used to keep enumerator in sync w/ collection. Do not rename (binary serialization)

		private const int DefaultCapacity = 4;
		bool _isReference;


		public FormsStack()
		{
			_array = new T[0];
#if NETSTANDARD1_0
			_isReference = !typeof(T).GetTypeInfo().IsValueType;
#else
			_isReference = !typeof(T).IsValueType;
#endif
		}

		// Create a stack with a specific initial capacity.  The initial capacity
		// must be a non-negative number.
		public FormsStack(int capacity)
		{
			if (capacity < 0)
				throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "ArgumentOutOfRange_NeedNonNegNum");
			_array = new T[capacity];
		}

		// Fills a Stack with the contents of a particular collection.  The items are
		// pushed onto the stack in the same order they are read by the enumerator.
		public FormsStack(IEnumerable<T> collection)
		{
			if (collection == null)
				throw new ArgumentNullException(nameof(collection));
			_array = FormsEnumerableHelpers.ToArray(collection, out _size);
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

		// Removes all Objects from the Stack.
		public void Clear()
		{
			if (IsReference())
			{
				Array.Clear(_array, 0, _size); // Don't need to doc this but we clear the elements so that the gc can reclaim the references.
			}
			_size = 0;
			_version++;
		}

		public bool Contains(T item)
		{
			// Compare items using the default equality comparer

			// PERF: Internally Array.LastIndexOf calls
			// EqualityComparer<T>.Default.LastIndexOf, which
			// is specialized for different types. This
			// boosts performance since instead of making a
			// virtual method call each iteration of the loop,
			// via EqualityComparer<T>.Default.Equals, we
			// only make one virtual call to EqualityComparer.LastIndexOf.

			return _size != 0 && Array.LastIndexOf(_array, item, _size - 1) != -1;
		}

		// Copies the stack into an array.
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

			if (array.Length - arrayIndex < _size)
			{
				throw new ArgumentException("Argument_InvalidOffLen");
			}

			Debug.Assert(array != _array);
			int srcIndex = 0;
			int dstIndex = arrayIndex + _size;
			while (srcIndex < _size)
			{
				array[--dstIndex] = _array[srcIndex++];
			}
		}

		void ICollection.CopyTo(Array array, int arrayIndex)
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

			if (arrayIndex < 0 || arrayIndex > array.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(arrayIndex), arrayIndex, "ArgumentOutOfRange_Index");
			}

			if (array.Length - arrayIndex < _size)
			{
				throw new ArgumentException("Argument_InvalidOffLen");
			}

			try
			{
				Array.Copy(_array, 0, array, arrayIndex, _size);
				Array.Reverse(array, arrayIndex, _size);
			}
			catch (ArrayTypeMismatchException)
			{
				throw new ArgumentException("Argument_InvalidArrayType", nameof(array));
			}
		}

		// Returns an IEnumerator for this Stack.
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

		public void TrimExcess()
		{
			int threshold = (int)(((double)_array.Length) * 0.9);
			if (_size < threshold)
			{
				Array.Resize(ref _array, _size);
				_version++;
			}
		}

		// Returns the top object on the stack without removing it.  If the stack
		// is empty, Peek throws an InvalidOperationException.
		public T Peek()
		{
			int size = _size - 1;
			T[] array = _array;

			if ((uint)size >= (uint)array.Length)
			{
				ThrowForEmptyStack();
			}

			return array[size];
		}

		public bool TryPeek(out T result)
		{
			int size = _size - 1;
			T[] array = _array;

			if ((uint)size >= (uint)array.Length)
			{
				result = default(T);
				return false;
			}
			result = array[size];
			return true;
		}

		// Pops an item from the top of the stack.  If the stack is empty, Pop
		// throws an InvalidOperationException.
		public T Pop()
		{
			int size = _size - 1;
			T[] array = _array;

			// if (_size == 0) is equivalent to if (size == -1), and this case
			// is covered with (uint)size, thus allowing bounds check elimination
			// https://github.com/dotnet/coreclr/pull/9773
			if ((uint)size >= (uint)array.Length)
			{
				ThrowForEmptyStack();
			}

			_version++;
			_size = size;
			T item = array[size];
			if (IsReference())
			{
				array[size] = default(T);     // Free memory quicker.
			}
			return item;
		}

		public bool TryPop(out T result)
		{
			int size = _size - 1;
			T[] array = _array;

			if ((uint)size >= (uint)array.Length)
			{
				result = default(T);
				return false;
			}

			_version++;
			_size = size;
			result = array[size];
			if (IsReference())
			{
				array[size] = default(T);
			}
			return true;
		}

		// Pushes an item to the top of the stack.
		public void Push(T item)
		{
			int size = _size;
			T[] array = _array;

			if ((uint)size < (uint)array.Length)
			{
				array[size] = item;
				_version++;
				_size = size + 1;
			}
			else
			{
				PushWithResize(item);
			}
		}

		// Non-inline from Stack.Push to improve its code quality as uncommon path
		[MethodImpl(MethodImplOptions.NoInlining)]
		private void PushWithResize(T item)
		{
			Array.Resize(ref _array, (_array.Length == 0) ? DefaultCapacity : 2 * _array.Length);
			_array[_size] = item;
			_version++;
			_size++;
		}

		// Copies the Stack to an array, in the same order Pop would return the items.
		public T[] ToArray()
		{
			if (_size == 0)
				return new T[0];

			T[] objArray = new T[_size];
			int i = 0;
			while (i < _size)
			{
				objArray[i] = _array[_size - i - 1];
				i++;
			}
			return objArray;
		}

		private void ThrowForEmptyStack()
		{
			Debug.Assert(_size == 0);
			throw new InvalidOperationException("InvalidOperation_EmptyStack");
		}

		// The original version of this is IsReferenceOrContainsReferences
		// But I don't think we have to satisfy the Contains reference portion of the check
		public bool IsReference()
		{
			return _isReference;
		}

		[SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "not an expected scenario")]
		public struct Enumerator : IEnumerator<T>, System.Collections.IEnumerator
		{
			private readonly FormsStack<T> _stack;
			private readonly int _version;
			private int _index;
			private T _currentElement;

			internal Enumerator(FormsStack<T> stack)
			{
				_stack = stack;
				_version = stack._version;
				_index = -2;
				_currentElement = default;
			}

			public void Dispose()
			{
				_index = -1;
			}

			public bool MoveNext()
			{
				bool retval;
				if (_version != _stack._version)
					throw new InvalidOperationException("InvalidOperation_EnumFailedVersion");
				if (_index == -2)
				{  // First call to enumerator.
					_index = _stack._size - 1;
					retval = (_index >= 0);
					if (retval)
						_currentElement = _stack._array[_index];
					return retval;
				}
				if (_index == -1)
				{  // End of enumeration.
					return false;
				}

				retval = (--_index >= 0);
				if (retval)
					_currentElement = _stack._array[_index];
				else
					_currentElement = default;
				return retval;
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
				throw new InvalidOperationException(_index == -2 ? "InvalidOperation_EnumNotStarted" : "InvalidOperation_EnumEnded");
			}

			object System.Collections.IEnumerator.Current
			{
				get { return Current; }
			}

			void IEnumerator.Reset()
			{
				if (_version != _stack._version)
					throw new InvalidOperationException("InvalidOperation_EnumFailedVersion");
				_index = -2;
				_currentElement = default;
			}
		}



	}

	/// <summary>
	/// Internal helper functions for working with enumerables.
	/// </summary>
	internal static partial class FormsEnumerableHelpers
	{
		/// <summary>Converts an enumerable to an array using the same logic as List{T}.</summary>
		/// <param name="source">The enumerable to convert.</param>
		/// <param name="length">The number of items stored in the resulting array, 0-indexed.</param>
		/// <returns>
		/// The resulting array.  The length of the array may be greater than <paramref name="length"/>,
		/// which is the actual number of elements in the array.
		/// </returns>
		internal static T[] ToArray<T>(IEnumerable<T> source, out int length)
		{
			if (source is ICollection<T> ic)
			{
				int count = ic.Count;
				if (count != 0)
				{
					// Allocate an array of the desired size, then copy the elements into it. Note that this has the same
					// issue regarding concurrency as other existing collections like List<T>. If the collection size
					// concurrently changes between the array allocation and the CopyTo, we could end up either getting an
					// exception from overrunning the array (if the size went up) or we could end up not filling as many
					// items as 'count' suggests (if the size went down).  This is only an issue for concurrent collections
					// that implement ICollection<T>, which as of .NET 4.6 is just ConcurrentDictionary<TKey, TValue>.
					T[] arr = new T[count];
					ic.CopyTo(arr, 0);
					length = count;
					return arr;
				}
			}
			else
			{
				using (var en = source.GetEnumerator())
				{
					if (en.MoveNext())
					{
						const int DefaultCapacity = 4;
						T[] arr = new T[DefaultCapacity];
						arr[0] = en.Current;
						int count = 1;

						while (en.MoveNext())
						{
							if (count == arr.Length)
							{
								// MaxArrayLength is defined in Array.MaxArrayLength and in gchelpers in CoreCLR.
								// It represents the maximum number of elements that can be in an array where
								// the size of the element is greater than one byte; a separate, slightly larger constant,
								// is used when the size of the element is one.
								const int MaxArrayLength = 0x7FEFFFFF;

								// This is the same growth logic as in List<T>:
								// If the array is currently empty, we make it a default size.  Otherwise, we attempt to
								// double the size of the array.  Doubling will overflow once the size of the array reaches
								// 2^30, since doubling to 2^31 is 1 larger than Int32.MaxValue.  In that case, we instead
								// constrain the length to be MaxArrayLength (this overflow check works because of the
								// cast to uint).  Because a slightly larger constant is used when T is one byte in size, we
								// could then end up in a situation where arr.Length is MaxArrayLength or slightly larger, such
								// that we constrain newLength to be MaxArrayLength but the needed number of elements is actually
								// larger than that.  For that case, we then ensure that the newLength is large enough to hold
								// the desired capacity.  This does mean that in the very rare case where we've grown to such a
								// large size, each new element added after MaxArrayLength will end up doing a resize.
								int newLength = count << 1;
								if ((uint)newLength > MaxArrayLength)
								{
									newLength = MaxArrayLength <= count ? count + 1 : MaxArrayLength;
								}

								Array.Resize(ref arr, newLength);
							}

							arr[count++] = en.Current;
						}

						length = count;
						return arr;
					}
				}
			}

			length = 0;
			return new T[0];
		}
	}
}
