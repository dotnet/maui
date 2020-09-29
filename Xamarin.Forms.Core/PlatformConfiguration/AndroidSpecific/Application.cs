namespace Xamarin.Forms.PlatformConfiguration.AndroidSpecific
{
	using FormsElement = Forms.Application;

	public enum WindowSoftInputModeAdjust
	{
		Pan,
		Resize,
		Unspecified
	}

	public static class Application
	{
		public static readonly BindableProperty WindowSoftInputModeAdjustProperty =
			BindableProperty.Create("WindowSoftInputModeAdjust", typeof(WindowSoftInputModeAdjust),
			typeof(Application), WindowSoftInputModeAdjust.Pan);

		public static WindowSoftInputModeAdjust GetWindowSoftInputModeAdjust(BindableObject element)
		{
			return (WindowSoftInputModeAdjust)element.GetValue(WindowSoftInputModeAdjustProperty);
		}

		public static void SetWindowSoftInputModeAdjust(BindableObject element, WindowSoftInputModeAdjust value)
		{
			element.SetValue(WindowSoftInputModeAdjustProperty, value);
		}

		public static WindowSoftInputModeAdjust GetWindowSoftInputModeAdjust(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetWindowSoftInputModeAdjust(config.Element);
		}

		public static IPlatformElementConfiguration<Android, FormsElement> UseWindowSoftInputModeAdjust(this IPlatformElementConfiguration<Android, FormsElement> config, WindowSoftInputModeAdjust value)
		{
			SetWindowSoftInputModeAdjust(config.Element, value);
			return config;
		}
	}
}