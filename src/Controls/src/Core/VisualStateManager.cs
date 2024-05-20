#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/VisualStateManager.xml" path="Type[@FullName='Microsoft.Maui.Controls.VisualStateManager']/Docs/*" />
	public static class VisualStateManager
	{
		public class CommonStates
		{
			public const string Normal = "Normal";
			public const string Disabled = "Disabled";
			public const string Focused = "Focused";
			public const string Selected = "Selected";
			public const string PointerOver = "PointerOver";
			internal const string Unfocused = "Unfocused";
		}

		/// <summary>Bindable property for attached property <c>VisualStateGroups</c>.</summary>
		public static readonly BindableProperty VisualStateGroupsProperty =
			BindableProperty.CreateAttached("VisualStateGroups", typeof(VisualStateGroupList), typeof(VisualElement),
				defaultValue: null, propertyChanged: VisualStateGroupsPropertyChanged,
				defaultValueCreator: bindable => new VisualStateGroupList(true) { VisualElement = (VisualElement)bindable });

		static void VisualStateGroupsPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (oldValue is VisualStateGroupList oldVisualStateGroupList && oldVisualStateGroupList.VisualElement is VisualElement oldElement)
			{
				var vsgSpecificity = ((VisualStateGroupList)oldValue).Specificity;
				var specificity = new SetterSpecificity(1, 0, 0, 0, vsgSpecificity.Style, vsgSpecificity.Id, vsgSpecificity.Class, vsgSpecificity.Type);

				foreach (var group in oldVisualStateGroupList)
				{
					if (group.CurrentState is VisualState state)
						foreach (var setter in state.Setters)
							setter.UnApply(oldElement, specificity);
				}
				oldVisualStateGroupList.VisualElement = null;
			}

			var visualElement = (VisualElement)bindable;

			if (newValue != null)
				((VisualStateGroupList)newValue).VisualElement = visualElement;

			visualElement.ChangeVisualState();

			UpdateStateTriggers(visualElement);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualStateManager.xml" path="//Member[@MemberName='GetVisualStateGroups']/Docs/*" />
		public static IList<VisualStateGroup> GetVisualStateGroups(VisualElement visualElement)
			=> (IList<VisualStateGroup>)visualElement.GetValue(VisualStateGroupsProperty);

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualStateManager.xml" path="//Member[@MemberName='SetVisualStateGroups']/Docs/*" />
		public static void SetVisualStateGroups(VisualElement visualElement, VisualStateGroupList value)
			=> visualElement.SetValue(VisualStateGroupsProperty, value);

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualStateManager.xml" path="//Member[@MemberName='GoToState']/Docs/*" />
		public static bool GoToState(VisualElement visualElement, string name)
		{
			if (!visualElement.HasVisualStateGroups())
			{
				return false;
			}

			var groups = (VisualStateGroupList)visualElement.GetValue(VisualStateGroupsProperty);
			var context = visualElement.GetContext(VisualStateGroupsProperty);
			var vsgSpecificity = context.Values.GetSpecificityAndValue().Key;
			if (vsgSpecificity == SetterSpecificity.DefaultValue)
				vsgSpecificity = new SetterSpecificity();
			groups.Specificity = vsgSpecificity;
			var specificity = new SetterSpecificity(1, 0, 0, 0, vsgSpecificity.Style, vsgSpecificity.Id, vsgSpecificity.Class, vsgSpecificity.Type);

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
						setter.UnApply(visualElement, specificity);
					}
				}

				// Update the current state
				group.CurrentState = target;

				// Apply the setters from the new state
				foreach (Setter setter in target.Setters)
				{
					setter.Apply(visualElement, specificity);
				}

				return true;
			}

			return false;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualStateManager.xml" path="//Member[@MemberName='HasVisualStateGroups']/Docs/*" />
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

	/// <include file="../../docs/Microsoft.Maui.Controls/VisualStateGroupList.xml" path="Type[@FullName='Microsoft.Maui.Controls.VisualStateGroupList']/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualStateGroupList.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public VisualStateGroupList() : this(false)
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualStateGroupList.xml" path="//Member[@MemberName='.ctor'][2]/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualStateGroupList.xml" path="//Member[@MemberName='GetEnumerator']/Docs/*" />
		public IEnumerator<VisualStateGroup> GetEnumerator()
		{
			return _internalList.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)_internalList).GetEnumerator();
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualStateGroupList.xml" path="//Member[@MemberName='Add']/Docs/*" />
		public void Add(VisualStateGroup item)
		{
			if (item == null)
			{
				throw new ArgumentNullException(nameof(item));
			}

			_internalList.Add(item);

			item.StatesChanged += ValidateAndNotify;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualStateGroupList.xml" path="//Member[@MemberName='Clear']/Docs/*" />
		public void Clear()
		{
			foreach (var group in _internalList)
			{
				group.StatesChanged -= ValidateAndNotify;
			}

			_internalList.Clear();
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualStateGroupList.xml" path="//Member[@MemberName='Contains']/Docs/*" />
		public bool Contains(VisualStateGroup item)
		{
			return _internalList.Contains(item);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualStateGroupList.xml" path="//Member[@MemberName='CopyTo']/Docs/*" />
		public void CopyTo(VisualStateGroup[] array, int arrayIndex)
		{
			_internalList.CopyTo(array, arrayIndex);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualStateGroupList.xml" path="//Member[@MemberName='Remove']/Docs/*" />
		public bool Remove(VisualStateGroup item)
		{
			if (item == null)
			{
				throw new ArgumentNullException(nameof(item));
			}

			item.StatesChanged -= ValidateAndNotify;
			return _internalList.Remove(item);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualStateGroupList.xml" path="//Member[@MemberName='Count']/Docs/*" />
		public int Count => _internalList.Count;

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualStateGroupList.xml" path="//Member[@MemberName='IsReadOnly']/Docs/*" />
		public bool IsReadOnly => false;

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualStateGroupList.xml" path="//Member[@MemberName='IndexOf']/Docs/*" />
		public int IndexOf(VisualStateGroup item)
		{
			return _internalList.IndexOf(item);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualStateGroupList.xml" path="//Member[@MemberName='Insert']/Docs/*" />
		public void Insert(int index, VisualStateGroup item)
		{
			if (item == null)
			{
				throw new ArgumentNullException(nameof(item));
			}

			item.StatesChanged += ValidateAndNotify;
			_internalList.Insert(index, item);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualStateGroupList.xml" path="//Member[@MemberName='RemoveAt']/Docs/*" />
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

	/// <include file="../../docs/Microsoft.Maui.Controls/VisualStateGroup.xml" path="Type[@FullName='Microsoft.Maui.Controls.VisualStateGroup']/Docs/*" />
	[RuntimeNameProperty(nameof(Name))]
	[ContentProperty(nameof(States))]
	public sealed class VisualStateGroup
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/VisualStateGroup.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public VisualStateGroup()
		{
			States = new WatchAddList<VisualState>(OnStatesChanged);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualStateGroup.xml" path="//Member[@MemberName='TargetType']/Docs/*" />
		public Type TargetType { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualStateGroup.xml" path="//Member[@MemberName='Name']/Docs/*" />
		public string Name { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualStateGroup.xml" path="//Member[@MemberName='States']/Docs/*" />
		public IList<VisualState> States { get; }

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualStateGroup.xml" path="//Member[@MemberName='CurrentState']/Docs/*" />
		public VisualState CurrentState { get; internal set; }

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

			if (DebuggerHelper.DebuggerIsAttached && VisualDiagnostics.GetSourceInfo(this) is SourceInfo info)
				VisualDiagnostics.RegisterSourceInfo(clone, info.SourceUri, info.LineNumber, info.LinePosition);

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

		public override bool Equals(object obj) => Equals(obj as VisualStateGroup);

		bool Equals(VisualStateGroup other)
		{
			if (other is null)
				return false;
			if (object.ReferenceEquals(this, other))
				return true;
			if (Name != other.Name)
				return false;
			if (TargetType != other.TargetType)
				return false;
			if (States.Count != other.States.Count)
				return false;
			for (var i = 0; i < States.Count; i++)
				if (!States[i].Equals(other.States[i]))
					return false;
			return true;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hash = (Name, TargetType).GetHashCode();
				for (var i = 0; i < States.Count; i++)
					hash = (hash * 43) ^ States[i].GetHashCode();
				return hash;
			}
		}
	}

	/// <include file="../../docs/Microsoft.Maui.Controls/VisualState.xml" path="Type[@FullName='Microsoft.Maui.Controls.VisualState']/Docs/*" />
	[RuntimeNameProperty(nameof(Name))]
	public sealed class VisualState
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/VisualState.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public VisualState()
		{
			Setters = new ObservableCollection<Setter>();
			StateTriggers = new WatchAddList<StateTriggerBase>(OnStateTriggersChanged);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualState.xml" path="//Member[@MemberName='Name']/Docs/*" />
		public string Name { get; set; }
		/// <include file="../../docs/Microsoft.Maui.Controls/VisualState.xml" path="//Member[@MemberName='Setters']/Docs/*" />
		public IList<Setter> Setters { get; }
		/// <include file="../../docs/Microsoft.Maui.Controls/VisualState.xml" path="//Member[@MemberName='StateTriggers']/Docs/*" />
		public IList<StateTriggerBase> StateTriggers { get; }
		/// <include file="../../docs/Microsoft.Maui.Controls/VisualState.xml" path="//Member[@MemberName='TargetType']/Docs/*" />
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

			if (DebuggerHelper.DebuggerIsAttached && VisualDiagnostics.GetSourceInfo(this) is SourceInfo info)
				VisualDiagnostics.RegisterSourceInfo(clone, info.SourceUri, info.LineNumber, info.LinePosition);

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

		public override bool Equals(object obj) => Equals(obj as VisualState);

		bool Equals(VisualState other)
		{
			if (other is null)
				return false;
			if (object.ReferenceEquals(this, other))
				return true;
			if (Name != other.Name)
				return false;
			if (TargetType != other.TargetType)
				return false;
			if (Setters.Count != other.Setters.Count)
				return false;
			if (StateTriggers.Count != other.StateTriggers.Count)
				return false;
			for (var i = 0; i < Setters.Count; i++)
				if (!Setters[i].Equals(other.Setters[i]))
					return false;
			for (var i = 0; i < StateTriggers.Count; i++)
				if (!StateTriggers[i].Equals(other.StateTriggers[i]))
					return false;
			return true;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hash = (Name, TargetType).GetHashCode();
				for (var i = 0; i < Setters.Count; i++)
					hash = (hash * 43) ^ Setters[i].GetHashCode();
				for (var i = 0; i < StateTriggers.Count; i++)
					hash = (hash * 43) ^ StateTriggers[i].GetHashCode();
				return hash;
			}
		}
	}

	internal static class VisualStateGroupListExtensions
	{
		internal static IList<VisualStateGroup> Clone(this IList<VisualStateGroup> groups)
		{
			var clone = new VisualStateGroupList();

			foreach (var group in groups)
			{
				group.VisualElement = clone.VisualElement;
				clone.Add(group.Clone());
			}

			if (DebuggerHelper.DebuggerIsAttached && VisualDiagnostics.GetSourceInfo(groups) is SourceInfo info)
				VisualDiagnostics.RegisterSourceInfo(clone, info.SourceUri, info.LineNumber, info.LinePosition);

			return clone;
		}

		internal static bool HasVisualState(this VisualElement element, string name)
		{
			IList<VisualStateGroup> list = VisualStateManager.GetVisualStateGroups(element);
			for (var i = 0; i < list.Count; i++)
			{
				VisualStateGroup group = list[i];
				for (var j = 0; j < group.States.Count; j++)
				{
					if (group.States[j].Name == name)
						return true;
				}
			}

			return false;
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