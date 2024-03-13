#nullable disable
using System.ComponentModel;

namespace Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific
{
	using FormsElement = Maui.Controls.Page;

	/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Page.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page']/Docs/*" />
	public static class Page
	{
		/// <summary>Bindable property for <see cref="PrefersStatusBarHidden"/>.</summary>
		public static readonly BindableProperty PrefersStatusBarHiddenProperty =
			BindableProperty.Create("PrefersStatusBarHidden", typeof(StatusBarHiddenMode), typeof(Page), StatusBarHiddenMode.Default);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Page.xml" path="//Member[@MemberName='GetPrefersStatusBarHidden']/Docs/*" />
		public static StatusBarHiddenMode GetPrefersStatusBarHidden(BindableObject element)
		{
			return (StatusBarHiddenMode)element.GetValue(PrefersStatusBarHiddenProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Page.xml" path="//Member[@MemberName='SetPrefersStatusBarHidden'][1]/Docs/*" />
		public static void SetPrefersStatusBarHidden(BindableObject element, StatusBarHiddenMode value)
		{
			element.SetValue(PrefersStatusBarHiddenProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Page.xml" path="//Member[@MemberName='PrefersStatusBarHidden']/Docs/*" />
		public static StatusBarHiddenMode PrefersStatusBarHidden(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetPrefersStatusBarHidden(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Page.xml" path="//Member[@MemberName='SetPrefersStatusBarHidden'][2]/Docs/*" />
		public static IPlatformElementConfiguration<iOS, FormsElement> SetPrefersStatusBarHidden(this IPlatformElementConfiguration<iOS, FormsElement> config, StatusBarHiddenMode value)
		{
			SetPrefersStatusBarHidden(config.Element, value);
			return config;
		}

		/// <summary>Bindable property for <see cref="PreferredStatusBarUpdateAnimation"/>.</summary>
		public static readonly BindableProperty PreferredStatusBarUpdateAnimationProperty =
			BindableProperty.Create("PreferredStatusBarUpdateAnimation", typeof(UIStatusBarAnimation), typeof(Page), UIStatusBarAnimation.None);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Page.xml" path="//Member[@MemberName='GetPreferredStatusBarUpdateAnimation']/Docs/*" />
		public static UIStatusBarAnimation GetPreferredStatusBarUpdateAnimation(BindableObject element)
		{
			return (UIStatusBarAnimation)element.GetValue(PreferredStatusBarUpdateAnimationProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Page.xml" path="//Member[@MemberName='SetPreferredStatusBarUpdateAnimation'][1]/Docs/*" />
		public static void SetPreferredStatusBarUpdateAnimation(BindableObject element, UIStatusBarAnimation value)
		{
			if (value == UIStatusBarAnimation.Fade)
				element.SetValue(PreferredStatusBarUpdateAnimationProperty, value);
			else if (value == UIStatusBarAnimation.Slide)
				element.SetValue(PreferredStatusBarUpdateAnimationProperty, value);
			else
				element.SetValue(PreferredStatusBarUpdateAnimationProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Page.xml" path="//Member[@MemberName='PreferredStatusBarUpdateAnimation']/Docs/*" />
		public static UIStatusBarAnimation PreferredStatusBarUpdateAnimation(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetPreferredStatusBarUpdateAnimation(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Page.xml" path="//Member[@MemberName='SetPreferredStatusBarUpdateAnimation'][2]/Docs/*" />
		public static IPlatformElementConfiguration<iOS, FormsElement> SetPreferredStatusBarUpdateAnimation(this IPlatformElementConfiguration<iOS, FormsElement> config, UIStatusBarAnimation value)
		{
			SetPreferredStatusBarUpdateAnimation(config.Element, value);
			return config;
		}

		/// <summary>
		/// This iOS platform-specific controls whether padding values are overridden with the safe area insets.
		/// </summary>
#if MACCATALYST
		public static readonly BindableProperty UseSafeAreaProperty = BindableProperty.Create("UseSafeArea", typeof(bool), typeof(Page), true);
#else
		public static readonly BindableProperty UseSafeAreaProperty = BindableProperty.Create("UseSafeArea", typeof(bool), typeof(Page), false);
#endif

		/// <summary>
		/// Gets a value that indicates whether padding values are overridden with values that conform to the safe area on the device.
		/// </summary>
		/// <param name="element">The element to get the safe area behavior from.</param>
		/// <returns>true if the padding values are overridden; otherwise, false.</returns>
		public static bool GetUseSafeArea(BindableObject element)
		{
			return (bool)element.GetValue(UseSafeAreaProperty);
		}

		/// <summary>
		/// Sets a value that controls whether padding values are overridden with the safe area insets.
		/// </summary>
		/// <param name="element">The element whose safe area use behavior to set.</param>
		/// <param name="value">true to use the safe area inset behavior; otherwise, false.</param>
		public static void SetUseSafeArea(BindableObject element, bool value)
		{
			element.SetValue(UseSafeAreaProperty, value);
		}

		/// <summary>
		/// Sets a value that controls whether padding values are overridden with the safe area insets.
		/// </summary>
		/// <param name="config">The element whose safe area behavior to get.</param>
		/// <param name="value">true to use the safe area inset behavior; otherwise, false.</param>
		/// <returns></returns>
		public static IPlatformElementConfiguration<iOS, FormsElement> SetUseSafeArea(this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetUseSafeArea(config.Element, value);
			return config;
		}

		/// <summary>
		/// Returns a Boolean value that tells whether the padding is overridden with the safe area.
		/// </summary>
		/// <param name="config">The element whose safe area behavior to get.</param>
		/// <returns>true if the padding is overridden with the safe area; otherwise, false.</returns>
		public static bool UsingSafeArea(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetUseSafeArea(config.Element);
		}

		/// <summary>Bindable property for <see cref="LargeTitleDisplay"/>.</summary>
		public static readonly BindableProperty LargeTitleDisplayProperty = BindableProperty.Create(nameof(LargeTitleDisplay), typeof(LargeTitleDisplayMode), typeof(Page), LargeTitleDisplayMode.Automatic);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Page.xml" path="//Member[@MemberName='GetLargeTitleDisplay']/Docs/*" />
		public static LargeTitleDisplayMode GetLargeTitleDisplay(BindableObject element)
		{
			return (LargeTitleDisplayMode)element.GetValue(LargeTitleDisplayProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Page.xml" path="//Member[@MemberName='SetLargeTitleDisplay'][1]/Docs/*" />
		public static void SetLargeTitleDisplay(BindableObject element, LargeTitleDisplayMode value)
		{
			element.SetValue(LargeTitleDisplayProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Page.xml" path="//Member[@MemberName='LargeTitleDisplay']/Docs/*" />
		public static LargeTitleDisplayMode LargeTitleDisplay(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetLargeTitleDisplay(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Page.xml" path="//Member[@MemberName='SetLargeTitleDisplay'][2]/Docs/*" />
		public static IPlatformElementConfiguration<iOS, FormsElement> SetLargeTitleDisplay(this IPlatformElementConfiguration<iOS, FormsElement> config, LargeTitleDisplayMode value)
		{
			SetLargeTitleDisplay(config.Element, value);
			return config;
		}

		static readonly BindablePropertyKey SafeAreaInsetsPropertyKey = BindableProperty.CreateReadOnly(nameof(SafeAreaInsets), typeof(Thickness), typeof(Page), default(Thickness));

		/// <summary>Bindable property for <see cref="SafeAreaInsets"/>.</summary>
		public static readonly BindableProperty SafeAreaInsetsProperty = SafeAreaInsetsPropertyKey.BindableProperty;

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Page.xml" path="//Member[@MemberName='GetSafeAreaInsets']/Docs/*" />
		public static Thickness GetSafeAreaInsets(BindableObject element)
		{
			return (Thickness)element.GetValue(SafeAreaInsetsProperty);
		}

		static void SetSafeAreaInsets(BindableObject element, Thickness value)
		{
			element.SetValue(SafeAreaInsetsPropertyKey, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Page.xml" path="//Member[@MemberName='SafeAreaInsets']/Docs/*" />
		public static Thickness SafeAreaInsets(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetSafeAreaInsets(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Page.xml" path="//Member[@MemberName='SetSafeAreaInsets']/Docs/*" />
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
		/// Gets the modal presentation style of the <see cref="Page"/>.
		/// </summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <returns>The modal presentation style.</returns>
		public static UIModalPresentationStyle ModalPresentationStyle(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetModalPresentationStyle(config.Element);
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
		/// Gets the current value of the <see cref="UIModalPresentationStyle"/> enumeration that's applied to the <see cref="Page"/>.
		/// </summary>
		/// <param name="element">A page, the VisualElement that occupies the entire screen.</param>
		/// <returns>The current value of the UIModalPresentationStyle enumeration that's applied to the Page.</returns>
		public static UIModalPresentationStyle GetModalPresentationStyle(BindableObject element)
		{
			return (UIModalPresentationStyle)element.GetValue(ModalPresentationStyleProperty);
		}

		/// <summary>
		/// Sets the modal presentation style on a Page.
		/// </summary>
		/// <param name="element">A page, the VisualElement that occupies the entire screen.</param>
		/// <param name="value">The modal presentation style.</param>
		static void SetModalPresentationStyle(BindableObject element, UIModalPresentationStyle value)
		{
			element.SetValue(ModalPresentationStyleProperty, value);
		}

		/// <summary>Bindable property for <see cref="PrefersHomeIndicatorAutoHidden"/>.</summary>
		public static readonly BindableProperty PrefersHomeIndicatorAutoHiddenProperty =
			BindableProperty.Create(nameof(PrefersHomeIndicatorAutoHidden), typeof(bool), typeof(Page), false);

		/// <summary>
		/// Gets a value that indicates whether the visual indicator should hide upon returning to the home screen.
		/// </summary>
		/// <param name="element">A page, the VisualElement that occupies the entire screen.</param>
		/// <returns>true if the home visual indicator is hidden; otherwise, false.</returns>
		public static bool GetPrefersHomeIndicatorAutoHidden(BindableObject element)
		{
			return (bool)element.GetValue(PrefersHomeIndicatorAutoHiddenProperty);
		}

		/// <summary>
		/// Sets a value that indicates whether the visual indicator should hide upon returning to the home screen.
		/// </summary>
		/// <param name="element">A page, the VisualElement that occupies the entire screen.</param>
		/// <param name="value">true if hide the home indicator; otherwise, false.</param>
		public static void SetPrefersHomeIndicatorAutoHidden(BindableObject element, bool value)
		{
			element.SetValue(PrefersHomeIndicatorAutoHiddenProperty, value);
		}

		/// <summary>
		/// Gets a Boolean that indicates whether is allowed to hide the visual indicator for returning to the Home Screen.
		/// </summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <returns>true if hide the home indicator; otherwise, false.</returns>
		public static bool PrefersHomeIndicatorAutoHidden(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetPrefersHomeIndicatorAutoHidden(config.Element);
		}

		/// <summary>
		/// Sets a Boolean that indicates whether is allowed to hide the visual indicator for returning to the Home Screen.
		/// </summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <param name="value">true if hide the home indicator; otherwise, false.</param>
		/// <returns></returns>
		public static IPlatformElementConfiguration<iOS, FormsElement> SetPrefersHomeIndicatorAutoHidden(this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetPrefersHomeIndicatorAutoHidden(config.Element, value);
			return config;
		}
	}
}
