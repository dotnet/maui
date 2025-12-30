using System.Reflection;

namespace Microsoft.Maui.ManualTests.Controls
{
	class EnumPicker : Picker
	{
		public static readonly BindableProperty EnumTypeProperty =
			BindableProperty.Create(nameof(EnumType), typeof(Type), typeof(EnumPicker),
				propertyChanged: (bindable, oldValue, newValue) =>
				{
					EnumPicker picker = (EnumPicker)bindable;

					if (oldValue != null)
					{
						picker.ItemsSource = null;
					}
					if (newValue != null)
					{
						if (!((Type)newValue).GetTypeInfo().IsEnum)
							throw new ArgumentException("EnumPicker: EnumType property must be enumeration type");

						picker.ItemsSource = Enum.GetValues((Type)newValue);
					}
				});

		public Type EnumType
		{
			set => SetValue(EnumTypeProperty, value);
			get => (Type)GetValue(EnumTypeProperty);
		}
	}
}
