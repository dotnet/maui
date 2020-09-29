namespace Xamarin.Forms.PlatformConfiguration.WindowsSpecific
{
	using FormsElement = Forms.VisualElement;

	public static class VisualElement
	{
		public static readonly BindableProperty AccessKeyProperty =
			BindableProperty.Create("AccessKey", typeof(string), typeof(VisualElement));

		public static readonly BindableProperty AccessKeyPlacementProperty =
					BindableProperty.Create(nameof(AccessKeyPlacement), typeof(AccessKeyPlacement), typeof(VisualElement), AccessKeyPlacement.Auto);

		public static readonly BindableProperty AccessKeyHorizontalOffsetProperty =
					BindableProperty.Create("AccessKeyHorizontalOffset", typeof(double), typeof(FormsElement), 0.0);

		public static readonly BindableProperty AccessKeyVerticalOffsetProperty =
					BindableProperty.Create("AccessKeyVerticalOffset", typeof(double), typeof(FormsElement), 0.0);

		public static string GetAccessKey(BindableObject element)
		{
			return (string)element.GetValue(AccessKeyProperty);
		}

		public static void SetAccessKey(BindableObject element, string value)
		{
			element.SetValue(AccessKeyProperty, value);
		}

		public static string GetAccessKey(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return (string)config.Element.GetValue(AccessKeyProperty);
		}

		public static IPlatformElementConfiguration<Windows, FormsElement> SetAccessKey(
			this IPlatformElementConfiguration<Windows, FormsElement> config, string value)
		{
			config.Element.SetValue(AccessKeyProperty, value);
			return config;
		}

		public static AccessKeyPlacement GetAccessKeyPlacement(BindableObject element)
		{
			return (AccessKeyPlacement)element.GetValue(AccessKeyPlacementProperty);
		}
		public static void SetAccessKeyPlacement(BindableObject element, AccessKeyPlacement value)
		{
			element.SetValue(AccessKeyPlacementProperty, value);
		}
		public static AccessKeyPlacement GetAccessKeyPlacement(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return (AccessKeyPlacement)config.Element.GetValue(AccessKeyPlacementProperty);
		}

		public static IPlatformElementConfiguration<Windows, FormsElement> SetAccessKeyPlacement(
			this IPlatformElementConfiguration<Windows, FormsElement> config, AccessKeyPlacement value)
		{
			config.Element.SetValue(AccessKeyPlacementProperty, value);
			return config;
		}
		public static double GetAccessKeyHorizontalOffset(BindableObject element)
		{
			return (double)element.GetValue(AccessKeyHorizontalOffsetProperty);
		}
		public static void SetAccessKeyHorizontalOffset(BindableObject element, double value)
		{
			element.SetValue(AccessKeyHorizontalOffsetProperty, value);
		}
		public static double GetAccessKeyHorizontalOffset(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return (double)config.Element.GetValue(AccessKeyHorizontalOffsetProperty);
		}
		public static IPlatformElementConfiguration<Windows, FormsElement> SetAccessKeyHorizontalOffset(
			this IPlatformElementConfiguration<Windows, FormsElement> config, double value)
		{
			config.Element.SetValue(AccessKeyHorizontalOffsetProperty, value);
			return config;
		}
		public static double GetAccessKeyVerticalOffset(BindableObject element)
		{
			return (double)element.GetValue(AccessKeyVerticalOffsetProperty);
		}
		public static void SetAccessKeyVerticalOffset(BindableObject element, double value)
		{
			element.SetValue(AccessKeyVerticalOffsetProperty, value);
		}
		public static double GetAccessKeyVerticalOffset(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return (double)config.Element.GetValue(AccessKeyVerticalOffsetProperty);
		}
		public static IPlatformElementConfiguration<Windows, FormsElement> SetAccessKeyVerticalOffset(
			this IPlatformElementConfiguration<Windows, FormsElement> config, double value)
		{
			config.Element.SetValue(AccessKeyVerticalOffsetProperty, value);
			return config;
		}
		#region IsLegacyColorModeEnabled

		public static readonly BindableProperty IsLegacyColorModeEnabledProperty =
			BindableProperty.CreateAttached("IsLegacyColorModeEnabled", typeof(bool),
				typeof(FormsElement), true);

		public static bool GetIsLegacyColorModeEnabled(BindableObject element)
		{
			return (bool)element.GetValue(IsLegacyColorModeEnabledProperty);
		}

		public static void SetIsLegacyColorModeEnabled(BindableObject element, bool value)
		{
			element.SetValue(IsLegacyColorModeEnabledProperty, value);
		}

		public static bool GetIsLegacyColorModeEnabled(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return (bool)config.Element.GetValue(IsLegacyColorModeEnabledProperty);
		}

		public static IPlatformElementConfiguration<Windows, FormsElement> SetIsLegacyColorModeEnabled(
			this IPlatformElementConfiguration<Windows, FormsElement> config, bool value)
		{
			config.Element.SetValue(IsLegacyColorModeEnabledProperty, value);
			return config;
		}

		#endregion
	}
}