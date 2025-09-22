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

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/VisualElement.xml" path="//Member[@MemberName='GetAccessKey'][1]/Docs/*" />
		public static string GetAccessKey(BindableObject element)
		{
			return (string)element.GetValue(AccessKeyProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/VisualElement.xml" path="//Member[@MemberName='SetAccessKey'][1]/Docs/*" />
		public static void SetAccessKey(BindableObject element, string value)
		{
			element.SetValue(AccessKeyProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/VisualElement.xml" path="//Member[@MemberName='GetAccessKey'][2]/Docs/*" />
		public static string GetAccessKey(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return (string)config.Element.GetValue(AccessKeyProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/VisualElement.xml" path="//Member[@MemberName='SetAccessKey'][2]/Docs/*" />
		public static IPlatformElementConfiguration<Windows, FormsElement> SetAccessKey(
			this IPlatformElementConfiguration<Windows, FormsElement> config, string value)
		{
			config.Element.SetValue(AccessKeyProperty, value);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/VisualElement.xml" path="//Member[@MemberName='GetAccessKeyPlacement'][1]/Docs/*" />
		public static AccessKeyPlacement GetAccessKeyPlacement(BindableObject element)
		{
			return (AccessKeyPlacement)element.GetValue(AccessKeyPlacementProperty);
		}
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/VisualElement.xml" path="//Member[@MemberName='SetAccessKeyPlacement'][1]/Docs/*" />
		public static void SetAccessKeyPlacement(BindableObject element, AccessKeyPlacement value)
		{
			element.SetValue(AccessKeyPlacementProperty, value);
		}
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/VisualElement.xml" path="//Member[@MemberName='GetAccessKeyPlacement'][2]/Docs/*" />
		public static AccessKeyPlacement GetAccessKeyPlacement(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return (AccessKeyPlacement)config.Element.GetValue(AccessKeyPlacementProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/VisualElement.xml" path="//Member[@MemberName='SetAccessKeyPlacement'][2]/Docs/*" />
		public static IPlatformElementConfiguration<Windows, FormsElement> SetAccessKeyPlacement(
			this IPlatformElementConfiguration<Windows, FormsElement> config, AccessKeyPlacement value)
		{
			config.Element.SetValue(AccessKeyPlacementProperty, value);
			return config;
		}
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/VisualElement.xml" path="//Member[@MemberName='GetAccessKeyHorizontalOffset'][1]/Docs/*" />
		public static double GetAccessKeyHorizontalOffset(BindableObject element)
		{
			return (double)element.GetValue(AccessKeyHorizontalOffsetProperty);
		}
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/VisualElement.xml" path="//Member[@MemberName='SetAccessKeyHorizontalOffset'][1]/Docs/*" />
		public static void SetAccessKeyHorizontalOffset(BindableObject element, double value)
		{
			element.SetValue(AccessKeyHorizontalOffsetProperty, value);
		}
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/VisualElement.xml" path="//Member[@MemberName='GetAccessKeyHorizontalOffset'][2]/Docs/*" />
		public static double GetAccessKeyHorizontalOffset(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return (double)config.Element.GetValue(AccessKeyHorizontalOffsetProperty);
		}
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/VisualElement.xml" path="//Member[@MemberName='SetAccessKeyHorizontalOffset'][2]/Docs/*" />
		public static IPlatformElementConfiguration<Windows, FormsElement> SetAccessKeyHorizontalOffset(
			this IPlatformElementConfiguration<Windows, FormsElement> config, double value)
		{
			config.Element.SetValue(AccessKeyHorizontalOffsetProperty, value);
			return config;
		}
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/VisualElement.xml" path="//Member[@MemberName='GetAccessKeyVerticalOffset'][1]/Docs/*" />
		public static double GetAccessKeyVerticalOffset(BindableObject element)
		{
			return (double)element.GetValue(AccessKeyVerticalOffsetProperty);
		}
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/VisualElement.xml" path="//Member[@MemberName='SetAccessKeyVerticalOffset'][1]/Docs/*" />
		public static void SetAccessKeyVerticalOffset(BindableObject element, double value)
		{
			element.SetValue(AccessKeyVerticalOffsetProperty, value);
		}
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/VisualElement.xml" path="//Member[@MemberName='GetAccessKeyVerticalOffset'][2]/Docs/*" />
		public static double GetAccessKeyVerticalOffset(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return (double)config.Element.GetValue(AccessKeyVerticalOffsetProperty);
		}
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/VisualElement.xml" path="//Member[@MemberName='SetAccessKeyVerticalOffset'][2]/Docs/*" />
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

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/VisualElement.xml" path="//Member[@MemberName='GetIsLegacyColorModeEnabled'][1]/Docs/*" />
		public static bool GetIsLegacyColorModeEnabled(BindableObject element)
		{
			return (bool)element.GetValue(IsLegacyColorModeEnabledProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/VisualElement.xml" path="//Member[@MemberName='SetIsLegacyColorModeEnabled'][1]/Docs/*" />
		public static void SetIsLegacyColorModeEnabled(BindableObject element, bool value)
		{
			element.SetValue(IsLegacyColorModeEnabledProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/VisualElement.xml" path="//Member[@MemberName='GetIsLegacyColorModeEnabled'][2]/Docs/*" />
		public static bool GetIsLegacyColorModeEnabled(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return (bool)config.Element.GetValue(IsLegacyColorModeEnabledProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/VisualElement.xml" path="//Member[@MemberName='SetIsLegacyColorModeEnabled'][2]/Docs/*" />
		public static IPlatformElementConfiguration<Windows, FormsElement> SetIsLegacyColorModeEnabled(
			this IPlatformElementConfiguration<Windows, FormsElement> config, bool value)
		{
			config.Element.SetValue(IsLegacyColorModeEnabledProperty, value);
			return config;
		}

		#endregion
	}
}
