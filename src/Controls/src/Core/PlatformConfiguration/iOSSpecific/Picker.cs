#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific
{
	using FormsElement = Maui.Controls.Picker;

	/// <summary>The picker instance that Microsoft.Maui.Controls created on the iOS platform.</summary>
	public static class Picker
	{
		/// <summary>Bindable property for <see cref="UpdateMode"/>.</summary>
		public static readonly BindableProperty UpdateModeProperty = BindableProperty.Create(nameof(UpdateMode), typeof(UpdateMode), typeof(Picker), default(UpdateMode));

		/// <summary>Returns a value that tells whether elements in the picker are continuously updated while scrolling or updated once after scrolling has completed.</summary>
		/// <param name="element">The platform specific element on which to perform the operation.</param>
		/// <returns>A value that tells whether elements in the picker are continuously updated while scrolling or updated once after scrolling has completed.</returns>
		public static UpdateMode GetUpdateMode(BindableObject element)
		{
			return (UpdateMode)element.GetValue(UpdateModeProperty);
		}

		/// <summary>Sets when the selected value is updated during user interaction on iOS.</summary>
		/// <param name="element">The element to set the value on.</param>
		/// <param name="value">When to update: immediately while scrolling or after selection completes.</param>
		public static void SetUpdateMode(BindableObject element, UpdateMode value)
		{
			element.SetValue(UpdateModeProperty, value);
		}

		/// <summary>Returns a value that tells whether elements in the picker are continuously updated while scrolling or updated once after scrolling has completed.</summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <returns>A value that tells whether elements in the picker are continuously updated while scrolling or updated once after scrolling has completed.</returns>
		public static UpdateMode UpdateMode(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetUpdateMode(config.Element);
		}

		/// <summary>Sets when the selected value is updated during user interaction on iOS.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="value">When to update: immediately while scrolling or after selection completes.</param>
		/// <returns>The updated platform configuration.</returns>
		public static IPlatformElementConfiguration<iOS, FormsElement> SetUpdateMode(this IPlatformElementConfiguration<iOS, FormsElement> config, UpdateMode value)
		{
			SetUpdateMode(config.Element, value);
			return config;
		}
	}
}
