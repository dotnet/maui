using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Class for managing up to two Specificity values, and falling back to a SortedList once three values are present.
	/// This yields better performance in cases where a BP has one or two Specificity values set.
	/// </summary>
	internal class SetterSpecificityList
	{
		KeyValuePair<SetterSpecificity, object>? _first;
		KeyValuePair<SetterSpecificity, object>? _second;
		SortedList<SetterSpecificity, object>? _values;

		public object this[SetterSpecificity key]
		{
			set => SetValue(key, value);
		}

		public void SetValue(SetterSpecificity specificity, object value)
		{
			if (_first is null || _first.Value.Key == specificity)
			{
				_first = new KeyValuePair<SetterSpecificity, object>(specificity, value);
				if (_values is not null)
					_values[specificity] = value;
				return;
			}

			if (_second is null || _second.Value.Key == specificity)
			{
				_second = new KeyValuePair<SetterSpecificity, object>(specificity, value);
				if (_values is not null)
					_values[specificity] = value;
				return;
			}

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

		public void Remove(SetterSpecificity specificity)
		{
			_values?.Remove(specificity);
			if (_first is not null && _first.Value.Key == specificity)
				_first = null;
			if (_second is not null && _second.Value.Key == specificity)
				_second = null;
		}

		public KeyValuePair<SetterSpecificity, object> GetSpecificityAndValue()
		{
			// Slow path calls SortedList.Last()
			if (_values is not null)
				return _values.Last();

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
		/// Called by ClearValueCore, returns what the value would be if cleared
		/// </summary>
		public object? GetClearedValue()
		{
			if (_values is not null)
			{
				return _values.Count >= 2 ? _values[_values.Keys[_values.Count - 2]] : null;
			}

			// Fast path should return the "lower" value
			if (_first is not null && _second is not null)
			{
				if (_second.Value.Key.CompareTo(_first.Value.Key) >= 0)
				{
					return _first.Value.Value;
				}
				else
				{
					return _second.Value.Value;
				}
			}
			else if (_first is not null)
			{
				return _first.Value.Value;
			}
			else if (_second is not null)
			{
				return _second.Value.Value;
			}

			return null;
		}
	}
}
