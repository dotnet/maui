namespace Xamarin.Forms.PlatformConfiguration.iOSSpecific
{
	using FormsElement = Forms.Application;

	public static class Application
	{
		public static readonly BindableProperty PanGestureRecognizerShouldRecognizeSimultaneouslyProperty = BindableProperty.Create("PanGestureRecognizerShouldRecognizeSimultaneously", typeof(bool), typeof(Application), false);

		public static bool GetPanGestureRecognizerShouldRecognizeSimultaneously(BindableObject element)
		{
			return (bool)element.GetValue(PanGestureRecognizerShouldRecognizeSimultaneouslyProperty);
		}

		public static void SetPanGestureRecognizerShouldRecognizeSimultaneously(BindableObject element, bool value)
		{
			element.SetValue(PanGestureRecognizerShouldRecognizeSimultaneouslyProperty, value);
		}

		public static bool GetPanGestureRecognizerShouldRecognizeSimultaneously(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetPanGestureRecognizerShouldRecognizeSimultaneously(config.Element);
		}

		public static IPlatformElementConfiguration<iOS, FormsElement> SetPanGestureRecognizerShouldRecognizeSimultaneously(this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetPanGestureRecognizerShouldRecognizeSimultaneously(config.Element, value);
			return config;
		}
	}
}
