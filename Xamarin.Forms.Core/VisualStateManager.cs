using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms
{
	public static class VisualStateManager
	{
		internal class CommonStates
		{
			public const string Normal = "Normal";
			public const string Disabled = "Disabled";
			public const string Focused = "Focused";
		}

		public static readonly BindableProperty VisualStateGroupsProperty =
			BindableProperty.CreateAttached("VisualStateGroups", typeof(VisualStateGroupList), typeof(VisualElement), 
				defaultValue: null, propertyChanged: VisualStateGroupsPropertyChanged, 
				defaultValueCreator: bindable => new VisualStateGroupList {VisualElement = (VisualElement)bindable});

		static void VisualStateGroupsPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (oldValue is VisualStateGroupList oldVisualStateGroupList)
			{
				oldVisualStateGroupList.VisualElement = null;
			}

			var visualElement = (VisualElement)bindable;

			((VisualStateGroupList)newValue).VisualElement = visualElement;

			visualElement.ChangeVisualState();
		}

		public static IList<VisualStateGroup> GetVisualStateGroups(VisualElement visualElement)
		{
			return (IList<VisualStateGroup>)visualElement.GetValue(VisualStateGroupsProperty);
		}

		public static void SetVisualStateGroups(VisualElement visualElement, VisualStateGroupList value)
		{
			visualElement.SetValue(VisualStateGroupsProperty, value);
		}

		public static bool GoToState(VisualElement visualElement, string name)
		{
			if (!visualElement.IsSet(VisualStateGroupsProperty))
			{
				return false;
			}

			var groups = (IList<VisualStateGroup>)visualElement.GetValue(VisualStateGroupsProperty);

			foreach (VisualStateGroup group in groups)
			{
				if (group.CurrentState?.Name == name)
				{
					// We're already in the target state; nothing else to do
					return true;
				}

				// See if this group contains the new state
				var target = group.GetState(name);
				if (target == null)
				{
					continue;
				}

				// If we've got a new state to transition to, unapply the setters from the current state
				if (group.CurrentState != null)
				{
					foreach (Setter setter in group.CurrentState.Setters)
					{
						setter.UnApply(visualElement);
					}
				}

				// Update the current state
				group.CurrentState = target;

				// Apply the setters from the new state
				foreach (Setter setter in target.Setters)
				{
					setter.Apply(visualElement);
				}

				return true;
			}

			return false;
		}

		public static bool HasVisualStateGroups(this VisualElement element)
		{
			return element.IsSet(VisualStateGroupsProperty);
		}
	}

	public class VisualStateGroupList : IList<VisualStateGroup>
	{
		readonly IList<VisualStateGroup> _internalList;

		// Used to check for duplicate names; we keep it around because it's cheaper to create it once and clear it
		// than to create one every time we need to validate
		readonly HashSet<string> _names = new HashSet<string>();

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

		public VisualStateGroupList() 
		{
			_internalList = new WatchAddList<VisualStateGroup>(ValidateAndNotify);
		}

		void ValidateAndNotify(object sender, EventArgs eventArgs)
		{
			ValidateAndNotify(_internalList);
		}

		void ValidateAndNotify(IList<VisualStateGroup> groups)
		{
			Validate(groups);
			OnStatesChanged();
		}

		public IEnumerator<VisualStateGroup> GetEnumerator()
		{
			return _internalList.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)_internalList).GetEnumerator();
		}

		public void Add(VisualStateGroup item)
		{
			if (item == null)
			{
				throw new ArgumentNullException(nameof(item));
			}

			_internalList.Add(item);

			item.StatesChanged += ValidateAndNotify;
		}

		public void Clear()
		{
			foreach (var group in _internalList)
			{
				group.StatesChanged -= ValidateAndNotify;
			}

			_internalList.Clear();
		}

		public bool Contains(VisualStateGroup item)
		{
			return _internalList.Contains(item);
		}

		public void CopyTo(VisualStateGroup[] array, int arrayIndex)
		{
			_internalList.CopyTo(array, arrayIndex);
		}

		public bool Remove(VisualStateGroup item)
		{
			if (item == null)
			{
				throw new ArgumentNullException(nameof(item));
			}

			item.StatesChanged -= ValidateAndNotify;
			return _internalList.Remove(item);
		}

		public int Count => _internalList.Count;

		public bool IsReadOnly => false;

		public int IndexOf(VisualStateGroup item)
		{
			return _internalList.IndexOf(item);
		}

		public void Insert(int index, VisualStateGroup item)
		{
			if (item == null)
			{
				throw new ArgumentNullException(nameof(item));
			}

			item.StatesChanged += ValidateAndNotify;
			_internalList.Insert(index, item);
		}

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

		internal VisualElement VisualElement { get; set; }

		void OnStatesChanged()
		{
			VisualElement?.ChangeVisualState();
		}
	}

	[RuntimeNameProperty(nameof(Name))]
	[ContentProperty(nameof(States))]
	public sealed class VisualStateGroup 
	{
		public VisualStateGroup()
		{
			States = new WatchAddList<VisualState>(OnStatesChanged);
		}

		public Type TargetType { get; set; }
		public string Name { get; set; }
		public IList<VisualState> States { get; }
		public VisualState CurrentState { get; internal set; }

		internal VisualState GetState(string name)
		{
			foreach (VisualState state in States)
			{
				if (string.CompareOrdinal(state.Name, name) == 0)
				{
					return state;
				}
			}

			return null;
		}

		internal VisualStateGroup Clone()
		{
			var clone =  new VisualStateGroup {TargetType = TargetType, Name = Name, CurrentState = CurrentState};
			foreach (VisualState state in States)
			{
				clone.States.Add(state.Clone());
			}

			return clone;
		}

		internal event EventHandler StatesChanged;

		void OnStatesChanged(IList<VisualState> list)
		{
			if (list.Any(state => string.IsNullOrEmpty(state.Name)))
			{
				throw new InvalidOperationException("State names may not be null or empty");
			}

			StatesChanged?.Invoke(this, EventArgs.Empty);
		}
	}

	[RuntimeNameProperty(nameof(Name))]
	public sealed class VisualState 
	{
		public VisualState()
		{
			Setters = new ObservableCollection<Setter>();
		}

		public string Name { get; set; }
		public IList<Setter> Setters { get;}
		public Type TargetType { get; set; }

		internal VisualState Clone()
		{
			var clone = new VisualState { Name = Name, TargetType = TargetType };
			foreach (var setter in Setters)
			{
				clone.Setters.Add(setter);
			}

			return clone;
		}
	}

	internal static class VisualStateGroupListExtensions
	{
		internal static IList<VisualStateGroup> Clone(this IList<VisualStateGroup> groups)
		{
			var actual = new VisualStateGroupList();
			foreach (var group in groups)
			{
				actual.Add(group.Clone());
			}

			return actual;
		}
	}

	internal class WatchAddList<T> : IList<T>
	{
		readonly Action<List<T>> _onAdd;
		readonly List<T> _internalList;

		public WatchAddList(Action<List<T>> onAdd)
		{
			_onAdd = onAdd;
			_internalList = new List<T>();
		}

		public IEnumerator<T> GetEnumerator()
		{
			return _internalList.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)_internalList).GetEnumerator();
		}

		public void Add(T item)
		{
			_internalList.Add(item);
			_onAdd(_internalList);
		}

		public void Clear()
		{
			_internalList.Clear();
		}

		public bool Contains(T item)
		{
			return _internalList.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			_internalList.CopyTo(array, arrayIndex);
		}

		public bool Remove(T item)
		{
			return _internalList.Remove(item);
		}

		public int Count => _internalList.Count;

		public bool IsReadOnly => false;

		public int IndexOf(T item)
		{
			return _internalList.IndexOf(item);
		}

		public void Insert(int index, T item)
		{
			_internalList.Insert(index, item);
			_onAdd(_internalList);
		}

		public void RemoveAt(int index)
		{
			_internalList.RemoveAt(index);
		}

		public T this[int index]
		{
			get => _internalList[index];
			set => _internalList[index] = value;
		}
	}
}
