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
	/// Contains a collection of mutually exclusive <see cref="VisualState"/> objects and the setters to apply when transitioning between them.
	/// </summary>
	[RuntimeNameProperty(nameof(Name))]
	[ContentProperty(nameof(States))]
	public sealed class VisualStateGroup
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="VisualStateGroup"/> class.
		/// </summary>
		public VisualStateGroup()
		{
			States = new WatchAddList<VisualState>(OnStatesChanged);
		}

		/// <summary>
		/// Gets or sets the type that this visual state group targets.
		/// </summary>
		public Type TargetType { get; set; }

		/// <summary>
		/// Gets or sets the name of the visual state group.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets the collection of <see cref="VisualState"/> objects in this group.
		/// </summary>
		public IList<VisualState> States { get; }

		/// <summary>
		/// Gets the currently active <see cref="VisualState"/> in this group.
		/// </summary>
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
				if (string.Equals(state.Name, name, StringComparison.Ordinal))
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

			if (VisualDiagnostics.IsEnabled && VisualDiagnostics.GetSourceInfo(this) is SourceInfo info)
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
}
