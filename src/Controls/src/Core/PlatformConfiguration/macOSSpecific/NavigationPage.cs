#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.macOSSpecific
{
	using FormsElement = Maui.Controls.NavigationPage;

	/// <summary>Provides macOS-specific platform configuration for <see cref="Maui.Controls.NavigationPage"/>.</summary>
	public static class NavigationPage
	{
		/// <summary>Bindable property for attached property <c>NavigationTransitionPushStyle</c>.</summary>
		public static readonly BindableProperty NavigationTransitionPushStyleProperty = BindableProperty.Create("NavigationTransitionPushStyle", typeof(NavigationTransitionStyle), typeof(NavigationPage), NavigationTransitionStyle.SlideForward);
		/// <summary>Bindable property for attached property <c>NavigationTransitionPopStyle</c>.</summary>
		public static readonly BindableProperty NavigationTransitionPopStyleProperty = BindableProperty.Create("NavigationTransitionPopStyle", typeof(NavigationTransitionStyle), typeof(NavigationPage), NavigationTransitionStyle.SlideBackward);

		#region PushStyle
		/// <summary>Gets the navigation transition style used when pushing pages onto the navigation stack.</summary>
		/// <param name="element">The element whose transition style to get.</param>
		/// <returns>The navigation transition push style for the element.</returns>
		public static NavigationTransitionStyle GetNavigationTransitionPushStyle(BindableObject element)
		{
			return (NavigationTransitionStyle)element.GetValue(NavigationTransitionPushStyleProperty);
		}

		/// <summary>Sets the navigation transition style used when pushing pages onto the navigation stack.</summary>
		/// <param name="element">The element whose transition style to set.</param>
		/// <param name="value">The navigation transition style to use for push operations.</param>
		public static void SetNavigationTransitionPushStyle(BindableObject element, NavigationTransitionStyle value)
		{
			element.SetValue(NavigationTransitionPushStyleProperty, value);
		}

		/// <summary>Gets the navigation transition style used when pushing pages onto the navigation stack.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns>The navigation transition push style for the element.</returns>
		public static NavigationTransitionStyle GetNavigationTransitionPushStyle(this IPlatformElementConfiguration<macOS, FormsElement> config)
		{
			return GetNavigationTransitionPushStyle(config.Element);
		}
		#endregion

		#region PopStyle
		/// <summary>Gets the navigation transition style used when popping pages from the navigation stack.</summary>
		/// <param name="element">The element whose transition style to get.</param>
		/// <returns>The navigation transition pop style for the element.</returns>
		public static NavigationTransitionStyle GetNavigationTransitionPopStyle(BindableObject element)
		{
			return (NavigationTransitionStyle)element.GetValue(NavigationTransitionPopStyleProperty);
		}

		/// <summary>Sets the navigation transition style used when popping pages from the navigation stack.</summary>
		/// <param name="element">The element whose transition style to set.</param>
		/// <param name="value">The navigation transition style to use for pop operations.</param>
		public static void SetNavigationTransitionPopStyle(BindableObject element, NavigationTransitionStyle value)
		{
			element.SetValue(NavigationTransitionPopStyleProperty, value);
		}

		/// <summary>Gets the navigation transition style used when popping pages from the navigation stack.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns>The navigation transition pop style for the element.</returns>
		public static NavigationTransitionStyle GetNavigationTransitionPopStyle(this IPlatformElementConfiguration<macOS, FormsElement> config)
		{
			return GetNavigationTransitionPopStyle(config.Element);
		}
		#endregion

		/// <summary>Sets both the push and pop navigation transition styles.</summary>
		/// <param name="element">The element whose transition styles to set.</param>
		/// <param name="pushStyle">The navigation transition style for push operations.</param>
		/// <param name="popStyle">The navigation transition style for pop operations.</param>
		public static void SetNavigationTransitionStyle(BindableObject element, NavigationTransitionStyle pushStyle, NavigationTransitionStyle popStyle)
		{
			SetNavigationTransitionPushStyle(element, pushStyle);
			SetNavigationTransitionPopStyle(element, popStyle);
		}

		/// <summary>Sets both the push and pop navigation transition styles.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="pushStyle">The navigation transition style for push operations.</param>
		/// <param name="popStyle">The navigation transition style for pop operations.</param>
		/// <returns>The platform configuration for fluent chaining.</returns>
		public static IPlatformElementConfiguration<macOS, FormsElement> SetNavigationTransitionStyle(this IPlatformElementConfiguration<macOS, FormsElement> config, NavigationTransitionStyle pushStyle, NavigationTransitionStyle popStyle)
		{
			SetNavigationTransitionStyle(config.Element, pushStyle, popStyle);
			return config;
		}
	}
}
