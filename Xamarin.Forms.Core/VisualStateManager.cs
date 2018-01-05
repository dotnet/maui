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
				defaultValueCreator: bindable => new VisualStateGroupList());

		static void VisualStateGroupsPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			GoToState((VisualElement)bindable, CommonStates.Normal);
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
			if (visualElement.GetIsDefault(VisualStateGroupsProperty))
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
			return !element.GetIsDefault(VisualStateGroupsProperty);
		}
	}

	public class VisualStateGroupList : IList<VisualStateGroup>
	{
		readonly IList<VisualStateGroup> _internalList;

		void Validate(IList<VisualStateGroup> groups)
		{ 
			// If we have 1 group, no need to worry about duplicate group names
			if (groups.Count > 1)
			{
				if (groups.GroupBy(vsg => vsg.Name).Any(g => g.Count() > 1))
				{
					throw new InvalidOperationException("VisualStateGroup Names must be unique");
				}
			}

			// State names must be unique within this group list, so pull in all 
			// the states in all the groups, group them by name, and see if we have
			// and duplicates
			if (groups.SelectMany(group => group.States)
				.GroupBy(state => state.Name)
				.Any(g => g.Count() > 1))
			{
				throw new InvalidOperationException("VisualState Names must be unique");
			}
		}

		public VisualStateGroupList() 
		{
			_internalList = new WatchAddList<VisualStateGroup>(Validate);
		}

		void ValidateOnStatesChanged(object sender, EventArgs eventArgs)
		{
			Validate(_internalList);
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
			_internalList.Add(item);
			item.StatesChanged += ValidateOnStatesChanged;
		}

		public void Clear()
		{
			foreach (var group in _internalList)
			{
				group.StatesChanged -= ValidateOnStatesChanged;
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
			item.StatesChanged -= ValidateOnStatesChanged;
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
			item.StatesChanged += ValidateOnStatesChanged;
			_internalList.Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			_internalList[index].StatesChanged -= ValidateOnStatesChanged;
			_internalList.RemoveAt(index);
		}

		public VisualStateGroup this[int index]
		{
			get => _internalList[index];
			set => _internalList[index] = value;
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
