
namespace Xamarin.Forms.PlatformConfiguration.iOSSpecific
{
	using FormsElement = Forms.NavigationPage;

	public static class NavigationPage
	{
		#region Translucent
		public static readonly BindableProperty IsNavigationBarTranslucentProperty =
			BindableProperty.Create("IsNavigationBarTranslucent", typeof(bool),
			typeof(NavigationPage), false);

		public static bool GetIsNavigationBarTranslucent(BindableObject element)
		{
			return (bool)element.GetValue(IsNavigationBarTranslucentProperty);
		}

		public static void SetIsNavigationBarTranslucent(BindableObject element, bool value)
		{
			element.SetValue(IsNavigationBarTranslucentProperty, value);
		}

		public static bool IsNavigationBarTranslucent(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetIsNavigationBarTranslucent(config.Element);
		}

		public static IPlatformElementConfiguration<iOS, FormsElement> SetIsNavigationBarTranslucent(this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetIsNavigationBarTranslucent(config.Element, value);
			return config;
		}

		public static IPlatformElementConfiguration<iOS, FormsElement> EnableTranslucentNavigationBar(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			SetIsNavigationBarTranslucent(config.Element, true);
			return config;
		}

		public static IPlatformElementConfiguration<iOS, FormsElement> DisableTranslucentNavigationBar(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			SetIsNavigationBarTranslucent(config.Element, false);
			return config;
		}
		#endregion


		#region StatusBarTextColorMode
		public static readonly BindableProperty StatusBarTextColorModeProperty =
			BindableProperty.Create("StatusBarColorTextMode", typeof(StatusBarTextColorMode),
			typeof(NavigationPage), StatusBarTextColorMode.MatchNavigationBarTextLuminosity);

		public static StatusBarTextColorMode GetStatusBarTextColorMode(BindableObject element)
		{
			return (StatusBarTextColorMode)element.GetValue(StatusBarTextColorModeProperty);
		}

		public static void SetStatusBarTextColorMode(BindableObject element, StatusBarTextColorMode value)
		{
			element.SetValue(StatusBarTextColorModeProperty, value);
		}

		public static StatusBarTextColorMode GetStatusBarTextColorMode(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetStatusBarTextColorMode(config.Element);
		}

		public static IPlatformElementConfiguration<iOS, FormsElement> SetStatusBarTextColorMode(this IPlatformElementConfiguration<iOS, FormsElement> config, StatusBarTextColorMode value)
		{
			SetStatusBarTextColorMode(config.Element, value);
			return config;
		}
		#endregion

		#region PrefersLargeTitles
		public static readonly BindableProperty PrefersLargeTitlesProperty = BindableProperty.Create(nameof(PrefersLargeTitles), typeof(bool), typeof(Page), false);

		public static bool GetPrefersLargeTitles(BindableObject element)
		{
			return (bool)element.GetValue(PrefersLargeTitlesProperty);
		}

		public static void SetPrefersLargeTitles(BindableObject element, bool value)
		{
			element.SetValue(PrefersLargeTitlesProperty, value);
		}

		public static IPlatformElementConfiguration<iOS, FormsElement> SetPrefersLargeTitles(this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetPrefersLargeTitles(config.Element, value);
			return config;
		}

		public static bool PrefersLargeTitles(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetPrefersLargeTitles(config.Element);
		}
		#endregion

		#region HideNavigationBarSeparator
		public static readonly BindableProperty HideNavigationBarSeparatorProperty = BindableProperty.Create(nameof(HideNavigationBarSeparator), typeof(bool), typeof(Page), false);

		public static bool GetHideNavigationBarSeparator(BindableObject element)
		{
			return (bool)element.GetValue(HideNavigationBarSeparatorProperty);
		}

		public static void SetHideNavigationBarSeparator(BindableObject element, bool value)
		{
			element.SetValue(HideNavigationBarSeparatorProperty, value);
		}

		public static IPlatformElementConfiguration<iOS, FormsElement> SetHideNavigationBarSeparator(this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetHideNavigationBarSeparator(config.Element, value);
			return config;
		}

		public static bool HideNavigationBarSeparator(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetHideNavigationBarSeparator(config.Element);
		}
		#endregion
	}
}