#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific
{
	using FormsElement = Maui.Controls.Slider;

	/// <summary>Platform-specific functionality for sliders the iOS platform.</summary>
	public static class Slider
	{
		/// <summary>Bindable property for attached property <c>UpdateOnTap</c>.</summary>
		public static readonly BindableProperty UpdateOnTapProperty = BindableProperty.Create("UpdateOnTap", typeof(bool), typeof(Slider), false);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Slider.xml" path="//Member[@MemberName='GetUpdateOnTap'][1]/Docs/*" />
		public static bool GetUpdateOnTap(BindableObject element)
		{
			return (bool)element.GetValue(UpdateOnTapProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Slider.xml" path="//Member[@MemberName='SetUpdateOnTap'][1]/Docs/*" />
		public static void SetUpdateOnTap(BindableObject element, bool value)
		{
			element.SetValue(UpdateOnTapProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Slider.xml" path="//Member[@MemberName='GetUpdateOnTap'][2]/Docs/*" />
		public static bool GetUpdateOnTap(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetUpdateOnTap(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Slider.xml" path="//Member[@MemberName='SetUpdateOnTap'][2]/Docs/*" />
		public static IPlatformElementConfiguration<iOS, FormsElement> SetUpdateOnTap(this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetUpdateOnTap(config.Element, value);
			return config;
		}
	}
}
