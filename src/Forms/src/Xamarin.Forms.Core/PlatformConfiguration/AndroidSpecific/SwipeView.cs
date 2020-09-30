namespace Xamarin.Forms.PlatformConfiguration.AndroidSpecific
{
	using FormsElement = Forms.SwipeView;

	public enum SwipeTransitionMode
	{
		Reveal = 0,
		Drag = 1
	}

	public static class SwipeView
	{
		public static readonly BindableProperty SwipeTransitionModeProperty = BindableProperty.Create("SwipeTransitionMode", typeof(SwipeTransitionMode), typeof(SwipeView), SwipeTransitionMode.Reveal);

		public static SwipeTransitionMode GetSwipeTransitionMode(BindableObject element)
		{
			return (SwipeTransitionMode)element.GetValue(SwipeTransitionModeProperty);
		}

		public static void SetSwipeTransitionMode(BindableObject element, SwipeTransitionMode value)
		{
			element.SetValue(SwipeTransitionModeProperty, value);
		}

		public static SwipeTransitionMode GetSwipeTransitionMode(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetSwipeTransitionMode(config.Element);
		}

		public static IPlatformElementConfiguration<Android, FormsElement> SetSwipeTransitionMode(this IPlatformElementConfiguration<Android, FormsElement> config, SwipeTransitionMode value)
		{
			SetSwipeTransitionMode(config.Element, value);
			return config;
		}
	}
}