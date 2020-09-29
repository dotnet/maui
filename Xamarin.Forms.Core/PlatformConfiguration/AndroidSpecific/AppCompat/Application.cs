namespace Xamarin.Forms.PlatformConfiguration.AndroidSpecific.AppCompat
{
	using FormsElement = Forms.Application;

	public static class Application
	{
		public static readonly BindableProperty SendDisappearingEventOnPauseProperty = BindableProperty.Create(nameof(SendDisappearingEventOnPause), typeof(bool), typeof(Application), true);

		public static bool GetSendDisappearingEventOnPause(BindableObject element)
		{
			return (bool)element.GetValue(SendDisappearingEventOnPauseProperty);
		}

		public static void SetSendDisappearingEventOnPause(BindableObject element, bool value)
		{
			element.SetValue(SendDisappearingEventOnPauseProperty, value);
		}

		public static bool GetSendDisappearingEventOnPause(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetSendDisappearingEventOnPause(config.Element);
		}

		public static IPlatformElementConfiguration<Android, FormsElement> SendDisappearingEventOnPause(this IPlatformElementConfiguration<Android, FormsElement> config, bool value)
		{
			SetSendDisappearingEventOnPause(config.Element, value);
			return config;
		}

		public static readonly BindableProperty SendAppearingEventOnResumeProperty = BindableProperty.Create(nameof(SendAppearingEventOnResume), typeof(bool), typeof(Application), true);

		public static bool GetSendAppearingEventOnResume(BindableObject element)
		{
			return (bool)element.GetValue(SendAppearingEventOnResumeProperty);
		}

		public static void SetSendAppearingEventOnResume(BindableObject element, bool value)
		{
			element.SetValue(SendAppearingEventOnResumeProperty, value);
		}

		public static bool GetSendAppearingEventOnResume(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetSendAppearingEventOnResume(config.Element);
		}

		public static IPlatformElementConfiguration<Android, FormsElement> SendAppearingEventOnResume(this IPlatformElementConfiguration<Android, FormsElement> config, bool value)
		{
			SetSendAppearingEventOnResume(config.Element, value);
			return config;
		}

		public static readonly BindableProperty ShouldPreserveKeyboardOnResumeProperty = BindableProperty.Create(nameof(ShouldPreserveKeyboardOnResume), typeof(bool), typeof(Application), false);

		public static bool GetShouldPreserveKeyboardOnResume(BindableObject element)
		{
			return (bool)element.GetValue(ShouldPreserveKeyboardOnResumeProperty);
		}

		public static void SetShouldPreserveKeyboardOnResume(BindableObject element, bool value)
		{
			element.SetValue(ShouldPreserveKeyboardOnResumeProperty, value);
		}

		public static bool GetShouldPreserveKeyboardOnResume(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetShouldPreserveKeyboardOnResume(config.Element);
		}

		public static IPlatformElementConfiguration<Android, FormsElement> ShouldPreserveKeyboardOnResume(this IPlatformElementConfiguration<Android, FormsElement> config, bool value)
		{
			SetShouldPreserveKeyboardOnResume(config.Element, value);
			return config;
		}
	}
}