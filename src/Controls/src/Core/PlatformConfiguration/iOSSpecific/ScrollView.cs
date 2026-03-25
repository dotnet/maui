#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific
{
	using FormsElement = Maui.Controls.ScrollView;

	/// <summary>The scroll view instance that Microsoft.Maui.Controls created on the iOS platform.</summary>
	public static class ScrollView
	{
		/// <summary>Bindable property for <see cref="ShouldDelayContentTouches"/>.</summary>
		public static readonly BindableProperty ShouldDelayContentTouchesProperty = BindableProperty.Create(nameof(ShouldDelayContentTouches), typeof(bool), typeof(ScrollView), true);

		/// <summary>Returns a Boolean value that tells whether iOS will wait to determine if a touch is intended as a scroll, or scroll immediately.</summary>
		/// <param name="element">The platform specific element on which to perform the operation.</param>
		/// <returns>A Boolean value that tells whether iOS will wait to determine if a touch is intended as a scroll, or scroll immediately.</returns>
		public static bool GetShouldDelayContentTouches(BindableObject element)
		{
			return (bool)element.GetValue(ShouldDelayContentTouchesProperty);
		}

		/// <summary>Sets whether iOS delays touch events to determine scroll intent.</summary>
		/// <param name="element">The element to set the value on.</param>
		/// <param name="value"><see langword="true"/> to delay; <see langword="false"/> for immediate touch response.</param>
		public static void SetShouldDelayContentTouches(BindableObject element, bool value)
		{
			element.SetValue(ShouldDelayContentTouchesProperty, value);
		}

		/// <summary>Returns a Boolean value that tells whether iOS will wait to determine if a touch is intended as a scroll, or scroll immediately.</summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <returns>A Boolean value that tells whether iOS will wait to determine if a touch is intended as a scroll, or scroll immediately.</returns>
		public static bool ShouldDelayContentTouches(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetShouldDelayContentTouches(config.Element);
		}

		/// <summary>Sets whether iOS delays touch events to determine scroll intent.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="value"><see langword="true"/> to delay; <see langword="false"/> for immediate touch response.</param>
		/// <returns>The updated platform configuration.</returns>
		public static IPlatformElementConfiguration<iOS, FormsElement> SetShouldDelayContentTouches(this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetShouldDelayContentTouches(config.Element, value);
			return config;
		}
	}
}
