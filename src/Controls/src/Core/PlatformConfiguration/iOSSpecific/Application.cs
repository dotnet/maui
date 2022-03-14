namespace Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific
{
	using FormsElement = Maui.Controls.Application;

	/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Application.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Application']/Docs" />
	public static class Application
	{
		#region PanGestureRecognizerShouldRecognizeSimultaneously
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Application.xml" path="//Member[@MemberName='PanGestureRecognizerShouldRecognizeSimultaneouslyProperty']/Docs" />
		public static readonly BindableProperty PanGestureRecognizerShouldRecognizeSimultaneouslyProperty = BindableProperty.Create("PanGestureRecognizerShouldRecognizeSimultaneously", typeof(bool), typeof(Application), false);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Application.xml" path="//Member[@MemberName='GetPanGestureRecognizerShouldRecognizeSimultaneously' and position()=0]/Docs" />
		public static bool GetPanGestureRecognizerShouldRecognizeSimultaneously(BindableObject element)
		{
			return (bool)element.GetValue(PanGestureRecognizerShouldRecognizeSimultaneouslyProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Application.xml" path="//Member[@MemberName='SetPanGestureRecognizerShouldRecognizeSimultaneously' and position()=0]/Docs" />
		public static void SetPanGestureRecognizerShouldRecognizeSimultaneously(BindableObject element, bool value)
		{
			element.SetValue(PanGestureRecognizerShouldRecognizeSimultaneouslyProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Application.xml" path="//Member[@MemberName='GetPanGestureRecognizerShouldRecognizeSimultaneously' and position()=1]/Docs" />
		public static bool GetPanGestureRecognizerShouldRecognizeSimultaneously(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetPanGestureRecognizerShouldRecognizeSimultaneously(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Application.xml" path="//Member[@MemberName='SetPanGestureRecognizerShouldRecognizeSimultaneously' and position()=1]/Docs" />
		public static IPlatformElementConfiguration<iOS, FormsElement> SetPanGestureRecognizerShouldRecognizeSimultaneously(this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetPanGestureRecognizerShouldRecognizeSimultaneously(config.Element, value);
			return config;
		}
		#endregion

		#region HandleControlUpdatesOnMainThread
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Application.xml" path="//Member[@MemberName='HandleControlUpdatesOnMainThreadProperty']/Docs" />
		public static readonly BindableProperty HandleControlUpdatesOnMainThreadProperty = BindableProperty.Create("HandleControlUpdatesOnMainThread", typeof(bool), typeof(Application), false);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Application.xml" path="//Member[@MemberName='GetHandleControlUpdatesOnMainThread' and position()=0]/Docs" />
		public static bool GetHandleControlUpdatesOnMainThread(BindableObject element)
		{
			return (bool)element.GetValue(HandleControlUpdatesOnMainThreadProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Application.xml" path="//Member[@MemberName='SetHandleControlUpdatesOnMainThread' and position()=0]/Docs" />
		public static void SetHandleControlUpdatesOnMainThread(BindableObject element, bool value)
		{
			element.SetValue(HandleControlUpdatesOnMainThreadProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Application.xml" path="//Member[@MemberName='GetHandleControlUpdatesOnMainThread' and position()=1]/Docs" />
		public static bool GetHandleControlUpdatesOnMainThread(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetHandleControlUpdatesOnMainThread(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Application.xml" path="//Member[@MemberName='SetHandleControlUpdatesOnMainThread' and position()=1]/Docs" />
		public static IPlatformElementConfiguration<iOS, FormsElement> SetHandleControlUpdatesOnMainThread(this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetHandleControlUpdatesOnMainThread(config.Element, value);
			return config;
		}
		#endregion

		#region EnableAccessibilityScalingForNamedFontSizes
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Application.xml" path="//Member[@MemberName='EnableAccessibilityScalingForNamedFontSizesProperty']/Docs" />
		public static readonly BindableProperty EnableAccessibilityScalingForNamedFontSizesProperty = BindableProperty.Create("EnableAccessibilityScalingForNamedFontSizes", typeof(bool), typeof(Application), true);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Application.xml" path="//Member[@MemberName='GetEnableAccessibilityScalingForNamedFontSizes' and position()=0]/Docs" />
		public static bool GetEnableAccessibilityScalingForNamedFontSizes(BindableObject element)
		{
			return (bool)element.GetValue(EnableAccessibilityScalingForNamedFontSizesProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Application.xml" path="//Member[@MemberName='SetEnableAccessibilityScalingForNamedFontSizes' and position()=0]/Docs" />
		public static void SetEnableAccessibilityScalingForNamedFontSizes(BindableObject element, bool value)
		{
			element.SetValue(EnableAccessibilityScalingForNamedFontSizesProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Application.xml" path="//Member[@MemberName='GetEnableAccessibilityScalingForNamedFontSizes' and position()=1]/Docs" />
		public static bool GetEnableAccessibilityScalingForNamedFontSizes(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetEnableAccessibilityScalingForNamedFontSizes(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Application.xml" path="//Member[@MemberName='SetEnableAccessibilityScalingForNamedFontSizes' and position()=1]/Docs" />
		public static IPlatformElementConfiguration<iOS, FormsElement> SetEnableAccessibilityScalingForNamedFontSizes(this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetEnableAccessibilityScalingForNamedFontSizes(config.Element, value);
			return config;
		}
		#endregion
	}
}
