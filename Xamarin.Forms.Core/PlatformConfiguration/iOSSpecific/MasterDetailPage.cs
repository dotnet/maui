
namespace Xamarin.Forms.PlatformConfiguration.iOSSpecific
{
	using FormsElement = Forms.MasterDetailPage;

	public static class MasterDetailPage
	{
		#region ApplyShadow
		public static readonly BindableProperty ApplyShadowProperty = BindableProperty.Create("ApplyShadow", typeof(bool), typeof(MasterDetailPage), false);

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
