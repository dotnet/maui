#nullable disable
using System;
using System.ComponentModel;
using System.Reflection;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// A condition that is satisfied when a property has a specified value.
	/// </summary>
	public sealed class PropertyCondition : Condition
	{
		readonly BindableProperty _stateProperty;

		BindableProperty _property;
		object _triggerValue;

		/// <summary>
		/// Initializes a new instance of the <see cref="PropertyCondition"/> class.
		/// </summary>
		public PropertyCondition()
		{
			_stateProperty = BindableProperty.CreateAttached("State", typeof(bool), typeof(PropertyCondition), false, propertyChanged: OnStatePropertyChanged);
		}

		/// <summary>
		/// Gets or sets the bindable property whose value is evaluated for this condition.
		/// </summary>
		public BindableProperty Property
		{
			get { return _property; }
			set
			{
				if (_property == value)
					return;
				if (IsSealed)
					throw new InvalidOperationException("Cannot change Property once the Trigger has been applied.");
				_property = value;

				//convert the value
				if (_property != null && s_valueConverter != null)
				{
					Func<MemberInfo> minforetriever = () =>
					{
						try
						{
							return Property.DeclaringType.GetRuntimeProperty(Property.PropertyName);
						}
						catch (AmbiguousMatchException e)
						{
							throw new XamlParseException($"Multiple properties with name '{Property.DeclaringType}.{Property.PropertyName}' found.", new XmlLineInfo(), innerException: e);
						}
					};
					Value = s_valueConverter.Convert(Value, Property.ReturnType, minforetriever, null);
				}
			}
		}

		/// <summary>
		/// Gets or sets the value that satisfies this condition when matched by the property.
		/// </summary>
		public object Value
		{
			get { return _triggerValue; }
			set
			{
				if (_triggerValue == value)
					return;
				if (IsSealed)
					throw new InvalidOperationException("Cannot change Value once the Trigger has been applied.");

				//convert the value
				if (_property != null && s_valueConverter != null)
				{
					Func<MemberInfo> minforetriever = () =>
					{
						try
						{
							return Property.DeclaringType.GetRuntimeProperty(Property.PropertyName);
						}
						catch (AmbiguousMatchException e)
						{
							throw new XamlParseException($"Multiple properties with name '{Property.DeclaringType}.{Property.PropertyName}' found.", new XmlLineInfo(), innerException: e);
						}
					};
					value = s_valueConverter.Convert(value, Property.ReturnType, minforetriever, null);
				}
				_triggerValue = value;
			}
		}

		internal override bool GetState(BindableObject bindable)
		{
			return (bool)bindable.GetValue(_stateProperty);
		}

		static IValueConverterProvider s_valueConverter = DependencyService.Get<IValueConverterProvider>();

		internal override void SetUp(BindableObject bindable)
		{
			object newvalue = bindable.GetValue(Property);
			bool newState = (newvalue == Value) || (newvalue != null && newvalue.Equals(Value));
			bindable.SetValue(_stateProperty, newState);
			bindable.PropertyChanged += OnAttachedObjectPropertyChanged;
		}

		internal override void TearDown(BindableObject bindable)
		{
			bindable.ClearValue(_stateProperty);
			bindable.PropertyChanged -= OnAttachedObjectPropertyChanged;
		}

		void OnAttachedObjectPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			var bindable = (BindableObject)sender;
			var oldState = (bool)bindable.GetValue(_stateProperty);

			if (Property == null)
				return;
			if (e.PropertyName != Property.PropertyName)
				return;
			object newvalue = bindable.GetValue(Property);
			bool newstate = (newvalue == Value) || (newvalue != null && newvalue.Equals(Value));
			if (oldState != newstate)
				bindable.SetValue(_stateProperty, newstate);
		}

		void OnStatePropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if ((bool)oldValue == (bool)newValue)
				return;

			ConditionChanged?.Invoke(bindable, (bool)oldValue, (bool)newValue);
		}
	}
}