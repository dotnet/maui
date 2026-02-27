#nullable disable
using System.ComponentModel;

namespace Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific
{
	using FormsElement = Maui.Controls.Page;

	/// <summary>
	/// The page instance that Microsoft.Maui.Controls created on the iOS platform.
	/// </summary>
	public static class Page
	{
		/// <summary>
		/// Sets a value that controls whether it is preferred that the status bar is shown, hidden, or relies on the system default.
		/// </summary>
		public static readonly BindableProperty PrefersStatusBarHiddenProperty =
			BindableProperty.Create("PrefersStatusBarHidden", typeof(StatusBarHiddenMode), typeof(Page), StatusBarHiddenMode.Default);

		/// <summary>
		/// Returns a value that tells whether it is preferred that the status bar is shown, hidden, or relies on the system default.
		/// </summary>
		/// <param name="element">The platform specific element on which to perform the operation.</param>
		/// <returns>A value that tells whether it is preferred that the status bar is shown, hidden, or relies on the system default.</returns>
		public static StatusBarHiddenMode GetPrefersStatusBarHidden(BindableObject element)
		{
			return (StatusBarHiddenMode)element.GetValue(PrefersStatusBarHiddenProperty);
		}

		/// <summary>
		/// Sets a value that controls whether it is preferred that the status bar is shown, hidden, or relies on the system default.
		/// </summary>
		/// <param name="element">The platform specific element on which to perform the operation.</param>
		/// <param name="value">The new property value to assign.</param>
		public static void SetPrefersStatusBarHidden(BindableObject element, StatusBarHiddenMode value)
		{
			element.SetValue(PrefersStatusBarHiddenProperty, value);
		}

		/// <summary>
		/// Sets a value that controls whether it is preferred that the status bar is shown, hidden, or relies on the system default.
		/// </summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <returns>A value that controls whether it is preferred that the status bar is shown, hidden, or relies on the system default.</returns>
		public static StatusBarHiddenMode PrefersStatusBarHidden(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetPrefersStatusBarHidden(config.Element);
		}

		/// <summary>
		/// Sets a value that controls whether it is preferred that the status bar is shown, hidden, or relies on the system default.
		/// </summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <param name="value">The new property value to assign.</param>
		/// <returns>The updated configuration object on which developers can make successive method calls.</returns>
		public static IPlatformElementConfiguration<iOS, FormsElement> SetPrefersStatusBarHidden(this IPlatformElementConfiguration<iOS, FormsElement> config, StatusBarHiddenMode value)
		{
			SetPrefersStatusBarHidden(config.Element, value);
			return config;
		}

		/// <summary>
		/// Backing store for the attached property that controls whether the preferred animation style to use when updating the status bar is <c>None</c>, <c>Slide</c>, or <c>Fade</c>.
		/// </summary>
		public static readonly BindableProperty PreferredStatusBarUpdateAnimationProperty =
			BindableProperty.Create("PreferredStatusBarUpdateAnimation", typeof(UIStatusBarAnimation), typeof(Page), UIStatusBarAnimation.None);

		/// <summary>
		/// Returns a value that tells whether the preferred animation style to use when updating the status bar is <c>None</c>, <c>Slide</c>, or <c>Fade</c>.
		/// </summary>
		/// <param name="element">The platform specific element on which to perform the operation.</param>
		/// <returns>A value that tells whether the preferred animation style to use when updating the status bar is <c>None</c>, <c>Slide</c>, or <c>Fade</c>.</returns>
		public static UIStatusBarAnimation GetPreferredStatusBarUpdateAnimation(BindableObject element)
		{
			return (UIStatusBarAnimation)element.GetValue(PreferredStatusBarUpdateAnimationProperty);
		}

		/// <summary>
		/// Sets a value that controls whether the preferred animation style to use when updating the status bar is <c>None</c>, <c>Slide</c>, or <c>Fade</c>.
		/// </summary>
		/// <param name="element">The platform specific element on which to perform the operation.</param>
		/// <param name="value">The new property value to assign.</param>
		public static void SetPreferredStatusBarUpdateAnimation(BindableObject element, UIStatusBarAnimation value)
		{
			if (value == UIStatusBarAnimation.Fade)
				element.SetValue(PreferredStatusBarUpdateAnimationProperty, value);
			else if (value == UIStatusBarAnimation.Slide)
				element.SetValue(PreferredStatusBarUpdateAnimationProperty, value);
			else
				element.SetValue(PreferredStatusBarUpdateAnimationProperty, value);
		}

		/// <summary>
		/// Returns a value that tells whether the preferred animation style to use when updating the status bar is <c>None</c>, <c>Slide</c>, or <c>Fade</c>.
		/// </summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <returns>A value that tells whether the preferred animation style to use when updating the status bar is <c>None</c>, <c>Slide</c>, or <c>Fade</c>.</returns>
		public static UIStatusBarAnimation PreferredStatusBarUpdateAnimation(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetPreferredStatusBarUpdateAnimation(config.Element);
		}

		/// <summary>
		/// Sets a value that controls whether the preferred animation style to use when updating the status bar is <c>None</c>, <c>Slide</c>, or <c>Fade</c>.
		/// </summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <param name="value">The new property value to assign.</param>
		/// <returns>The updated configuration object on which developers can make successive method calls.</returns>
		public static IPlatformElementConfiguration<iOS, FormsElement> SetPreferredStatusBarUpdateAnimation(this IPlatformElementConfiguration<iOS, FormsElement> config, UIStatusBarAnimation value)
		{
			SetPreferredStatusBarUpdateAnimation(config.Element, value);
			return config;
		}

		/// <summary>
		/// This iOS platform-specific controls whether padding values are overridden with the safe area insets.
		/// </summary>
		/// <remarks>
		/// This API is deprecated. Use SafeAreaEdges attached property instead for per-edge safe area control.
		/// </remarks>
		[System.Obsolete("Use SafeAreaEdges attached property instead for per-edge safe area control.")]
#if MACCATALYST
		public static readonly BindableProperty UseSafeAreaProperty = BindableProperty.Create("UseSafeArea", typeof(bool), typeof(Page), true);
#else
		public static readonly BindableProperty UseSafeAreaProperty = BindableProperty.Create("UseSafeArea", typeof(bool), typeof(Page), false);
#endif

		/// <summary>
		/// Gets a value that indicates whether padding values are overridden with values that conform to the safe area on the device.
		/// </summary>
		/// <param name="element">The element to get the safe area behavior from.</param>
		/// <returns><see langword="true"/> if the padding values are overridden; otherwise, <see langword="false"/>.</returns>
		/// <remarks>
		/// This API is deprecated. Use SafeAreaEdges attached property instead for per-edge safe area control.
		/// </remarks>
		[System.Obsolete("Use SafeAreaEdges attached property instead for per-edge safe area control.")]
		public static bool GetUseSafeArea(BindableObject element)
		{
			return (bool)element.GetValue(UseSafeAreaProperty);
		}

		/// <summary>
		/// Sets a value that controls whether padding values are overridden with the safe area insets.
		/// </summary>
		/// <param name="element">The element whose safe area use behavior to set.</param>
		/// <param name="value"><see langword="true"/> to use the safe area inset behavior; otherwise, <see langword="false"/>.</param>
		/// <remarks>
		/// This API is deprecated. Use SafeAreaEdges attached property instead for per-edge safe area control.
		/// </remarks>
		[System.Obsolete("Use SafeAreaEdges attached property instead for per-edge safe area control.")]
		public static void SetUseSafeArea(BindableObject element, bool value)
		{
			element.SetValue(UseSafeAreaProperty, value);
		}

		/// <summary>
		/// Sets a value that controls whether padding values are overridden with the safe area insets.
		/// </summary>
		/// <param name="config">The element whose safe area behavior to get.</param>
		/// <param name="value"><see langword="true"/> to use the safe area inset behavior; otherwise, <see langword="false"/>.</param>
		/// <returns>The updated configuration object on which developers can make successive method calls.</returns>
		/// <remarks>
		/// This API is deprecated. Use SafeAreaEdges attached property instead for per-edge safe area control.
		/// </remarks>
		[System.Obsolete("Use SafeAreaEdges attached property instead for per-edge safe area control.")]
		public static IPlatformElementConfiguration<iOS, FormsElement> SetUseSafeArea(this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetUseSafeArea(config.Element, value);
			return config;
		}

		/// <summary>
		/// Gets a value that represents whether the padding is overridden with the safe area.
		/// </summary>
		/// <param name="config">The element whose safe area behavior to get.</param>
		/// <returns><see langword="true"/> if the padding is overridden with the safe area; otherwise, <see langword="false"/>.</returns>
		/// <remarks>
		/// This API is deprecated. Use SafeAreaEdges attached property instead for per-edge safe area control.
		/// </remarks>
		[System.Obsolete("Use SafeAreaEdges attached property instead for per-edge safe area control.")]
		public static bool UsingSafeArea(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetUseSafeArea(config.Element);
		}

		/// <summary>Bindable property for <see cref="LargeTitleDisplay"/>.</summary>
		public static readonly BindableProperty LargeTitleDisplayProperty = BindableProperty.Create(nameof(LargeTitleDisplay), typeof(LargeTitleDisplayMode), typeof(Page), LargeTitleDisplayMode.Automatic);

		/// <summary>
		/// Returns the large title display preferences for <paramref name="element" />.
		/// </summary>
		/// <param name="element">The element whose large title display preferences to get.</param>
		/// <returns>The large title display preferences for <paramref name="element" />.</returns>
		public static LargeTitleDisplayMode GetLargeTitleDisplay(BindableObject element)
		{
			return (LargeTitleDisplayMode)element.GetValue(LargeTitleDisplayProperty);
		}

		/// <summary>
		/// Sets the large title display preferences of <paramref name="element" /> to <paramref name="value" />.
		/// </summary>
		/// <param name="element">The element whose large title display preference to set.</param>
		/// <param name="value">The new large title display preferences.</param>
		public static void SetLargeTitleDisplay(BindableObject element, LargeTitleDisplayMode value)
		{
			element.SetValue(LargeTitleDisplayProperty, value);
		}

		/// <summary>
		/// Returns a value that describes the large title behavior preference of <paramref name="config" />.
		/// </summary>
		/// <param name="config">The element whose large title preferences to return.</param>
		/// <returns>A value that describes the large title behavior preference of <paramref name="config" />.</returns>
		public static LargeTitleDisplayMode LargeTitleDisplay(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetLargeTitleDisplay(config.Element);
		}

		/// <summary>
		/// Sets the large title display preferences of <paramref name="config" /> to <paramref name="value" />.
		/// </summary>
		/// <param name="config">The element whose large title display preference to set.</param>
		/// <param name="value">The new large title display preferences.</param>
		/// <returns>The updated configuration object on which developers can make successive method calls.</returns>
		public static IPlatformElementConfiguration<iOS, FormsElement> SetLargeTitleDisplay(this IPlatformElementConfiguration<iOS, FormsElement> config, LargeTitleDisplayMode value)
		{
			SetLargeTitleDisplay(config.Element, value);
			return config;
		}

		static readonly BindablePropertyKey SafeAreaInsetsPropertyKey = BindableProperty.CreateReadOnly(nameof(SafeAreaInsets), typeof(Thickness), typeof(Page), default(Thickness));

		/// <summary>
		/// Backing store for the attached property that represents the safe area insets.
		/// </summary>
		public static readonly BindableProperty SafeAreaInsetsProperty = SafeAreaInsetsPropertyKey.BindableProperty;

		/// <summary>
		/// Returns the safe area insets for <paramref name="element" />.
		/// </summary>
		/// <param name="element">The element whose safe area insets to get.</param>
		/// <returns>The safe area insets for <paramref name="element" />.</returns>
		public static Thickness GetSafeAreaInsets(BindableObject element)
		{
			return (Thickness)element.GetValue(SafeAreaInsetsProperty);
		}

		static void SetSafeAreaInsets(BindableObject element, Thickness value)
		{
			element.SetValue(SafeAreaInsetsPropertyKey, value);
		}

		/// <summary>
		/// Returns a <see cref="T:Microsoft.Maui.Thickness" /> object that represents the safe area insets.
		/// </summary>
		/// <param name="config">The element whose safe area insets to return.</param>
		/// <returns>A <see cref="T:Microsoft.Maui.Thickness" /> object that represents the safe area insets.</returns>
		public static Thickness SafeAreaInsets(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetSafeAreaInsets(config.Element);
		}

		/// <summary>
		/// Sets the safe area insets of <paramref name="config" /> to <paramref name="value" />.
		/// </summary>
		/// <param name="config">The element whose safe area insets to set.</param>
		/// <param name="value">The new safe area insets.</param>
		/// <returns>The updated configuration object on which developers can make successive method calls.</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IPlatformElementConfiguration<iOS, FormsElement> SetSafeAreaInsets(this IPlatformElementConfiguration<iOS, FormsElement> config, Thickness value)
		{
			SetSafeAreaInsets(config.Element, value);
			return config;
		}

		/// <summary>
		/// Defines the modal presentation style of the <see cref="Page"/>.
		/// </summary>
		public static readonly BindableProperty ModalPresentationStyleProperty =
			BindableProperty.Create(nameof(ModalPresentationStyle), typeof(UIModalPresentationStyle), typeof(Page), UIModalPresentationStyle.FullScreen);

		/// <summary>
		/// Defines the popover source of the modal <see cref="Page"/>.
		/// </summary>
		public static readonly BindableProperty ModalPopoverSourceViewProperty =
			BindableProperty.Create(nameof(ModalPopoverSourceView), typeof(View), typeof(Page), null);

		/// <summary>
		/// Defines the rect within the popover source of the modal <see cref="Page"/>.
		/// </summary>
		public static readonly BindableProperty ModalPopoverRectProperty =
			BindableProperty.Create(nameof(ModalPopoverRect), typeof(System.Drawing.Rectangle), typeof(Page), System.Drawing.Rectangle.Empty);


		/// <summary>
		/// Gets the modal presentation style of the <see cref="Page"/>.
		/// </summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <returns>The modal presentation style.</returns>
		public static UIModalPresentationStyle ModalPresentationStyle(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetModalPresentationStyle(config.Element);
		}

		/// <summary>
		/// Gets the source view of the popover, if the ModalPresentationStyle is set to <see cref="UIModalPresentationStyle.Popover"/><see cref="Page"/>.
		/// </summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <returns>The source view for the modal, if it's a popover.</returns>
		public static View ModalPopoverSourceView(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetPopoverSourceView(config.Element);
		}

		/// <summary>
		/// Gets the source view of the popover, if the ModalPresentationStyle is set to <see cref="UIModalPresentationStyle.Popover"/><see cref="Page"/>.
		/// </summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <returns>The rectangle within the source view of the modal</returns>
		public static System.Drawing.Rectangle ModalPopoverRect(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetPopoverRect(config.Element);
		}
		/// <summary>
		/// Sets the modal presentation style of the <see cref="Page"/>.
		/// </summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <param name="value">The modal presentation style.</param>
		/// <returns>The platform specific configuration that contains the element on which to perform the operation.</returns>
		public static IPlatformElementConfiguration<iOS, FormsElement> SetModalPresentationStyle(this IPlatformElementConfiguration<iOS, FormsElement> config, UIModalPresentationStyle value)
		{
			SetModalPresentationStyle(config.Element, value);
			return config;
		}

		/// <summary>
		/// Sets the source view of the popover, if the ModalPresentationStyle is set to <see cref="UIModalPresentationStyle.Popover"/><see cref="Page"/>.
		/// </summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <param name="value">The Microsoft.Maui.Controls.View from which the modal will originate</param>
		/// <returns>The modal presentation style.</returns>
		public static IPlatformElementConfiguration<iOS, FormsElement> SetModalPopoverView(this IPlatformElementConfiguration<iOS, FormsElement> config, View value)
		{
			SetModalPopoverView(config.Element, value);
			return config;
		}

		/// <summary>
		/// Sets the rectangle within the popoverview from which the popover will originate, if the ModalPresentationStyle is set to <see cref="UIModalPresentationStyle.Popover"/><see cref="Page"/>.
		/// </summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <param name="value">The Rectangle within the view from which the modal will originate</param>
		/// <returns>The modal presentation style.</returns>
		public static IPlatformElementConfiguration<iOS, FormsElement> SetModalPopoverRect(this IPlatformElementConfiguration<iOS, FormsElement> config, System.Drawing.Rectangle value)
		{
			SetModalPopoverRect(config.Element, value);
			return config;
		}

		/// <summary>
		/// Gets the current value of the <see cref="UIModalPresentationStyle"/> enumeration that's applied to the <see cref="Page"/>.
		/// </summary>
		/// <param name="element">The <see cref="BindableObject" /> whose modal presentation style is being retrieved.</param>
		/// <returns>The current value of the <see cref="UIModalPresentationStyle" /> enumeration that's applied to the <paramref name="element" />.</returns>
		public static UIModalPresentationStyle GetModalPresentationStyle(BindableObject element)
		{
			return (UIModalPresentationStyle)element.GetValue(ModalPresentationStyleProperty);
		}

		/// <summary>
		/// Gets the current value of the <see cref="UIModalPresentationStyle"/> enumeration that's applied to the <see cref="Page"/>.
		/// </summary>
		/// <param name="element">The <see cref="BindableObject" /> whose modal presentation style is being retrieved.</param>
		/// <returns>The current value of the <see cref="UIModalPresentationStyle" /> enumeration that's applied to the <paramref name="element" />.</returns>
		public static View GetPopoverSourceView(BindableObject element)
		{
			return (View)element.GetValue(ModalPopoverSourceViewProperty);
		}

		/// <summary>
		/// Gets the current value of the <see cref="UIModalPresentationStyle"/> enumeration that's applied to the <see cref="Page"/>.
		/// </summary>
		/// <param name="element">The <see cref="BindableObject" /> whose modal presentation style is being retrieved.</param>
		/// <returns>The current value of the <see cref="UIModalPresentationStyle" /> enumeration that's applied to the <paramref name="element" />.</returns>
		public static System.Drawing.Rectangle GetPopoverRect(BindableObject element)
		{
			return (System.Drawing.Rectangle)element.GetValue(ModalPopoverRectProperty);
		}

		/// <summary>
		/// Sets the modal presentation style on a <see cref="Page"/>.
		/// </summary>
		/// <param name="element">A page, the VisualElement that occupies the entire screen.</param>
		/// <param name="value">The modal presentation style.</param>
		static void SetModalPresentationStyle(BindableObject element, UIModalPresentationStyle value)
		{
			element.SetValue(ModalPresentationStyleProperty, value);
		}

		/// <summary>
		/// Sets the popover source view for a modal <see cref="Page"/>.
		/// </summary>
		/// <param name="element">A page, the VisualElement that occupies the entire screen.</param>
		/// <param name="value">The view from which the popover originates.</param>
		static void SetModalPopoverView(BindableObject element, View value)
		{
			element.SetValue(ModalPopoverSourceViewProperty, value);
		}

		/// <summary>
		/// Sets the rectangle within the popover source view for a modal <see cref="Page"/>.
		/// </summary>
		/// <param name="element">A page, the VisualElement that occupies the entire screen.</param>
		/// <param name="value">The rectangle within the view from which the popover originates.</param>
		static void SetModalPopoverRect(BindableObject element, System.Drawing.Rectangle value)
		{
			element.SetValue(ModalPopoverRectProperty, value);
		}

		/// <summary>Bindable property for <see cref="PrefersHomeIndicatorAutoHidden"/>.</summary>
		public static readonly BindableProperty PrefersHomeIndicatorAutoHiddenProperty =
			BindableProperty.Create(nameof(PrefersHomeIndicatorAutoHidden), typeof(bool), typeof(Page), false);

		/// <summary>
		/// Gets a value that indicates whether the visual indicator should hide upon returning to the home screen.
		/// </summary>
		/// <param name="element">A page, the VisualElement that occupies the entire screen.</param>
		/// <returns><see langword="true"/> if the home visual indicator is hidden; otherwise, <see langword="false"/>.</returns>
		public static bool GetPrefersHomeIndicatorAutoHidden(BindableObject element)
		{
			return (bool)element.GetValue(PrefersHomeIndicatorAutoHiddenProperty);
		}

		/// <summary>
		/// Sets a value that indicates whether the visual indicator should hide upon returning to the home screen.
		/// </summary>
		/// <param name="element">A page, the VisualElement that occupies the entire screen.</param>
		/// <param name="value"><see langword="true"/> if hide the home indicator; otherwise, <see langword="false"/>.</param>
		public static void SetPrefersHomeIndicatorAutoHidden(BindableObject element, bool value)
		{
			element.SetValue(PrefersHomeIndicatorAutoHiddenProperty, value);
		}

		/// <summary>
		/// Gets a Boolean that indicates whether is allowed to hide the visual indicator for returning to the Home Screen.
		/// </summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <returns><see langword="true"/> if hide the home indicator; otherwise, <see langword="false"/>.</returns>
		public static bool PrefersHomeIndicatorAutoHidden(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetPrefersHomeIndicatorAutoHidden(config.Element);
		}

		/// <summary>
		/// Sets a Boolean that indicates whether is allowed to hide the visual indicator for returning to the Home Screen.
		/// </summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <param name="value"><see langword="true"/> if hide the home indicator; otherwise, <see langword="false"/>.</param>
		/// <returns>The updated configuration object on which developers can make successive method calls.</returns>
		public static IPlatformElementConfiguration<iOS, FormsElement> SetPrefersHomeIndicatorAutoHidden(this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetPrefersHomeIndicatorAutoHidden(config.Element, value);
			return config;
		}
	}
}