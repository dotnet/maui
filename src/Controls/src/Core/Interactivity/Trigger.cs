#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// A trigger that activates setters when a property on the control matches a specified value.
	/// </summary>
	[ContentProperty("Setters")]
	public sealed class Trigger : TriggerBase
	{
		/// <summary>
		/// Initializes a new <see cref="Trigger" /> instance.
		/// </summary>
		/// <param name="targetType">The type of object to which this trigger can be attached.</param>
		public Trigger([System.ComponentModel.TypeConverter(typeof(TypeTypeConverter))][Parameter("TargetType")] Type targetType) : base(new PropertyCondition(), targetType)
		{
		}

		/// <summary>
		/// Gets or sets the property whose value will be compared to <see cref="Value" /> to determine when to activate the trigger.
		/// </summary>
		public BindableProperty Property
		{
			get { return ((PropertyCondition)Condition).Property; }
			set
			{
				if (((PropertyCondition)Condition).Property == value)
					return;
				if (IsSealed)
					throw new InvalidOperationException("Cannot change Property once the Trigger has been applied.");
				OnPropertyChanging();
				((PropertyCondition)Condition).Property = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Gets the collection of <see cref="Setter" /> objects to apply when the trigger condition is met.
		/// </summary>
		public new IList<Setter> Setters
		{
			get { return base.Setters; }
		}

		/// <summary>
		/// Gets or sets the value to compare against <see cref="Property" /> to determine when to activate the trigger.
		/// </summary>
		public object Value
		{
			get { return ((PropertyCondition)Condition).Value; }
			set
			{
				if (((PropertyCondition)Condition).Value == value)
					return;
				if (IsSealed)
					throw new InvalidOperationException("Cannot change Value once the Trigger has been applied.");
				OnPropertyChanging();
				((PropertyCondition)Condition).Value = value;
				OnPropertyChanged();
			}
		}
	}
}