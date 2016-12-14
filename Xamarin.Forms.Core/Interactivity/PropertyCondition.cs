using System;
using System.ComponentModel;
using System.Reflection;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms
{
	public sealed class PropertyCondition : Condition, IValueProvider
	{
		readonly BindableProperty _stateProperty;

		BindableProperty _property;
		object _triggerValue;

		public PropertyCondition()
		{
			_stateProperty = BindableProperty.CreateAttached("State", typeof(bool), typeof(PropertyCondition), false, propertyChanged: OnStatePropertyChanged);
		}

		public BindableProperty Property
		{
			get { return _property; }
			set
			{
				if (_property == value)
					return;
				if (IsSealed)
					throw new InvalidOperationException("Can not change Property once the Trigger has been applied.");
				_property = value;
			}
		}

		public object Value
		{
			get { return _triggerValue; }
			set
			{
				if (_triggerValue == value)
					return;
				if (IsSealed)
					throw new InvalidOperationException("Can not change Value once the Trigger has been applied.");
				_triggerValue = value;
			}
		}

		object IValueProvider.ProvideValue(IServiceProvider serviceProvider)
		{
			var valueconverter = serviceProvider.GetService(typeof(IValueConverterProvider)) as IValueConverterProvider;
			Func<MemberInfo> minforetriever = () => Property.DeclaringType.GetRuntimeProperty(Property.PropertyName);

			object value = valueconverter.Convert(Value, Property.ReturnType, minforetriever, serviceProvider);
			Value = value;
			return this;
		}

		internal override bool GetState(BindableObject bindable)
		{
			return (bool)bindable.GetValue(_stateProperty);
		}

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

			if (ConditionChanged != null)
				ConditionChanged(bindable, (bool)oldValue, (bool)newValue);
		}
	}
}