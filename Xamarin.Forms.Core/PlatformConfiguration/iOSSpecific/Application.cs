namespace Xamarin.Forms.PlatformConfiguration.iOSSpecific
{
	using FormsElement = Forms.Application;

	public static class Application
	{
		#region PanGestureRecognizerShouldRecognizeSimultaneously
		public static readonly BindableProperty PanGestureRecognizerShouldRecognizeSimultaneouslyProperty = BindableProperty.Create("PanGestureRecognizerShouldRecognizeSimultaneously", typeof(bool), typeof(Application), false);

		public static bool GetPanGestureRecognizerShouldRecognizeSimultaneously(BindableObject element)
		{
			return (bool)element.GetValue(PanGestureRecognizerShouldRecognizeSimultaneouslyProperty);
		}

		public static void SetPanGestureRecognizerShouldRecognizeSimultaneously(BindableObject element, bool value)
		{
			element.SetValue(PanGestureRecognizerShouldRecognizeSimultaneouslyProperty, value);
		}

		public static bool GetPanGestureRecognizerShouldRecognizeSimultaneously(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetPanGestureRecognizerShouldRecognizeSimultaneously(config.Element);
		}

		public static IPlatformElementConfiguration<iOS, FormsElement> SetPanGestureRecognizerShouldRecognizeSimultaneously(this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetPanGestureRecognizerShouldRecognizeSimultaneously(config.Element, value);
			return config;
		}
		#endregion

		#region HandleControlUpdatesOnMainThread
		public static readonly BindableProperty HandleControlUpdatesOnMainThreadProperty = BindableProperty.Create("HandleControlUpdatesOnMainThread", typeof(bool), typeof(Application), false);

		public static bool GetHandleControlUpdatesOnMainThread(BindableObject element)
		{
			return (bool)element.GetValue(HandleControlUpdatesOnMainThreadProperty);
		}

		public static void SetHandleControlUpdatesOnMainThread(BindableObject element, bool value)
		{
			element.SetValue(HandleControlUpdatesOnMainThreadProperty, value);
		}

		public static bool GetHandleControlUpdatesOnMainThread(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetHandleControlUpdatesOnMainThread(config.Element);
		}

		public static IPlatformElementConfiguration<iOS, FormsElement> SetHandleControlUpdatesOnMainThread(this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetHandleControlUpdatesOnMainThread(config.Element, value);
			return config;
		}
		#endregion

		#region EnableAccessibilityScalingForNamedFontSizes
		public static readonly BindableProperty EnableAccessibilityScalingForNamedFontSizesProperty = BindableProperty.Create("EnableAccessibilityScalingForNamedFontSizes", typeof(bool), typeof(Application), true);

		public static bool GetEnableAccessibilityScalingForNamedFontSizes(BindableObject element)
		{
			return (bool)element.GetValue(EnableAccessibilityScalingForNamedFontSizesProperty);
		}

		public static void SetEnableAccessibilityScalingForNamedFontSizes(BindableObject element, bool value)
		{
			element.SetValue(EnableAccessibilityScalingForNamedFontSizesProperty, value);
		}

		public static bool GetEnableAccessibilityScalingForNamedFontSizes(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetEnableAccessibilityScalingForNamedFontSizes(config.Element);
		}

		public static IPlatformElementConfiguration<iOS, FormsElement> SetEnableAccessibilityScalingForNamedFontSizes(this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetEnableAccessibilityScalingForNamedFontSizes(config.Element, value);
			return config;
		}
		#endregion
	}
}