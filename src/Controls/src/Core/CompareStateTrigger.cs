#nullable disable
using System;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/CompareStateTrigger.xml" path="Type[@FullName='Microsoft.Maui.Controls.CompareStateTrigger']/Docs/*" />
	public sealed class CompareStateTrigger : StateTriggerBase
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/CompareStateTrigger.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public CompareStateTrigger()
		{
			UpdateState();
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/CompareStateTrigger.xml" path="//Member[@MemberName='Property']/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/CompareStateTrigger.xml" path="//Member[@MemberName='Value']/Docs/*" />
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