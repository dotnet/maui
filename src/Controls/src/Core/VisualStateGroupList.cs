#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// A list of <see cref="VisualStateGroup"/> objects that enforces unique group and state names.
	/// </summary>
	public class VisualStateGroupList : IList<VisualStateGroup>
	{
		readonly IList<VisualStateGroup> _internalList;
		internal bool IsDefault { get; private set; }

		// Used to check for duplicate names; we keep it around because it's cheaper to create it once and clear it
		// than to create one every time we need to validate
		readonly HashSet<string> _names = new HashSet<string>(StringComparer.Ordinal);

		void Validate(IList<VisualStateGroup> groups)
		{
			var groupCount = groups.Count;

			// If we only have 1 group, no need to worry about duplicate group names
			if (groupCount > 1)
			{
				_names.Clear();

				// Using a for loop to avoid allocating an enumerator
				for (int n = 0; n < groupCount; n++)
				{
					// HashSet will return false if the string is already in the set
					if (!_names.Add(groups[n].Name))
					{
						throw new InvalidOperationException("VisualStateGroup Names must be unique");
					}
				}
			}

			// State names must be unique within this group list, so we'll iterate over all the groups
			// and their states and add the state names to a HashSet; we throw an exception if a duplicate shows up

			_names.Clear();

			// Using nested for loops to avoid allocating enumerators
			for (int groupIndex = 0; groupIndex < groupCount; groupIndex++)
			{
				// Cache the group lookup and states count; it's ugly, but it speeds things up a lot
				var group = groups[groupIndex];
				group.VisualElement = VisualElement;
				group.UpdateStateTriggers();

				var stateCount = group.States.Count;

				for (int stateIndex = 0; stateIndex < stateCount; stateIndex++)
				{
					// HashSet will return false if the string is already in the set
					if (!_names.Add(group.States[stateIndex].Name))
					{
						throw new InvalidOperationException("VisualState Names must be unique");
					}
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="VisualStateGroupList"/> class.
		/// </summary>
		public VisualStateGroupList() : this(false)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="VisualStateGroupList"/> class with the specified default state.
		/// </summary>
		/// <param name="isDefault">Indicates whether this list represents the default visual state groups.</param>
		public VisualStateGroupList(bool isDefault)
		{
			IsDefault = isDefault;
			_internalList = new WatchAddList<VisualStateGroup>(ValidateAndNotify);
		}

		void ValidateAndNotify(object sender, EventArgs eventArgs)
		{
			ValidateAndNotify(_internalList);
		}

		void ValidateAndNotify(IList<VisualStateGroup> groups)
		{
			if (groups.Count > 0)
				IsDefault = false;

			Validate(groups);
			OnStatesChanged();
		}

		/// <inheritdoc />
		public IEnumerator<VisualStateGroup> GetEnumerator()
		{
			return _internalList.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)_internalList).GetEnumerator();
		}

		/// <inheritdoc />
		public void Add(VisualStateGroup item)
		{
			if (item == null)
			{
				throw new ArgumentNullException(nameof(item));
			}

			_internalList.Add(item);

			item.StatesChanged += ValidateAndNotify;
		}

		/// <inheritdoc />
		public void Clear()
		{
			foreach (var group in _internalList)
			{
				group.StatesChanged -= ValidateAndNotify;
			}

			_internalList.Clear();
		}

		/// <inheritdoc />
		public bool Contains(VisualStateGroup item)
		{
			return _internalList.Contains(item);
		}

		/// <inheritdoc />
		public void CopyTo(VisualStateGroup[] array, int arrayIndex)
		{
			_internalList.CopyTo(array, arrayIndex);
		}

		/// <inheritdoc />
		public bool Remove(VisualStateGroup item)
		{
			if (item == null)
			{
				throw new ArgumentNullException(nameof(item));
			}

			item.StatesChanged -= ValidateAndNotify;
			return _internalList.Remove(item);
		}

		/// <inheritdoc />
		public int Count => _internalList.Count;

		/// <inheritdoc />
		public bool IsReadOnly => false;

		/// <inheritdoc />
		public int IndexOf(VisualStateGroup item)
		{
			return _internalList.IndexOf(item);
		}

		/// <inheritdoc />
		public void Insert(int index, VisualStateGroup item)
		{
			if (item == null)
			{
				throw new ArgumentNullException(nameof(item));
			}

			item.StatesChanged += ValidateAndNotify;
			_internalList.Insert(index, item);
		}

		/// <inheritdoc />
		public void RemoveAt(int index)
		{
			_internalList[index].StatesChanged -= ValidateAndNotify;
			_internalList.RemoveAt(index);
		}

		public VisualStateGroup this[int index]
		{
			get => _internalList[index];
			set => _internalList[index] = value;
		}

		WeakReference<VisualElement> _visualElement;
		internal VisualElement VisualElement
		{
			get
			{
				if (_visualElement == null)
					return null;
				_visualElement.TryGetTarget(out var ve);
				return ve;
			}
			set
			{
				_visualElement = new WeakReference<VisualElement>(value);
			}
		}

		internal SetterSpecificity Specificity { get; set; }

		void OnStatesChanged()
		{
			VisualElement?.ChangeVisualState();
		}

		public override bool Equals(object obj) => Equals(obj as VisualStateGroupList);
		bool Equals(VisualStateGroupList other)
		{
			if (other is null)
				return false;
			if (Object.ReferenceEquals(this, other))
				return true;
			if (Count != other.Count)
				return false;
			for (var i = 0; i < Count; i++)
				if (!this[i].Equals(other[i]))
					return false;
			return true;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hash = 41;
				for (var i = 0; i < Count; i++)
					hash = (hash * 43) ^ this[i].GetHashCode();
				return hash;
			}
		}

	}
}
