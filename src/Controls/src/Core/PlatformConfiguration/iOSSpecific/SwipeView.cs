#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific
{
	using FormsElement = Maui.Controls.SwipeView;

	/// <summary>Provides iOS-specific configuration for SwipeView transition animations.</summary>
	public static class SwipeView
	{
		/// <summary>Bindable property for <see cref="SwipeTransitionMode"/>.</summary>
		public static readonly BindableProperty SwipeTransitionModeProperty = BindableProperty.Create("SwipeTransitionMode", typeof(SwipeTransitionMode), typeof(SwipeView), SwipeTransitionMode.Reveal);

		/// <summary>Gets the swipe transition mode on iOS.</summary>
		/// <param name="element">The element to get the value from.</param>
		/// <returns>The swipe transition mode.</returns>
		public static SwipeTransitionMode GetSwipeTransitionMode(BindableObject element)
		{
			return (SwipeTransitionMode)element.GetValue(SwipeTransitionModeProperty);
		}

		/// <summary>Sets the swipe transition mode on iOS.</summary>
		/// <param name="element">The element to set the value on.</param>
		/// <param name="value">The swipe transition mode to apply.</param>
		public static void SetSwipeTransitionMode(BindableObject element, SwipeTransitionMode value)
		{
			element.SetValue(SwipeTransitionModeProperty, value);
		}

		/// <summary>Gets the swipe transition mode on iOS.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns>The swipe transition mode.</returns>
		public static SwipeTransitionMode GetSwipeTransitionMode(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetSwipeTransitionMode(config.Element);
		}

		/// <summary>Sets the swipe transition mode on iOS.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="value">The swipe transition mode to apply.</param>
		/// <returns>The updated platform configuration.</returns>
		public static IPlatformElementConfiguration<iOS, FormsElement> SetSwipeTransitionMode(this IPlatformElementConfiguration<iOS, FormsElement> config, SwipeTransitionMode value)
		{
			SetSwipeTransitionMode(config.Element, value);
			return config;
		}
	}
}
