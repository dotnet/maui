#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// A trigger that activates setters when a bound value matches a specified value.
	/// </summary>
	[ContentProperty("Setters")]
	public sealed class DataTrigger : TriggerBase
	{
		/// <summary>
		/// Initializes a new <see cref="DataTrigger" /> instance.
		/// </summary>
		/// <param name="targetType">The type of object to which this trigger can be attached.</param>
		public DataTrigger([System.ComponentModel.TypeConverter(typeof(TypeTypeConverter))][Parameter("TargetType")] Type targetType) : base(new BindingCondition(), targetType)
		{
		}

		/// <summary>
		/// Gets or sets the binding whose value will be compared to <see cref="Value" /> to determine when to activate the trigger.
		/// </summary>
		public BindingBase Binding
		{
			get { return ((BindingCondition)Condition).Binding; }
			set
			{
				if (((BindingCondition)Condition).Binding == value)
					return;
				if (IsSealed)
					throw new InvalidOperationException("Cannot change Binding once the Trigger has been applied.");
				OnPropertyChanging();
				((BindingCondition)Condition).Binding = value;
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
		/// Gets or sets the value to compare against <see cref="Binding" /> to determine when to activate the trigger.
		/// </summary>
		public object Value
		{
			get { return ((BindingCondition)Condition).Value; }
			set
			{
				if (((BindingCondition)Condition).Value == value)
					return;
				if (IsSealed)
					throw new InvalidOperationException("Cannot change Value once the Trigger has been applied.");
				OnPropertyChanging();
				((BindingCondition)Condition).Value = value;
				OnPropertyChanged();
			}
		}
	}
}