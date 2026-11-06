#nullable disable
using System;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// A condition that is satisfied when a binding evaluates to a specified value.
	/// </summary>
	public sealed class BindingCondition : Condition
	{
		readonly BindableProperty _boundProperty;
		readonly BindableProperty _stateProperty;

		BindingBase _binding;
		object _triggerValue;

		/// <summary>
		/// Initializes a new instance of the <see cref="BindingCondition"/> class.
		/// </summary>
		public BindingCondition()
		{
			_boundProperty = BindableProperty.CreateAttached("Bound", typeof(object), typeof(BindingCondition), null, propertyChanged: OnBoundPropertyChanged);
			_stateProperty = BindableProperty.CreateAttached("State", typeof(bool), typeof(BindingCondition), BooleanBoxes.FalseBox, propertyChanged: OnStatePropertyChanged);
		}

		/// <summary>
		/// Gets or sets the binding whose value is evaluated for this condition.
		/// </summary>
		public BindingBase Binding
		{
			get { return _binding; }
			set
			{
				if (_binding == value)
					return;
				if (IsSealed)
					throw new InvalidOperationException("Cannot change Binding once the Condition has been applied.");
				_binding = value;
			}
		}

		/// <summary>
		/// Gets or sets the value that satisfies this condition when matched by the binding.
		/// </summary>
		public object Value
		{
			get { return _triggerValue; }
			set
			{
				if (_triggerValue == value)
					return;
				if (IsSealed)
					throw new InvalidOperationException("Cannot change Value once the Condition has been applied.");
				_triggerValue = value;
			}
		}

		internal override bool GetState(BindableObject bindable)
		{
			return (bool)bindable.GetValue(_stateProperty);
		}

		internal override void SetUp(BindableObject bindable)
		{
			if (Binding != null)
				bindable.SetBinding(_boundProperty, Binding.Clone());

			// Applying the binding doesn't raise a property change when the bound value equals the
			// default value of _boundProperty (null), so evaluate the initial state explicitly (#6847).
			bindable.SetValue(_stateProperty, EqualsToValue(bindable.GetValue(_boundProperty)));
		}

		internal override void TearDown(BindableObject bindable)
		{
			bindable.ClearValue(_stateProperty);
			bindable.RemoveBinding(_boundProperty);
			bindable.ClearValue(_boundProperty, SetterSpecificity.FromBinding);
		}

		static IValueConverterProvider s_valueConverter = DependencyService.Get<IValueConverterProvider>();

		bool EqualsToValue(object other)
		{
			if ((other == Value) || (other != null && other.Equals(Value)))
				return true;

			object converted = null;
			if (s_valueConverter != null)
				converted = s_valueConverter.Convert(Value, other != null ? other.GetType() : typeof(object), null, null);
			else
				return false;

			return (other == converted) || (other != null && other.Equals(converted));
		}

		void OnBoundPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			// Not set up yet (the initial state is evaluated in SetUp), or already torn down.
			if (!bindable.IsSet(_stateProperty))
				return;

			bool newState = EqualsToValue(newValue);
			if ((bool)bindable.GetValue(_stateProperty) != newState)
				bindable.SetValue(_stateProperty, newState);
		}

		void OnStatePropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if ((bool)oldValue == (bool)newValue)
				return;

			ConditionChanged?.Invoke(bindable, (bool)oldValue, (bool)newValue);
		}
	}
}