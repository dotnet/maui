#nullable disable

namespace Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific
{
	using System;
	using FormsElement = Maui.Controls.NavigationPage;

	/// <summary>The navigation page instance that Microsoft.Maui.Controls created on the iOS platform.</summary>
	public static class NavigationPage
	{
		#region Translucent
		/// <summary>Bindable property for <see cref="IsNavigationBarTranslucent"/>.</summary>

		[Obsolete("IsNavigationBarTranslucent is deprecated. The Translucent will be enabled by default by setting the BarBackgroundColor to a transparent color.")]
		public static readonly BindableProperty IsNavigationBarTranslucentProperty =
					BindableProperty.Create("IsNavigationBarTranslucent", typeof(bool),
					typeof(NavigationPage), false);

		/// <summary>Returns a Boolean value that tells whether the navigation bar on the platform-specific navigation page is translucent.</summary>
		/// <param name="element">The platform specific element on which to perform the operation.</param>
		/// <returns>A Boolean value that tells whether the navigation bar on the platform-specific navigation page is translucent.</returns>
		[Obsolete("IsNavigationBarTranslucent is deprecated. The Translucent will be enabled by default by setting the BarBackgroundColor to a transparent color.")]
		public static bool GetIsNavigationBarTranslucent(BindableObject element)
		{
			return (bool)element.GetValue(IsNavigationBarTranslucentProperty);
		}

		/// <summary>Sets whether the navigation bar is translucent on iOS.</summary>
		/// <param name="element">The element to set the value on.</param>
		/// <param name="value"><see langword="true"/> for translucent; otherwise, <see langword="false"/>.</param>
		[Obsolete("IsNavigationBarTranslucent is deprecated. The Translucent will be enabled by default by setting the BarBackgroundColor to a transparent color.")]
		public static void SetIsNavigationBarTranslucent(BindableObject element, bool value)
		{
			element.SetValue(IsNavigationBarTranslucentProperty, value);
		}

		/// <summary>Returns a Boolean value that tells whether the navigation bar on the platform-specific navigation page is translucent.</summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <returns>A Boolean value that tells whether the navigation bar on the platform-specific navigation page is translucent.</returns>
		[Obsolete("IsNavigationBarTranslucent is deprecated. The Translucent will be enabled by default by setting the BarBackgroundColor to a transparent color.")]
		public static bool IsNavigationBarTranslucent(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetIsNavigationBarTranslucent(config.Element);
		}

		/// <summary>Sets whether the navigation bar is translucent on iOS.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="value"><see langword="true"/> for translucent; otherwise, <see langword="false"/>.</param>
		/// <returns>The updated platform configuration.</returns>
		[Obsolete("IsNavigationBarTranslucent is deprecated. The Translucent will be enabled by default by setting the BarBackgroundColor to a transparent color.")]
		public static IPlatformElementConfiguration<iOS, FormsElement> SetIsNavigationBarTranslucent(this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetIsNavigationBarTranslucent(config.Element, value);
			return config;
		}

		/// <summary>Makes the navigation bar translucent on the platform-specific element.</summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <returns>The updated configuration object on which developers can make successive method calls.</returns>
		[Obsolete("IsNavigationBarTranslucent is deprecated. The Translucent will be enabled by default by setting the BarBackgroundColor to a transparent color.")]
		public static IPlatformElementConfiguration<iOS, FormsElement> EnableTranslucentNavigationBar(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			SetIsNavigationBarTranslucent(config.Element, true);
			return config;
		}

		/// <summary>Makes the navigation bar opaque on the platform-specific element.</summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <returns>The updated configuration object on which developers can make successive method calls.</returns>
		[Obsolete("IsNavigationBarTranslucent is deprecated. The Translucent will be enabled by default by setting the BarBackgroundColor to a transparent color.")]
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

		/// <summary>Gets the status bar text color mode on iOS.</summary>
		/// <param name="element">The element to get the value from.</param>
		/// <returns>The status bar text color mode.</returns>
		public static StatusBarTextColorMode GetStatusBarTextColorMode(BindableObject element)
		{
			return (StatusBarTextColorMode)element.GetValue(StatusBarTextColorModeProperty);
		}

		/// <summary>Sets the status bar text color mode on iOS.</summary>
		/// <param name="element">The element to set the value on.</param>
		/// <param name="value">The status bar text color mode to apply.</param>
		public static void SetStatusBarTextColorMode(BindableObject element, StatusBarTextColorMode value)
		{
			element.SetValue(StatusBarTextColorModeProperty, value);
		}

		/// <summary>Gets the status bar text color mode on iOS.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns>The status bar text color mode.</returns>
		public static StatusBarTextColorMode GetStatusBarTextColorMode(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetStatusBarTextColorMode(config.Element);
		}

		/// <summary>Sets the status bar text color mode on iOS.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="value">The status bar text color mode to apply.</param>
		/// <returns>The updated platform configuration.</returns>
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

		/// <summary>Sets whether iOS 11+ large titles are displayed in the navigation bar.</summary>
		/// <param name="element">The element to set the value on.</param>
		/// <param name="value"><see langword="true"/> to prefer large titles; otherwise, <see langword="false"/>.</param>
		public static void SetPrefersLargeTitles(BindableObject element, bool value)
		{
			element.SetValue(PrefersLargeTitlesProperty, value);
		}

		/// <summary>Sets whether iOS 11+ large titles are displayed in the navigation bar.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="value"><see langword="true"/> to prefer large titles; otherwise, <see langword="false"/>.</param>
		/// <returns>The updated platform configuration.</returns>
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

		/// <summary>Sets whether to hide the navigation bar separator line on iOS.</summary>
		/// <param name="element">The element to set the value on.</param>
		/// <param name="value"><see langword="true"/> to hide the separator; otherwise, <see langword="false"/>.</param>
		public static void SetHideNavigationBarSeparator(BindableObject element, bool value)
		{
			element.SetValue(HideNavigationBarSeparatorProperty, value);
		}

		/// <summary>Sets whether to hide the navigation bar separator line on iOS.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="value"><see langword="true"/> to hide the separator; otherwise, <see langword="false"/>.</param>
		/// <returns>The updated platform configuration.</returns>
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
