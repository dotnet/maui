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

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Application.xml" path="//Member[@MemberName='GetPanGestureRecognizerShouldRecognizeSimultaneously'][1]/Docs/*" />
		public static bool GetPanGestureRecognizerShouldRecognizeSimultaneously(BindableObject element)
		{
			return (bool)element.GetValue(PanGestureRecognizerShouldRecognizeSimultaneouslyProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Application.xml" path="//Member[@MemberName='SetPanGestureRecognizerShouldRecognizeSimultaneously'][1]/Docs/*" />
		public static void SetPanGestureRecognizerShouldRecognizeSimultaneously(BindableObject element, bool value)
		{
			element.SetValue(PanGestureRecognizerShouldRecognizeSimultaneouslyProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Application.xml" path="//Member[@MemberName='GetPanGestureRecognizerShouldRecognizeSimultaneously'][2]/Docs/*" />
		public static bool GetPanGestureRecognizerShouldRecognizeSimultaneously(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetPanGestureRecognizerShouldRecognizeSimultaneously(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Application.xml" path="//Member[@MemberName='SetPanGestureRecognizerShouldRecognizeSimultaneously'][2]/Docs/*" />
		public static IPlatformElementConfiguration<iOS, FormsElement> SetPanGestureRecognizerShouldRecognizeSimultaneously(this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetPanGestureRecognizerShouldRecognizeSimultaneously(config.Element, value);
			return config;
		}
		#endregion

		#region HandleControlUpdatesOnMainThread
		/// <summary>Bindable property for attached property <c>HandleControlUpdatesOnMainThread</c>.</summary>
		public static readonly BindableProperty HandleControlUpdatesOnMainThreadProperty = BindableProperty.Create("HandleControlUpdatesOnMainThread", typeof(bool), typeof(Application), false);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Application.xml" path="//Member[@MemberName='GetHandleControlUpdatesOnMainThread'][1]/Docs/*" />
		public static bool GetHandleControlUpdatesOnMainThread(BindableObject element)
		{
			return (bool)element.GetValue(HandleControlUpdatesOnMainThreadProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Application.xml" path="//Member[@MemberName='SetHandleControlUpdatesOnMainThread'][1]/Docs/*" />
		public static void SetHandleControlUpdatesOnMainThread(BindableObject element, bool value)
		{
			element.SetValue(HandleControlUpdatesOnMainThreadProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Application.xml" path="//Member[@MemberName='GetHandleControlUpdatesOnMainThread'][2]/Docs/*" />
		public static bool GetHandleControlUpdatesOnMainThread(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetHandleControlUpdatesOnMainThread(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Application.xml" path="//Member[@MemberName='SetHandleControlUpdatesOnMainThread'][2]/Docs/*" />
		public static IPlatformElementConfiguration<iOS, FormsElement> SetHandleControlUpdatesOnMainThread(this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetHandleControlUpdatesOnMainThread(config.Element, value);
			return config;
		}
		#endregion

		#region EnableAccessibilityScalingForNamedFontSizes
		/// <summary>Bindable property for attached property <c>EnableAccessibilityScalingForNamedFontSize</c>.</summary>
		public static readonly BindableProperty EnableAccessibilityScalingForNamedFontSizesProperty = BindableProperty.Create("EnableAccessibilityScalingForNamedFontSizes", typeof(bool), typeof(Application), true);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Application.xml" path="//Member[@MemberName='GetEnableAccessibilityScalingForNamedFontSizes'][1]/Docs/*" />
		public static bool GetEnableAccessibilityScalingForNamedFontSizes(BindableObject element)
		{
			return (bool)element.GetValue(EnableAccessibilityScalingForNamedFontSizesProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Application.xml" path="//Member[@MemberName='SetEnableAccessibilityScalingForNamedFontSizes'][1]/Docs/*" />
		public static void SetEnableAccessibilityScalingForNamedFontSizes(BindableObject element, bool value)
		{
			element.SetValue(EnableAccessibilityScalingForNamedFontSizesProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Application.xml" path="//Member[@MemberName='GetEnableAccessibilityScalingForNamedFontSizes'][2]/Docs/*" />
		public static bool GetEnableAccessibilityScalingForNamedFontSizes(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetEnableAccessibilityScalingForNamedFontSizes(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Application.xml" path="//Member[@MemberName='SetEnableAccessibilityScalingForNamedFontSizes'][2]/Docs/*" />
		public static IPlatformElementConfiguration<iOS, FormsElement> SetEnableAccessibilityScalingForNamedFontSizes(this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetEnableAccessibilityScalingForNamedFontSizes(config.Element, value);
			return config;
		}
		#endregion
	}
}
