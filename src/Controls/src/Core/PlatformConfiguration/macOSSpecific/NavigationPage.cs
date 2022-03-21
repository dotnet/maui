namespace Microsoft.Maui.Controls.PlatformConfiguration.macOSSpecific
{
	using FormsElement = Maui.Controls.NavigationPage;

	/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.macOSSpecific/NavigationPage.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.macOSSpecific.NavigationPage']/Docs" />
	public static class NavigationPage
	{
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.macOSSpecific/NavigationPage.xml" path="//Member[@MemberName='NavigationTransitionPushStyleProperty']/Docs" />
		public static readonly BindableProperty NavigationTransitionPushStyleProperty = BindableProperty.Create("NavigationTransitionPushStyle", typeof(NavigationTransitionStyle), typeof(NavigationPage), NavigationTransitionStyle.SlideForward);
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.macOSSpecific/NavigationPage.xml" path="//Member[@MemberName='NavigationTransitionPopStyleProperty']/Docs" />
		public static readonly BindableProperty NavigationTransitionPopStyleProperty = BindableProperty.Create("NavigationTransitionPopStyle", typeof(NavigationTransitionStyle), typeof(NavigationPage), NavigationTransitionStyle.SlideBackward);

		#region PushStyle
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.macOSSpecific/NavigationPage.xml" path="//Member[@MemberName='GetNavigationTransitionPushStyle'][1]/Docs" />
		public static NavigationTransitionStyle GetNavigationTransitionPushStyle(BindableObject element)
		{
			return (NavigationTransitionStyle)element.GetValue(NavigationTransitionPushStyleProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.macOSSpecific/NavigationPage.xml" path="//Member[@MemberName='SetNavigationTransitionPushStyle']/Docs" />
		public static void SetNavigationTransitionPushStyle(BindableObject element, NavigationTransitionStyle value)
		{
			element.SetValue(NavigationTransitionPushStyleProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.macOSSpecific/NavigationPage.xml" path="//Member[@MemberName='GetNavigationTransitionPushStyle'][2]/Docs" />
		public static NavigationTransitionStyle GetNavigationTransitionPushStyle(this IPlatformElementConfiguration<macOS, FormsElement> config)
		{
			return GetNavigationTransitionPushStyle(config.Element);
		}
		#endregion

		#region PopStyle
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.macOSSpecific/NavigationPage.xml" path="//Member[@MemberName='GetNavigationTransitionPopStyle'][1]/Docs" />
		public static NavigationTransitionStyle GetNavigationTransitionPopStyle(BindableObject element)
		{
			return (NavigationTransitionStyle)element.GetValue(NavigationTransitionPopStyleProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.macOSSpecific/NavigationPage.xml" path="//Member[@MemberName='SetNavigationTransitionPopStyle']/Docs" />
		public static void SetNavigationTransitionPopStyle(BindableObject element, NavigationTransitionStyle value)
		{
			element.SetValue(NavigationTransitionPopStyleProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.macOSSpecific/NavigationPage.xml" path="//Member[@MemberName='GetNavigationTransitionPopStyle'][2]/Docs" />
		public static NavigationTransitionStyle GetNavigationTransitionPopStyle(this IPlatformElementConfiguration<macOS, FormsElement> config)
		{
			return GetNavigationTransitionPopStyle(config.Element);
		}
		#endregion

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.macOSSpecific/NavigationPage.xml" path="//Member[@MemberName='SetNavigationTransitionStyle'][1]/Docs" />
		public static void SetNavigationTransitionStyle(BindableObject element, NavigationTransitionStyle pushStyle, NavigationTransitionStyle popStyle)
		{
			SetNavigationTransitionPushStyle(element, pushStyle);
			SetNavigationTransitionPopStyle(element, popStyle);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.macOSSpecific/NavigationPage.xml" path="//Member[@MemberName='SetNavigationTransitionStyle'][2]/Docs" />
		public static IPlatformElementConfiguration<macOS, FormsElement> SetNavigationTransitionStyle(this IPlatformElementConfiguration<macOS, FormsElement> config, NavigationTransitionStyle pushStyle, NavigationTransitionStyle popStyle)
		{
			SetNavigationTransitionStyle(config.Element, pushStyle, popStyle);
			return config;
		}
	}
}
