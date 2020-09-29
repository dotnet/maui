namespace Xamarin.Forms.PlatformConfiguration.macOSSpecific
{
	using FormsElement = Forms.NavigationPage;

	public static class NavigationPage
	{
		public static readonly BindableProperty NavigationTransitionPushStyleProperty = BindableProperty.Create("NavigationTransitionPushStyle", typeof(NavigationTransitionStyle), typeof(NavigationPage), NavigationTransitionStyle.SlideForward);
		public static readonly BindableProperty NavigationTransitionPopStyleProperty = BindableProperty.Create("NavigationTransitionPopStyle", typeof(NavigationTransitionStyle), typeof(NavigationPage), NavigationTransitionStyle.SlideBackward);

		#region PushStyle
		public static NavigationTransitionStyle GetNavigationTransitionPushStyle(BindableObject element)
		{
			return (NavigationTransitionStyle)element.GetValue(NavigationTransitionPushStyleProperty);
		}

		public static void SetNavigationTransitionPushStyle(BindableObject element, NavigationTransitionStyle value)
		{
			element.SetValue(NavigationTransitionPushStyleProperty, value);
		}

		public static NavigationTransitionStyle GetNavigationTransitionPushStyle(this IPlatformElementConfiguration<macOS, FormsElement> config)
		{
			return GetNavigationTransitionPushStyle(config.Element);
		}
		#endregion

		#region PopStyle
		public static NavigationTransitionStyle GetNavigationTransitionPopStyle(BindableObject element)
		{
			return (NavigationTransitionStyle)element.GetValue(NavigationTransitionPopStyleProperty);
		}

		public static void SetNavigationTransitionPopStyle(BindableObject element, NavigationTransitionStyle value)
		{
			element.SetValue(NavigationTransitionPopStyleProperty, value);
		}

		public static NavigationTransitionStyle GetNavigationTransitionPopStyle(this IPlatformElementConfiguration<macOS, FormsElement> config)
		{
			return GetNavigationTransitionPopStyle(config.Element);
		}
		#endregion

		public static void SetNavigationTransitionStyle(BindableObject element, NavigationTransitionStyle pushStyle, NavigationTransitionStyle popStyle)
		{
			SetNavigationTransitionPushStyle(element, pushStyle);
			SetNavigationTransitionPopStyle(element, popStyle);
		}

		public static IPlatformElementConfiguration<macOS, FormsElement> SetNavigationTransitionStyle(this IPlatformElementConfiguration<macOS, FormsElement> config, NavigationTransitionStyle pushStyle, NavigationTransitionStyle popStyle)
		{
			SetNavigationTransitionStyle(config.Element, pushStyle, popStyle);
			return config;
		}
	}
}