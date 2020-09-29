namespace Xamarin.Forms.PlatformConfiguration.iOSSpecific
{
	using System.ComponentModel;
	using FormsElement = Forms.Page;

	public static class Page
	{
		public static readonly BindableProperty PrefersStatusBarHiddenProperty =
			BindableProperty.Create("PrefersStatusBarHidden", typeof(StatusBarHiddenMode), typeof(Page), StatusBarHiddenMode.Default);

		public static StatusBarHiddenMode GetPrefersStatusBarHidden(BindableObject element)
		{
			return (StatusBarHiddenMode)element.GetValue(PrefersStatusBarHiddenProperty);
		}

		public static void SetPrefersStatusBarHidden(BindableObject element, StatusBarHiddenMode value)
		{
			element.SetValue(PrefersStatusBarHiddenProperty, value);
		}

		public static StatusBarHiddenMode PrefersStatusBarHidden(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetPrefersStatusBarHidden(config.Element);
		}

		public static IPlatformElementConfiguration<iOS, FormsElement> SetPrefersStatusBarHidden(this IPlatformElementConfiguration<iOS, FormsElement> config, StatusBarHiddenMode value)
		{
			SetPrefersStatusBarHidden(config.Element, value);
			return config;
		}

		public static readonly BindableProperty PreferredStatusBarUpdateAnimationProperty =
			BindableProperty.Create("PreferredStatusBarUpdateAnimation", typeof(UIStatusBarAnimation), typeof(Page), UIStatusBarAnimation.None);

		public static UIStatusBarAnimation GetPreferredStatusBarUpdateAnimation(BindableObject element)
		{
			return (UIStatusBarAnimation)element.GetValue(PreferredStatusBarUpdateAnimationProperty);
		}

		public static void SetPreferredStatusBarUpdateAnimation(BindableObject element, UIStatusBarAnimation value)
		{
			if (value == UIStatusBarAnimation.Fade)
				element.SetValue(PreferredStatusBarUpdateAnimationProperty, value);
			else if (value == UIStatusBarAnimation.Slide)
				element.SetValue(PreferredStatusBarUpdateAnimationProperty, value);
			else
				element.SetValue(PreferredStatusBarUpdateAnimationProperty, value);
		}

		public static UIStatusBarAnimation PreferredStatusBarUpdateAnimation(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetPreferredStatusBarUpdateAnimation(config.Element);
		}

		public static IPlatformElementConfiguration<iOS, FormsElement> SetPreferredStatusBarUpdateAnimation(this IPlatformElementConfiguration<iOS, FormsElement> config, UIStatusBarAnimation value)
		{
			SetPreferredStatusBarUpdateAnimation(config.Element, value);
			return config;
		}

		public static readonly BindableProperty UseSafeAreaProperty = BindableProperty.Create("UseSafeArea", typeof(bool), typeof(Page), false);

		public static bool GetUseSafeArea(BindableObject element)
		{
			return (bool)element.GetValue(UseSafeAreaProperty);
		}

		public static void SetUseSafeArea(BindableObject element, bool value)
		{
			element.SetValue(UseSafeAreaProperty, value);
		}

		public static IPlatformElementConfiguration<iOS, FormsElement> SetUseSafeArea(this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetUseSafeArea(config.Element, value);
			return config;
		}

		public static bool UsingSafeArea(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetUseSafeArea(config.Element);
		}

		public static readonly BindableProperty LargeTitleDisplayProperty = BindableProperty.Create(nameof(LargeTitleDisplay), typeof(LargeTitleDisplayMode), typeof(Page), LargeTitleDisplayMode.Automatic);

		public static LargeTitleDisplayMode GetLargeTitleDisplay(BindableObject element)
		{
			return (LargeTitleDisplayMode)element.GetValue(LargeTitleDisplayProperty);
		}

		public static void SetLargeTitleDisplay(BindableObject element, LargeTitleDisplayMode value)
		{
			element.SetValue(LargeTitleDisplayProperty, value);
		}

		public static LargeTitleDisplayMode LargeTitleDisplay(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetLargeTitleDisplay(config.Element);
		}

		public static IPlatformElementConfiguration<iOS, FormsElement> SetLargeTitleDisplay(this IPlatformElementConfiguration<iOS, FormsElement> config, LargeTitleDisplayMode value)
		{
			SetLargeTitleDisplay(config.Element, value);
			return config;
		}

		static readonly BindablePropertyKey SafeAreaInsetsPropertyKey = BindableProperty.CreateReadOnly(nameof(SafeAreaInsets), typeof(Thickness), typeof(Page), default(Thickness));

		public static readonly BindableProperty SafeAreaInsetsProperty = SafeAreaInsetsPropertyKey.BindableProperty;

		public static Thickness GetSafeAreaInsets(BindableObject element)
		{
			return (Thickness)element.GetValue(SafeAreaInsetsProperty);
		}

		static void SetSafeAreaInsets(BindableObject element, Thickness value)
		{
			element.SetValue(SafeAreaInsetsPropertyKey, value);
		}

		public static Thickness SafeAreaInsets(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetSafeAreaInsets(config.Element);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IPlatformElementConfiguration<iOS, FormsElement> SetSafeAreaInsets(this IPlatformElementConfiguration<iOS, FormsElement> config, Thickness value)
		{
			SetSafeAreaInsets(config.Element, value);
			return config;
		}

		public static readonly BindableProperty ModalPresentationStyleProperty =
			BindableProperty.Create(nameof(ModalPresentationStyle), typeof(UIModalPresentationStyle), typeof(Page), UIModalPresentationStyle.FullScreen);

		public static UIModalPresentationStyle ModalPresentationStyle(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetModalPresentationStyle(config.Element);
		}

		public static IPlatformElementConfiguration<iOS, FormsElement> SetModalPresentationStyle(this IPlatformElementConfiguration<iOS, FormsElement> config, UIModalPresentationStyle value)
		{
			SetModalPresentationStyle(config.Element, value);
			return config;
		}

		public static UIModalPresentationStyle GetModalPresentationStyle(BindableObject element)
		{
			return (UIModalPresentationStyle)element.GetValue(ModalPresentationStyleProperty);
		}

		static void SetModalPresentationStyle(BindableObject element, UIModalPresentationStyle value)
		{
			element.SetValue(ModalPresentationStyleProperty, value);
		}

		public static readonly BindableProperty PrefersHomeIndicatorAutoHiddenProperty =
			BindableProperty.Create(nameof(PrefersHomeIndicatorAutoHidden), typeof(bool), typeof(Page), false);

		public static bool GetPrefersHomeIndicatorAutoHidden(BindableObject element)
		{
			return (bool)element.GetValue(PrefersHomeIndicatorAutoHiddenProperty);
		}

		public static void SetPrefersHomeIndicatorAutoHidden(BindableObject element, bool value)
		{
			element.SetValue(PrefersHomeIndicatorAutoHiddenProperty, value);
		}

		public static bool PrefersHomeIndicatorAutoHidden(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetPrefersHomeIndicatorAutoHidden(config.Element);
		}

		public static IPlatformElementConfiguration<iOS, FormsElement> SetPrefersHomeIndicatorAutoHidden(this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetPrefersHomeIndicatorAutoHidden(config.Element, value);
			return config;
		}
	}
}