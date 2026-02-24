#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific
{
	using FormsElement = Maui.Controls.VisualElement;

	/// <summary>Provides access to platform-specific features of visual elements on the Windows platform.</summary>
	public static class VisualElement
	{
		/// <summary>Bindable property for attached property <c>AccessKey</c>.</summary>
		public static readonly BindableProperty AccessKeyProperty =
			BindableProperty.Create("AccessKey", typeof(string), typeof(VisualElement));

		/// <summary>Bindable property for <see cref="AccessKeyPlacement"/>.</summary>
		public static readonly BindableProperty AccessKeyPlacementProperty =
					BindableProperty.Create(nameof(AccessKeyPlacement), typeof(AccessKeyPlacement), typeof(VisualElement), AccessKeyPlacement.Auto);

		/// <summary>Bindable property for attached property <c>AccessKeyHorizontalOffset</c>.</summary>
		public static readonly BindableProperty AccessKeyHorizontalOffsetProperty =
					BindableProperty.Create("AccessKeyHorizontalOffset", typeof(double), typeof(FormsElement), 0.0);

		/// <summary>Bindable property for attached property <c>AccessKeyVerticalOffset</c>.</summary>
		public static readonly BindableProperty AccessKeyVerticalOffsetProperty =
					BindableProperty.Create("AccessKeyVerticalOffset", typeof(double), typeof(FormsElement), 0.0);

		/// <summary>Gets the keyboard access key for the element on Windows.</summary>
		/// <param name="element">The element to query.</param>
		/// <returns>The access key string.</returns>
		public static string GetAccessKey(BindableObject element)
		{
			return (string)element.GetValue(AccessKeyProperty);
		}

		/// <summary>Sets the keyboard access key for the element on Windows.</summary>
		/// <param name="element">The element to configure.</param>
		/// <param name="value">The access key string.</param>
		public static void SetAccessKey(BindableObject element, string value)
		{
			element.SetValue(AccessKeyProperty, value);
		}

		/// <summary>Gets the keyboard access key for the element on Windows.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns>The access key string.</returns>
		public static string GetAccessKey(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return (string)config.Element.GetValue(AccessKeyProperty);
		}

		/// <summary>Sets the keyboard access key for the element on Windows.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="value">The access key string.</param>
		/// <returns>The updated platform configuration.</returns>
		public static IPlatformElementConfiguration<Windows, FormsElement> SetAccessKey(
			this IPlatformElementConfiguration<Windows, FormsElement> config, string value)
		{
			config.Element.SetValue(AccessKeyProperty, value);
			return config;
		}

		/// <summary>Gets the placement of the access key tooltip on Windows.</summary>
		/// <param name="element">The element to query.</param>
		/// <returns>The access key placement.</returns>
		public static AccessKeyPlacement GetAccessKeyPlacement(BindableObject element)
		{
			return (AccessKeyPlacement)element.GetValue(AccessKeyPlacementProperty);
		}
		/// <summary>Sets the placement of the access key tooltip on Windows.</summary>
		/// <param name="element">The element to configure.</param>
		/// <param name="value">The access key placement.</param>
		public static void SetAccessKeyPlacement(BindableObject element, AccessKeyPlacement value)
		{
			element.SetValue(AccessKeyPlacementProperty, value);
		}
		/// <summary>Gets the placement of the access key tooltip on Windows.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns>The access key placement.</returns>
		public static AccessKeyPlacement GetAccessKeyPlacement(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return (AccessKeyPlacement)config.Element.GetValue(AccessKeyPlacementProperty);
		}

		/// <summary>Sets the placement of the access key tooltip on Windows.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="value">The access key placement.</param>
		/// <returns>The updated platform configuration.</returns>
		public static IPlatformElementConfiguration<Windows, FormsElement> SetAccessKeyPlacement(
			this IPlatformElementConfiguration<Windows, FormsElement> config, AccessKeyPlacement value)
		{
			config.Element.SetValue(AccessKeyPlacementProperty, value);
			return config;
		}
		/// <summary>Gets the horizontal offset of the access key tooltip on Windows.</summary>
		/// <param name="element">The element to query.</param>
		/// <returns>The horizontal offset in device-independent units.</returns>
		public static double GetAccessKeyHorizontalOffset(BindableObject element)
		{
			return (double)element.GetValue(AccessKeyHorizontalOffsetProperty);
		}
		/// <summary>Sets the horizontal offset of the access key tooltip on Windows.</summary>
		/// <param name="element">The element to configure.</param>
		/// <param name="value">The horizontal offset in device-independent units.</param>
		public static void SetAccessKeyHorizontalOffset(BindableObject element, double value)
		{
			element.SetValue(AccessKeyHorizontalOffsetProperty, value);
		}
		/// <summary>Gets the horizontal offset of the access key tooltip on Windows.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns>The horizontal offset in device-independent units.</returns>
		public static double GetAccessKeyHorizontalOffset(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return (double)config.Element.GetValue(AccessKeyHorizontalOffsetProperty);
		}
		/// <summary>Sets the horizontal offset of the access key tooltip on Windows.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="value">The horizontal offset in device-independent units.</param>
		/// <returns>The updated platform configuration.</returns>
		public static IPlatformElementConfiguration<Windows, FormsElement> SetAccessKeyHorizontalOffset(
			this IPlatformElementConfiguration<Windows, FormsElement> config, double value)
		{
			config.Element.SetValue(AccessKeyHorizontalOffsetProperty, value);
			return config;
		}
		/// <summary>Gets the vertical offset of the access key tooltip on Windows.</summary>
		/// <param name="element">The element to query.</param>
		/// <returns>The vertical offset in device-independent units.</returns>
		public static double GetAccessKeyVerticalOffset(BindableObject element)
		{
			return (double)element.GetValue(AccessKeyVerticalOffsetProperty);
		}
		/// <summary>Sets the vertical offset of the access key tooltip on Windows.</summary>
		/// <param name="element">The element to configure.</param>
		/// <param name="value">The vertical offset in device-independent units.</param>
		public static void SetAccessKeyVerticalOffset(BindableObject element, double value)
		{
			element.SetValue(AccessKeyVerticalOffsetProperty, value);
		}
		/// <summary>Gets the vertical offset of the access key tooltip on Windows.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns>The vertical offset in device-independent units.</returns>
		public static double GetAccessKeyVerticalOffset(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return (double)config.Element.GetValue(AccessKeyVerticalOffsetProperty);
		}
		/// <summary>Sets the vertical offset of the access key tooltip on Windows.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="value">The vertical offset in device-independent units.</param>
		/// <returns>The updated platform configuration.</returns>
		public static IPlatformElementConfiguration<Windows, FormsElement> SetAccessKeyVerticalOffset(
			this IPlatformElementConfiguration<Windows, FormsElement> config, double value)
		{
			config.Element.SetValue(AccessKeyVerticalOffsetProperty, value);
			return config;
		}
		#region IsLegacyColorModeEnabled

		/// <summary>Bindable property for attached property <c>IsLegacyColorModeEnabled</c>.</summary>
		public static readonly BindableProperty IsLegacyColorModeEnabledProperty =
			BindableProperty.CreateAttached("IsLegacyColorModeEnabled", typeof(bool),
				typeof(FormsElement), true);

		/// <summary>Gets whether legacy color mode is enabled on Windows.</summary>
		/// <param name="element">The element to query.</param>
		/// <returns><see langword="true"/> if legacy color mode is enabled.</returns>
		public static bool GetIsLegacyColorModeEnabled(BindableObject element)
		{
			return (bool)element.GetValue(IsLegacyColorModeEnabledProperty);
		}

		/// <summary>Sets whether legacy color mode is enabled on Windows.</summary>
		/// <param name="element">The element to configure.</param>
		/// <param name="value"><see langword="true"/> to enable legacy color mode.</param>
		public static void SetIsLegacyColorModeEnabled(BindableObject element, bool value)
		{
			element.SetValue(IsLegacyColorModeEnabledProperty, value);
		}

		/// <summary>Gets whether legacy color mode is enabled on Windows.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns><see langword="true"/> if legacy color mode is enabled.</returns>
		public static bool GetIsLegacyColorModeEnabled(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return (bool)config.Element.GetValue(IsLegacyColorModeEnabledProperty);
		}

		/// <summary>Sets whether legacy color mode is enabled on Windows.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="value"><see langword="true"/> to enable legacy color mode.</param>
		/// <returns>The updated platform configuration.</returns>
		public static IPlatformElementConfiguration<Windows, FormsElement> SetIsLegacyColorModeEnabled(
			this IPlatformElementConfiguration<Windows, FormsElement> config, bool value)
		{
			config.Element.SetValue(IsLegacyColorModeEnabledProperty, value);
			return config;
		}

		#endregion
	}
}
