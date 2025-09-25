#nullable disable

namespace Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific
{
	using FormsElement = Maui.Controls.NavigationPage;

	/// <summary>The navigation page instance that Microsoft.Maui.Controls created on the iOS platform.</summary>
	public static class NavigationPage
	{
		#region Translucent
		/// <summary>Bindable property for <see cref="IsNavigationBarTranslucent"/>.</summary>
		public static readonly BindableProperty IsNavigationBarTranslucentProperty =
			BindableProperty.Create("IsNavigationBarTranslucent", typeof(bool),
			typeof(NavigationPage), false);

		/// <summary>Returns a Boolean value that tells whether the navigation bar on the platform-specific navigation page is translucent.</summary>
		/// <param name="element">The platform specific element on which to perform the operation.</param>
		/// <returns>A Boolean value that tells whether the navigation bar on the platform-specific navigation page is translucent.</returns>
		public static bool GetIsNavigationBarTranslucent(BindableObject element)
		{
			return (bool)element.GetValue(IsNavigationBarTranslucentProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/NavigationPage.xml" path="//Member[@MemberName='SetIsNavigationBarTranslucent'][1]/Docs/*" />
		public static void SetIsNavigationBarTranslucent(BindableObject element, bool value)
		{
			element.SetValue(IsNavigationBarTranslucentProperty, value);
		}

		/// <summary>Returns a Boolean value that tells whether the navigation bar on the platform-specific navigation page is translucent.</summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <returns>A Boolean value that tells whether the navigation bar on the platform-specific navigation page is translucent.</returns>
		public static bool IsNavigationBarTranslucent(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetIsNavigationBarTranslucent(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/NavigationPage.xml" path="//Member[@MemberName='SetIsNavigationBarTranslucent'][2]/Docs/*" />
		public static IPlatformElementConfiguration<iOS, FormsElement> SetIsNavigationBarTranslucent(this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetIsNavigationBarTranslucent(config.Element, value);
			return config;
		}

		/// <summary>Makes the navigation bar translucent on the platform-specific element.</summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <returns>The updated configuration object on which developers can make successive method calls.</returns>
		public static IPlatformElementConfiguration<iOS, FormsElement> EnableTranslucentNavigationBar(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			SetIsNavigationBarTranslucent(config.Element, true);
			return config;
		}

		/// <summary>Makes the navigation bar opaque on the platform-specific element.</summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <returns>The updated configuration object on which developers can make successive method calls.</returns>
		public static IPlatformElementConfiguration<iOS, FormsElement> DisableTranslucentNavigationBar(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			SetIsNavigationBarTranslucent(config.Element, false);
			return config;
		}
		#endregion


		#region StatusBarTextColorMode
		/// <summary>Bindable property for <see cref="StatusBarTextColorMode"/>.</summary>
		public static readonly BindableProperty StatusBarTextColorModeProperty =
			BindableProperty.Create("StatusBarColorTextMode", typeof(StatusBarTextColorMode),
			typeof(NavigationPage), StatusBarTextColorMode.MatchNavigationBarTextLuminosity);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/NavigationPage.xml" path="//Member[@MemberName='GetStatusBarTextColorMode'][1]/Docs/*" />
		public static StatusBarTextColorMode GetStatusBarTextColorMode(BindableObject element)
		{
			return (StatusBarTextColorMode)element.GetValue(StatusBarTextColorModeProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/NavigationPage.xml" path="//Member[@MemberName='SetStatusBarTextColorMode'][1]/Docs/*" />
		public static void SetStatusBarTextColorMode(BindableObject element, StatusBarTextColorMode value)
		{
			element.SetValue(StatusBarTextColorModeProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/NavigationPage.xml" path="//Member[@MemberName='GetStatusBarTextColorMode'][2]/Docs/*" />
		public static StatusBarTextColorMode GetStatusBarTextColorMode(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetStatusBarTextColorMode(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/NavigationPage.xml" path="//Member[@MemberName='SetStatusBarTextColorMode'][2]/Docs/*" />
		public static IPlatformElementConfiguration<iOS, FormsElement> SetStatusBarTextColorMode(this IPlatformElementConfiguration<iOS, FormsElement> config, StatusBarTextColorMode value)
		{
			SetStatusBarTextColorMode(config.Element, value);
			return config;
		}
		#endregion

		#region PrefersLargeTitles
		/// <summary>Bindable property for <see cref="PrefersLargeTitles"/>.</summary>
		public static readonly BindableProperty PrefersLargeTitlesProperty = BindableProperty.Create(nameof(PrefersLargeTitles), typeof(bool), typeof(Page), false);

		/// <summary>Returns the large title preference of <paramref name="element"/>.</summary>
		/// <param name="element">The element whose large title preference to get.</param>
		/// <returns>The large title preference of <paramref name="element"/>.</returns>
		public static bool GetPrefersLargeTitles(BindableObject element)
		{
			return (bool)element.GetValue(PrefersLargeTitlesProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/NavigationPage.xml" path="//Member[@MemberName='SetPrefersLargeTitles'][1]/Docs/*" />
		public static void SetPrefersLargeTitles(BindableObject element, bool value)
		{
			element.SetValue(PrefersLargeTitlesProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/NavigationPage.xml" path="//Member[@MemberName='SetPrefersLargeTitles'][2]/Docs/*" />
		public static IPlatformElementConfiguration<iOS, FormsElement> SetPrefersLargeTitles(this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetPrefersLargeTitles(config.Element, value);
			return config;
		}

		/// <summary>Returns a value that indicates the element's preference for large titles.</summary>
		/// <param name="config">The element whose large title preference to get.</param>
		/// <returns>A value that indicates the element's preference for large titles.</returns>
		public static bool PrefersLargeTitles(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetPrefersLargeTitles(config.Element);
		}
		#endregion

		#region HideNavigationBarSeparator
		/// <summary>Bindable property for <see cref="HideNavigationBarSeparator"/>.</summary>
		public static readonly BindableProperty HideNavigationBarSeparatorProperty = BindableProperty.Create(nameof(HideNavigationBarSeparator), typeof(bool), typeof(Page), false);

		/// <summary>Returns <see langword="true"/> if the separator is hidden. Otherwise, returns <see langword="false"/>.</summary>
		/// <param name="element">The element for which to return whether the navigation bar separator is hidden.</param>
		/// <returns>see langword="true" /> if the separator is hidden. Otherwise, <see langword="false"/></returns>
		public static bool GetHideNavigationBarSeparator(BindableObject element)
		{
			return (bool)element.GetValue(HideNavigationBarSeparatorProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/NavigationPage.xml" path="//Member[@MemberName='SetHideNavigationBarSeparator'][1]/Docs/*" />
		public static void SetHideNavigationBarSeparator(BindableObject element, bool value)
		{
			element.SetValue(HideNavigationBarSeparatorProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/NavigationPage.xml" path="//Member[@MemberName='SetHideNavigationBarSeparator'][2]/Docs/*" />
		public static IPlatformElementConfiguration<iOS, FormsElement> SetHideNavigationBarSeparator(this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetHideNavigationBarSeparator(config.Element, value);
			return config;
		}

		/// <summary>Returns <see langword="true"/> if the separator is hidden. Otherwise, returns <see langword="false"/>.</summary>
		/// <param name="config">The platform configuration for the element for which to return whether the navigation bar separator is hidden.</param>
		/// <returns><see langword="true"/> if the separator is hidden. Otherwise, <see langword="false"/></returns>
		public static bool HideNavigationBarSeparator(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetHideNavigationBarSeparator(config.Element);
		}
		#endregion
	}
}
