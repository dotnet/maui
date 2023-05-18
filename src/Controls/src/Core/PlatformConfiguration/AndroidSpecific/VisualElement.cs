#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific
{
	using FormsElement = Maui.Controls.VisualElement;

	/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/VisualElement.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.VisualElement']/Docs/*" />
	public static class VisualElement
	{
		#region Elevation

		/// <summary>Bindable property for attached property <c>Elevation</c>.</summary>
		public static readonly BindableProperty ElevationProperty =
			BindableProperty.Create("Elevation", typeof(float?),
				typeof(FormsElement));

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/VisualElement.xml" path="//Member[@MemberName='GetElevation'][2]/Docs/*" />
		public static float? GetElevation(FormsElement element)
		{
			return (float?)element.GetValue(ElevationProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/VisualElement.xml" path="//Member[@MemberName='SetElevation'][2]/Docs/*" />
		public static void SetElevation(FormsElement element, float? value)
		{
			element.SetValue(ElevationProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/VisualElement.xml" path="//Member[@MemberName='GetElevation'][1]/Docs/*" />
		public static float? GetElevation(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetElevation(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/VisualElement.xml" path="//Member[@MemberName='SetElevation'][1]/Docs/*" />
		public static IPlatformElementConfiguration<Android, FormsElement> SetElevation(this IPlatformElementConfiguration<Android, FormsElement> config, float? value)
		{
			SetElevation(config.Element, value);
			return config;
		}

		#endregion

		#region IsLegacyColorModeEnabled

		/// <summary>Bindable property for attached property <c>IsLegacyColorModeEnabled</c>.</summary>
		public static readonly BindableProperty IsLegacyColorModeEnabledProperty =
			BindableProperty.CreateAttached("IsLegacyColorModeEnabled", typeof(bool),
				typeof(FormsElement), true);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/VisualElement.xml" path="//Member[@MemberName='GetIsLegacyColorModeEnabled'][1]/Docs/*" />
		public static bool GetIsLegacyColorModeEnabled(BindableObject element)
		{
			return (bool)element.GetValue(IsLegacyColorModeEnabledProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/VisualElement.xml" path="//Member[@MemberName='SetIsLegacyColorModeEnabled'][1]/Docs/*" />
		public static void SetIsLegacyColorModeEnabled(BindableObject element, bool value)
		{
			element.SetValue(IsLegacyColorModeEnabledProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/VisualElement.xml" path="//Member[@MemberName='GetIsLegacyColorModeEnabled'][2]/Docs/*" />
		public static bool GetIsLegacyColorModeEnabled(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return (bool)config.Element.GetValue(IsLegacyColorModeEnabledProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/VisualElement.xml" path="//Member[@MemberName='SetIsLegacyColorModeEnabled'][2]/Docs/*" />
		public static IPlatformElementConfiguration<Android, FormsElement> SetIsLegacyColorModeEnabled(
			this IPlatformElementConfiguration<Android, FormsElement> config, bool value)
		{
			config.Element.SetValue(IsLegacyColorModeEnabledProperty, value);
			return config;
		}

		#endregion
	}
}
