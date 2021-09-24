
namespace Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific
{
	using FormsElement = Maui.Controls.FlyoutPage;

	public static class FlyoutPage
	{
		#region ApplyShadow
		public static readonly BindableProperty ApplyShadowProperty = BindableProperty.Create("ApplyShadow", typeof(bool), typeof(FlyoutPage), false);

		public static bool GetApplyShadow(BindableObject element)
		{
			return (bool)element.GetValue(ApplyShadowProperty);
		}

		public static void SetApplyShadow(BindableObject element, bool value)
		{
			element.SetValue(ApplyShadowProperty, value);
		}

		public static IPlatformElementConfiguration<iOS, FormsElement> SetApplyShadow(this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetApplyShadow(config.Element, value);
			return config;
		}

		public static bool GetApplyShadow(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetApplyShadow(config.Element);
		}
		#endregion
	}
}
