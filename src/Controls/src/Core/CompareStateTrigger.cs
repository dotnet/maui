#nullable disable
using System;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// A state trigger that activates when <see cref="Property"/> equals <see cref="Value"/>.
	/// </summary>
	public sealed class CompareStateTrigger : StateTriggerBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CompareStateTrigger"/> class.
		/// </summary>
		public CompareStateTrigger()
		{
			UpdateState();
		}

		/// <summary>
		/// Gets or sets the property value to compare against <see cref="Value"/>. This is a bindable property.
		/// </summary>
		public object Property
		{
			get => GetValue(PropertyProperty);
			set => SetValue(PropertyProperty, value);
		}

		/// <summary>Bindable property for <see cref="Property"/>.</summary>
		public static readonly BindableProperty PropertyProperty =
		BindableProperty.Create(nameof(Property), typeof(object), typeof(CompareStateTrigger), null,
			propertyChanged: OnPropertyChanged);

		static void OnPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
		{
			((CompareStateTrigger)bindable).UpdateState();
		}

		/// <summary>
		/// Gets or sets the value to compare against <see cref="Property"/>. This is a bindable property.
		/// </summary>
		public object Value
		{
			get => GetValue(ValueProperty);
			set => SetValue(ValueProperty, value);
		}

		/// <summary>Bindable property for <see cref="Value"/>.</summary>
		public static readonly BindableProperty ValueProperty =
		BindableProperty.Create(nameof(Value), typeof(object), typeof(CompareStateTrigger), null,
			propertyChanged: OnValueChanged);

		static void OnValueChanged(BindableObject bindable, object oldvalue, object newvalue)
		{
			((CompareStateTrigger)bindable).UpdateState();
		}

		protected override void OnAttached()
		{
			base.OnAttached();
			UpdateState();
		}

		void UpdateState()
		{
			SetActive(AreEqual(Property, Value));
		}

		bool AreEqual(object value1, object value2)
		{
			if (value1 == value2)
				return true;

			if (value1 != null && value2 != null)
				return AreEqualType(value1, value2) || AreEqualType(value2, value1);

			return false;
		}

		bool AreEqualType(object value1, object value2)
		{
			if (value2 is Enum)
				value1 = ConvertToEnum(value2.GetType(), value1);
			else
				value1 = Convert.ChangeType(value1, value2.GetType(), CultureInfo.InvariantCulture);

			return value2.Equals(value1);
		}

		object ConvertToEnum(Type enumType, object value)
		{
			try
			{
				return Enum.IsDefined(enumType, value) ? Enum.ToObject(enumType, value) : null;
			}
			catch
			{
				return null;
			}
		}
	}
}