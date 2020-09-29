namespace Xamarin.Forms.PlatformConfiguration.TizenSpecific
{
	using FormsElement = Forms.Page;

	public static class Page
	{
		#region BreadCrumbName
		public static readonly BindableProperty BreadCrumbProperty
			= BindableProperty.CreateAttached("BreadCrumb", typeof(string), typeof(FormsElement), default(string));

		public static string GetBreadCrumb(BindableObject page)
		{
			return (string)page.GetValue(BreadCrumbProperty);
		}

		public static void SetBreadCrumb(BindableObject page, string value)
		{
			page.SetValue(BreadCrumbProperty, value);
		}

		public static string GetBreadCrumb(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetBreadCrumb(config.Element);
		}

		public static IPlatformElementConfiguration<Tizen, FormsElement> SetBreadCrumb(this IPlatformElementConfiguration<Tizen, FormsElement> config, string value)
		{
			SetBreadCrumb(config.Element, value);
			return config;
		}
		#endregion
	}
}