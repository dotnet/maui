#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific
{
	using FormsElement = Maui.Controls.SwipeView;

	/// <summary>Controls the swipe transition animation mode for SwipeView on Android.</summary>
	public static class SwipeView
	{
		/// <summary>Bindable property for <see cref="SwipeTransitionMode"/>.</summary>
		public static readonly BindableProperty SwipeTransitionModeProperty = BindableProperty.Create("SwipeTransitionMode", typeof(SwipeTransitionMode), typeof(SwipeView), SwipeTransitionMode.Reveal);

		/// <summary>Gets the swipe transition animation mode on Android.</summary>
		/// <param name="element">The SwipeView element.</param>
		/// <returns>The swipe transition mode (Reveal or Drag).</returns>
		public static SwipeTransitionMode GetSwipeTransitionMode(BindableObject element)
		{
			return (SwipeTransitionMode)element.GetValue(SwipeTransitionModeProperty);
		}

		/// <summary>Sets the swipe transition animation mode on Android.</summary>
		/// <param name="element">The SwipeView element.</param>
		/// <param name="value">The swipe transition mode (Reveal or Drag).</param>
		public static void SetSwipeTransitionMode(BindableObject element, SwipeTransitionMode value)
		{
			element.SetValue(SwipeTransitionModeProperty, value);
		}

		/// <summary>Gets the swipe transition animation mode on Android.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns>The swipe transition mode (Reveal or Drag).</returns>
		public static SwipeTransitionMode GetSwipeTransitionMode(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetSwipeTransitionMode(config.Element);
		}

		/// <summary>Sets the swipe transition animation mode on Android.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="value">The swipe transition mode (Reveal or Drag).</param>
		/// <returns>The platform configuration for fluent API chaining.</returns>
		public static IPlatformElementConfiguration<Android, FormsElement> SetSwipeTransitionMode(this IPlatformElementConfiguration<Android, FormsElement> config, SwipeTransitionMode value)
		{
			SetSwipeTransitionMode(config.Element, value);
			return config;
		}
	}
}
