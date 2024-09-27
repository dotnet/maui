#nullable disable

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Stores values for a property with different specificities.
	/// </summary>
	internal class SetterSpecificityList<T> where T : class
	{
	    const int CapacityDelta = 3;
	    
	    SetterSpecificity[] _keys;
	    T[] _values;
	    int _count;
	    
	    public int Count => _count;

		object _lock = new();

		public object this[SetterSpecificity key]
		{
			set => SetValue(key, value);
		}

		public void SetValue(SetterSpecificity specificity, object value)
		{
			if (_first is null || _first.Value.Key == specificity)
			{
				_first = new KeyValuePair<SetterSpecificity, object>(specificity, value);
				lock(_lock) {
					if (_values is not null)
							_values[specificity] = value;
				}
				return;
			}

			if (_second is null || _second.Value.Key == specificity)
			{
				_second = new KeyValuePair<SetterSpecificity, object>(specificity, value);
				lock(_lock) {
					if (_values is not null)	
						_values[specificity] = value;
				}
				return;
			}

			lock(_lock) {
				if (_values is null)
				{
					_values = new()
					{
						[_first.Value.Key] = _first.Value.Value,
						[_second.Value.Key] = _second.Value.Value,
					};
					// Clear the fields, to reduce duplication in memory
					_first = null;
					_second = null;
				}
			
				_values[specificity] = value;
			}
		}

		public void Remove(SetterSpecificity specificity)
		{
			lock(_lock)
			{
				if (_values is not null)
					_values.Remove(specificity);
			}
			if (_first is not null && _first.Value.Key == specificity)
				_first = null;
			if (_second is not null && _second.Value.Key == specificity)
				_second = null;
		}

		public KeyValuePair<SetterSpecificity, object> GetSpecificityAndValue()
		{
			// Slow path calls SortedList.Last()
			lock(_lock) {
				if (_values is not null) {
					return _values.Last();
				}
			}
			// Fast path accesses _first and _second
			if (_first is not null && _second is not null)
			{
				if (_first.Value.Key.CompareTo(_second.Value.Key) >= 0)
				{
					return _first.Value;
				}
				else
				{
					return _second.Value;
				}
			}
			else if (_first is not null)
			{
				return _first.Value;
			}
			else if (_second is not null)
			{
				return _second.Value;
			}

			throw new InvalidOperationException("No BindablePropertyContext Value specified!");
		}

		/// <summary>
		/// Called by ClearValueCore, returns what the top value would be if cleared
		/// </summary>
		public object? GetClearedValue(SetterSpecificity clearedSpecificity)
		{
			lock(_lock) {
				if (_values is not null)
				{
					var index = _values.IndexOfKey(clearedSpecificity);
					if (index == _values.Count - 1) //last value will be cleared
						return _values.Count >= 2 ? _values[_values.Keys[_values.Count - 2]] : null;
					return _values.Last().Value;
				}
			}
	        return index < 0 ? default : _values[index];
	    }
			
	    /// <summary>
	    /// Returns what the SetterSpecificity would be if the current value was removed
	    /// </summary>
	    public SetterSpecificity GetClearedSpecificity()
	    {
	        var index = _count - 2;
	        return index < 0 ? default : _keys[index];
	    }
	    
	    /// <summary>
	    /// Gets the highest specificity and value
	    /// </summary>
	    /// <returns></returns>
	    public KeyValuePair<SetterSpecificity, T> GetSpecificityAndValue()
	    {
	        var index = _count - 1;
	        return index < 0 ? default : new KeyValuePair<SetterSpecificity, T>(_keys[index], _values[index]);
	    }

	    [MethodImpl(MethodImplOptions.AggressiveInlining)]
	    void SetValue(SetterSpecificity key, T value)
	    {
	        var count = _count;
	        var lo = 0;
	        var hi = count - 1;
	        while (lo <= hi)
	        {
	            var index = lo + ((hi - lo) >> 1);

	            var indexSpecificity = _keys[index];
	            if (indexSpecificity == key)
	            {
	                _values[index] = value;
	                return;
	            }
	            
	            if (indexSpecificity < key)
	            {
	                lo = index + 1;
	            }
	            else
	            {
	                hi = index - 1;
	            }
	        }

	        if (_keys.Length == count)
	        {
	            SetCapacity(count, count + CapacityDelta);
	        }

	        if (count > lo)
	        {
	            Array.Copy(_keys, lo, _keys, lo + 1, count - lo);
	            Array.Copy(_values, lo, _values, lo + 1, count - lo);
	        }

	        _keys[lo] = key;
	        _values[lo] = value;

	        ++_count;
	    }
	    
	    [MethodImpl(MethodImplOptions.AggressiveInlining)]
	    T GetValue(SetterSpecificity key)
	    {
	        var count = _count;
	        var lo = 0;
	        var hi = count - 1;
	        while (lo <= hi)
	        {
	            var index = lo + ((hi - lo) >> 1);

	            var indexSpecificity = _keys[index];
	            if (indexSpecificity == key)
	            {
	                return _values[index];
	            }
	            
	            if (indexSpecificity < key)
	            {
	                lo = index + 1;
	            }
	            else
	            {
	                hi = index - 1;
	            }
	        }

	        return default;
	    }
	    
	    public void Remove(SetterSpecificity key)
	    {
	        var count = _count;
	        var lo = 0;
	        var hi = count - 1;
	        while (lo <= hi)
	        {
	            var index = lo + ((hi - lo) >> 1);

	            var indexSpecificity = _keys[index];
	            if (indexSpecificity == key)
	            {
		            var nextIndex = index + 1;
	                if (nextIndex < count)
	                {
	                    Array.Copy(_keys, nextIndex, _keys, index, count - nextIndex);
	                    Array.Copy(_values, nextIndex, _values, index, count - nextIndex);
	                    _values[count - 1] = null;
	                }
	                else
	                {
		                _values[index] = null;
	                }
	                
	                --_count;
	                return;
	            }
	            
	            if (indexSpecificity < key)
	            {
	                lo = index + 1;
	            }
	            else
	            {
	                hi = index - 1;
	            }
	        }
	    }

	    [MethodImpl(MethodImplOptions.AggressiveInlining)]
	    void SetCapacity(int currentCapacity, int capacity)
	    {
	        var newKeys = new SetterSpecificity[capacity];
	        var newValues = new T[capacity];
	        if (currentCapacity > 0)
	        {
	            Array.Copy(_keys, newKeys, currentCapacity);
	            Array.Copy(_values, newValues, currentCapacity);
	        }
	        _keys = newKeys;
	        _values = newValues;
	    }
	}
}
