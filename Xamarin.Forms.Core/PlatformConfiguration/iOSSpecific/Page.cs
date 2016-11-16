namespace Xamarin.Forms.PlatformConfiguration.iOSSpecific
{
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
	}
}
