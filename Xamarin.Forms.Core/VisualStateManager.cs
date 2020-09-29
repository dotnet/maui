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
		public class CommonStates
		{
			public const string Normal = "Normal";
			public const string Disabled = "Disabled";
			public const string Focused = "Focused";
			public const string Selected = "Selected";
		}

		public static readonly BindableProperty VisualStateGroupsProperty =
			BindableProperty.CreateAttached("VisualStateGroups", typeof(VisualStateGroupList), typeof(VisualElement),
				defaultValue: null, propertyChanged: VisualStateGroupsPropertyChanged,
				defaultValueCreator: bindable => new VisualStateGroupList(true) { VisualElement = (VisualElement)bindable });

		static void VisualStateGroupsPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (oldValue is VisualStateGroupList oldVisualStateGroupList)
			{
				oldVisualStateGroupList.VisualElement = null;
			}

			var visualElement = (VisualElement)bindable;

			((VisualStateGroupList)newValue).VisualElement = visualElement;

			visualElement.ChangeVisualState();

			UpdateStateTriggers(visualElement);
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
			if (!visualElement.HasVisualStateGroups())
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
			if (!element.IsSet(VisualStateGroupsProperty))
				return false;

			if (GetVisualStateGroups(element) is VisualStateGroupList vsgl)
				return !vsgl.IsDefault;

			return true;
		}

		internal static void UpdateStateTriggers(VisualElement visualElement)
		{
			var groups = (IList<VisualStateGroup>)visualElement.GetValue(VisualStateGroupsProperty);

			foreach (VisualStateGroup group in groups)
			{
				group.VisualElement = visualElement;
				group.UpdateStateTriggers();
			}
		}
	}

	public class VisualStateGroupList : IList<VisualStateGroup>
	{
		readonly IList<VisualStateGroup> _internalList;
		internal bool IsDefault { get; private set; }

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

		public VisualStateGroupList() : this(false)
		{
		}

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
		internal VisualElement VisualElement { get; set; }

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

		internal bool HasStateTriggers()
		{
			bool hasStateTriggers = false;

			foreach (VisualState state in States)
			{
				if (state.StateTriggers.Count > 0)
				{
					hasStateTriggers = true;
					break;
				}
			}

			return hasStateTriggers;
		}

		internal VisualState GetActiveTrigger()
		{
			var defaultState = default(VisualState);
			var visualState = defaultState;
			var conflicts = new List<StateTriggerBase>();

			for (var stateIndex = 0; stateIndex < States.Count; stateIndex++)
			{
				var state = States[stateIndex];
				for (var triggerIndex = 0; triggerIndex < state.StateTriggers.Count; triggerIndex++)
				{
					var trigger = state.StateTriggers[triggerIndex];

					if (trigger.IsActive)
					{
						if (visualState == defaultState)
							visualState = state;

						conflicts.Add(trigger);
					}
				}
			}

			if (conflicts.Count > 1)
				visualState = ResolveStateTriggersConflict(conflicts);

			return visualState;
		}

		VisualState ResolveStateTriggersConflict(List<StateTriggerBase> conflicts)
		{
			// When using StateTriggers to control visual states, the trigger engine uses the following rules to 
			// score triggers and determine which trigger, and the corresponding VisualState, will be active:
			//
			// 1. Custom trigger that derives from StateTriggerBase
			// 2. AdaptiveTrigger activated due to MinWindowWidth
			// 3. AdaptiveTrigger activated due to MinWindowHeight
			//
			// If there are multiple active triggers at a time that have a conflict in scoring (i.e.two custom 
			// triggers), then the first one declared in the markup file takes precedence.

			var existCustomTriggers = conflicts.Where(c => !(c is AdaptiveTrigger));

			if (existCustomTriggers.Count() > 1)
			{
				var firstExistCustomTrigger = existCustomTriggers.FirstOrDefault();
				return firstExistCustomTrigger.VisualState;
			}

			var adaptiveTriggers = conflicts.Where(c => c is AdaptiveTrigger);

			var minWindowWidthAdaptiveTriggers = adaptiveTriggers.Where(c => ((AdaptiveTrigger)c).MinWindowWidth != -1d).OrderByDescending(c => ((AdaptiveTrigger)c).MinWindowWidth);
			var latestMinWindowWidthAdaptiveTrigger = minWindowWidthAdaptiveTriggers.FirstOrDefault();

			if (latestMinWindowWidthAdaptiveTrigger != null)
				return latestMinWindowWidthAdaptiveTrigger.VisualState;

			var minWindowHeightAdaptiveTriggers = adaptiveTriggers.Where(c => ((AdaptiveTrigger)c).MinWindowHeight != -1d).OrderByDescending(c => ((AdaptiveTrigger)c).MinWindowHeight);
			var latestMinWindowHeightAdaptiveTrigger = minWindowHeightAdaptiveTriggers.FirstOrDefault();

			if (latestMinWindowHeightAdaptiveTrigger != null)
				return latestMinWindowHeightAdaptiveTrigger.VisualState;

			return default;
		}

		internal VisualStateGroup Clone()
		{
			var clone = new VisualStateGroup { TargetType = TargetType, Name = Name, CurrentState = CurrentState, VisualElement = VisualElement };

			foreach (VisualState state in States)
			{
				state.VisualStateGroup = clone;
				clone.States.Add(state.Clone());
			}

			return clone;
		}

		internal void UpdateStateTriggers()
		{
			if (VisualElement == null)
				return;

			bool hasStateTriggers = HasStateTriggers();

			if (!hasStateTriggers)
				return;

			var newStateTrigger = GetActiveTrigger();

			if (newStateTrigger == null)
				return;

			var oldStateTrigger = CurrentState;

			if (newStateTrigger == oldStateTrigger)
				return;

			VisualStateManager.GoToState(VisualElement, newStateTrigger.Name);
		}

		internal event EventHandler StatesChanged;

		void OnStatesChanged(IList<VisualState> states)
		{
			if (states.Any(state => string.IsNullOrEmpty(state.Name)))
			{
				throw new InvalidOperationException("State names may not be null or empty");
			}

			foreach (var state in states)
			{
				state.VisualStateGroup = this;
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
			StateTriggers = new WatchAddList<StateTriggerBase>(OnStateTriggersChanged);
		}

		public string Name { get; set; }
		public IList<Setter> Setters { get; }
		public IList<StateTriggerBase> StateTriggers { get; }
		public Type TargetType { get; set; }
		internal VisualStateGroup VisualStateGroup { get; set; }

		internal VisualState Clone()
		{
			var clone = new VisualState { Name = Name, TargetType = TargetType };

			foreach (var setter in Setters)
			{
				clone.Setters.Add(setter);
			}

			foreach (var stateTrigger in StateTriggers)
			{
				stateTrigger.VisualState = this;
				clone.StateTriggers.Add(stateTrigger);
			}

			return clone;
		}

		void OnStateTriggersChanged(IList<StateTriggerBase> stateTriggers)
		{
			foreach (var stateTrigger in stateTriggers)
			{
				stateTrigger.VisualState = this;
			}

			VisualStateGroup?.UpdateStateTriggers();
		}
	}

	internal static class VisualStateGroupListExtensions
	{
		internal static IList<VisualStateGroup> Clone(this IList<VisualStateGroup> groups)
		{
			var actual = new VisualStateGroupList();

			foreach (var group in groups)
			{
				group.VisualElement = actual.VisualElement;
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