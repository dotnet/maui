#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific
{
	using FormsElement = Maui.Controls.ScrollView;

	/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/ScrollView.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.ScrollView']/Docs/*" />
	public static class ScrollView
	{
		/// <summary>Bindable property for <see cref="ShouldDelayContentTouches"/>.</summary>
		public static readonly BindableProperty ShouldDelayContentTouchesProperty = BindableProperty.Create(nameof(ShouldDelayContentTouches), typeof(bool), typeof(ScrollView), true);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/ScrollView.xml" path="//Member[@MemberName='GetShouldDelayContentTouches']/Docs/*" />
		public static bool GetShouldDelayContentTouches(BindableObject element)
		{
			return (bool)element.GetValue(ShouldDelayContentTouchesProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/ScrollView.xml" path="//Member[@MemberName='SetShouldDelayContentTouches'][1]/Docs/*" />
		public static void SetShouldDelayContentTouches(BindableObject element, bool value)
		{
			element.SetValue(ShouldDelayContentTouchesProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/ScrollView.xml" path="//Member[@MemberName='ShouldDelayContentTouches']/Docs/*" />
		public static bool ShouldDelayContentTouches(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetShouldDelayContentTouches(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/ScrollView.xml" path="//Member[@MemberName='SetShouldDelayContentTouches'][2]/Docs/*" />
		public static IPlatformElementConfiguration<iOS, FormsElement> SetShouldDelayContentTouches(this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetShouldDelayContentTouches(config.Element, value);
			return config;
		}
	}
}
