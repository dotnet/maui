#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Maui;

/// <summary>
/// A List type for holding WeakReference's
/// It clears the underlying List based on a threshold of operations: Add() or GetEnumerator()
/// </summary>
class WeakList<T> : IEnumerable<T> where T : class
{
	readonly List<WeakReference<T>> _list = new();
	int _operations;

	public int Count => _list.Count;

	public int CleanupThreshold { get; set; } = 32;

	public void Add(T item)
	{
		CleanupIfNeeded();
		_list.Add(new WeakReference<T>(item));
	}

	public void Remove(T item)
	{
		WeakReference<T> w;

		// A reverse for-loop, means we can call RemoveAt(i) inside the loop
		for (int i = _list.Count - 1; i >= 0; i--)
		{
			w = _list[i];
			if (w.TryGetTarget(out T? target))
			{
				if (target == item)
				{
					_list.RemoveAt(i);
					break;
				}
			}
			else
			{
				// Remove if we found one that is not alive
				_list.RemoveAt(i);
			}
		}
	}

	public void Clear()
	{
		_list.Clear();
		_operations = 0;
	}

	public IEnumerator<T> GetEnumerator()
	{
		CleanupIfNeeded();

		foreach (var w in _list)
			if (w.TryGetTarget(out T? item))
				yield return item;
	}

	void CleanupIfNeeded()
	{
		if (++_operations > CleanupThreshold)
		{
			_operations = 0;
			_list.RemoveAll(w => !w.TryGetTarget(out _));
		}
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
