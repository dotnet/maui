namespace Xamarin.Forms.PlatformConfiguration.TizenSpecific
{
	using FormsElement = Forms.NavigationPage;

	public static class NavigationPage
	{
		#region HasBreadCrumbsBar
		public static readonly BindableProperty HasBreadCrumbsBarProperty
			= BindableProperty.CreateAttached("HasBreadCrumbsBar", typeof(bool), typeof(FormsElement), false);

		public static bool GetHasBreadCrumbsBar(BindableObject element)
		{
			return (bool)element.GetValue(HasBreadCrumbsBarProperty);
		}

		public static void SetHasBreadCrumbsBar(BindableObject element, bool value)
		{
			element.SetValue(HasBreadCrumbsBarProperty, value);
		}

		public static bool HasBreadCrumbsBar(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetHasBreadCrumbsBar(config.Element);
		}

		public static IPlatformElementConfiguration<Tizen, FormsElement> SetHasBreadCrumbsBar(this IPlatformElementConfiguration<Tizen, FormsElement> config, bool value)
		{
			SetHasBreadCrumbsBar(config.Element, value);
			return config;
		}
		#endregion
	}
}