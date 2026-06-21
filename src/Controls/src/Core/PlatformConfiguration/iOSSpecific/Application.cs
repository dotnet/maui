#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific
{
	using FormsElement = Maui.Controls.Application;

	/// <summary>Provides control over simultaneous recognition for pan gesture recognizers.</summary>
	public static class Application
	{
		#region PanGestureRecognizerShouldRecognizeSimultaneously
		/// <summary>Bindable property for attached property <c>PanGestureRecognizerShouldRecognizeSimultaneously</c>.</summary>
		public static readonly BindableProperty PanGestureRecognizerShouldRecognizeSimultaneouslyProperty = BindableProperty.Create("PanGestureRecognizerShouldRecognizeSimultaneously", typeof(bool), typeof(Application), false);

		/// <summary>Gets whether pan gesture recognizers can recognize gestures simultaneously with other gesture recognizers.</summary>
		/// <param name="element">The element to get the value from.</param>
		/// <returns><see langword="true"/> if simultaneous recognition is enabled; otherwise, <see langword="false"/>.</returns>
		public static bool GetPanGestureRecognizerShouldRecognizeSimultaneously(BindableObject element)
		{
			return (bool)element.GetValue(PanGestureRecognizerShouldRecognizeSimultaneouslyProperty);
		}

		/// <summary>Sets whether pan gesture recognizers can recognize gestures simultaneously with other gesture recognizers.</summary>
		/// <param name="element">The element to set the value on.</param>
		/// <param name="value"><see langword="true"/> to enable simultaneous recognition; otherwise, <see langword="false"/>.</param>
		public static void SetPanGestureRecognizerShouldRecognizeSimultaneously(BindableObject element, bool value)
		{
			element.SetValue(PanGestureRecognizerShouldRecognizeSimultaneouslyProperty, value);
		}

		/// <summary>Gets whether pan gesture recognizers can recognize gestures simultaneously with other gesture recognizers.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns><see langword="true"/> if simultaneous recognition is enabled; otherwise, <see langword="false"/>.</returns>
		public static bool GetPanGestureRecognizerShouldRecognizeSimultaneously(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetPanGestureRecognizerShouldRecognizeSimultaneously(config.Element);
		}

		/// <summary>Sets whether pan gesture recognizers can recognize gestures simultaneously with other gesture recognizers.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="value"><see langword="true"/> to enable simultaneous recognition; otherwise, <see langword="false"/>.</param>
		/// <returns>The updated platform configuration.</returns>
		public static IPlatformElementConfiguration<iOS, FormsElement> SetPanGestureRecognizerShouldRecognizeSimultaneously(this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetPanGestureRecognizerShouldRecognizeSimultaneously(config.Element, value);
			return config;
		}
		#endregion

		#region HandleControlUpdatesOnMainThread
		/// <summary>Bindable property for attached property <c>HandleControlUpdatesOnMainThread</c>.</summary>
		public static readonly BindableProperty HandleControlUpdatesOnMainThreadProperty = BindableProperty.Create("HandleControlUpdatesOnMainThread", typeof(bool), typeof(Application), false);

		/// <summary>Gets whether control property updates are processed on the main thread on iOS.</summary>
		/// <param name="element">The element to get the value from.</param>
		/// <returns><see langword="true"/> if updates are handled on the main thread; otherwise, <see langword="false"/>.</returns>
		public static bool GetHandleControlUpdatesOnMainThread(BindableObject element)
		{
			return (bool)element.GetValue(HandleControlUpdatesOnMainThreadProperty);
		}

		/// <summary>Sets whether control property updates are processed on the main thread on iOS.</summary>
		/// <param name="element">The element to set the value on.</param>
		/// <param name="value"><see langword="true"/> to handle updates on the main thread; otherwise, <see langword="false"/>.</param>
		public static void SetHandleControlUpdatesOnMainThread(BindableObject element, bool value)
		{
			element.SetValue(HandleControlUpdatesOnMainThreadProperty, value);
		}

		/// <summary>Gets whether control property updates are processed on the main thread on iOS.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns><see langword="true"/> if updates are handled on the main thread; otherwise, <see langword="false"/>.</returns>
		public static bool GetHandleControlUpdatesOnMainThread(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetHandleControlUpdatesOnMainThread(config.Element);
		}

		/// <summary>Sets whether control property updates are processed on the main thread on iOS.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="value"><see langword="true"/> to handle updates on the main thread; otherwise, <see langword="false"/>.</param>
		/// <returns>The updated platform configuration.</returns>
		public static IPlatformElementConfiguration<iOS, FormsElement> SetHandleControlUpdatesOnMainThread(this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetHandleControlUpdatesOnMainThread(config.Element, value);
			return config;
		}
		#endregion

		#region EnableAccessibilityScalingForNamedFontSizes
		/// <summary>Bindable property for attached property <c>EnableAccessibilityScalingForNamedFontSize</c>.</summary>
		public static readonly BindableProperty EnableAccessibilityScalingForNamedFontSizesProperty = BindableProperty.Create("EnableAccessibilityScalingForNamedFontSizes", typeof(bool), typeof(Application), true);

		/// <summary>Gets whether named font sizes respond to iOS Dynamic Type accessibility settings.</summary>
		/// <param name="element">The element to get the value from.</param>
		/// <returns><see langword="true"/> if accessibility scaling is enabled; otherwise, <see langword="false"/>.</returns>
		public static bool GetEnableAccessibilityScalingForNamedFontSizes(BindableObject element)
		{
			return (bool)element.GetValue(EnableAccessibilityScalingForNamedFontSizesProperty);
		}

		/// <summary>Sets whether named font sizes respond to iOS Dynamic Type accessibility settings.</summary>
		/// <param name="element">The element to set the value on.</param>
		/// <param name="value"><see langword="true"/> to enable accessibility scaling; otherwise, <see langword="false"/>.</param>
		public static void SetEnableAccessibilityScalingForNamedFontSizes(BindableObject element, bool value)
		{
			element.SetValue(EnableAccessibilityScalingForNamedFontSizesProperty, value);
		}

		/// <summary>Gets whether named font sizes respond to iOS Dynamic Type accessibility settings.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns><see langword="true"/> if accessibility scaling is enabled; otherwise, <see langword="false"/>.</returns>
		public static bool GetEnableAccessibilityScalingForNamedFontSizes(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetEnableAccessibilityScalingForNamedFontSizes(config.Element);
		}

		/// <summary>Sets whether named font sizes respond to iOS Dynamic Type accessibility settings.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="value"><see langword="true"/> to enable accessibility scaling; otherwise, <see langword="false"/>.</param>
		/// <returns>The updated platform configuration.</returns>
		public static IPlatformElementConfiguration<iOS, FormsElement> SetEnableAccessibilityScalingForNamedFontSizes(this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetEnableAccessibilityScalingForNamedFontSizes(config.Element, value);
			return config;
		}
		#endregion
	}
}
