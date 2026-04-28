#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Represents a named visual state with setters and triggers that define the appearance of a control.
	/// </summary>
	[RuntimeNameProperty(nameof(Name))]
	public sealed class VisualState
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="VisualState"/> class.
		/// </summary>
		public VisualState()
		{
			Setters = new ObservableCollection<Setter>();
			StateTriggers = new WatchAddList<StateTriggerBase>(OnStateTriggersChanged);
		}

		/// <summary>
		/// Gets or sets the name of the visual state (e.g., "Normal", "Focused", "Disabled").
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets the collection of <see cref="Setter"/> objects that define property values for this state.
		/// </summary>
		public IList<Setter> Setters { get; }

		/// <summary>
		/// Gets the collection of <see cref="StateTriggerBase"/> objects that activate this state.
		/// </summary>
		public IList<StateTriggerBase> StateTriggers { get; }

		/// <summary>
		/// Gets or sets the type that this visual state targets.
		/// </summary>
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

			if (VisualDiagnostics.IsEnabled && VisualDiagnostics.GetSourceInfo(this) is SourceInfo info)
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
}
