namespace Xamarin.Forms.PlatformConfiguration.GTKSpecific
{
	using FormsElement = Forms.BoxView;

	public static class BoxView
	{
		public static readonly BindableProperty HasCornerRadiusProperty =
			BindableProperty.Create("HasCornerRadius", typeof(bool),
				typeof(BoxView), default(bool));

		public static bool GetHasCornerRadius(BindableObject element)
		{
			return (bool)element.GetValue(HasCornerRadiusProperty);
		}

		public static void SetHasCornerRadius(BindableObject element, bool tabPosition)
		{
			element.SetValue(HasCornerRadiusProperty, tabPosition);
		}

		public static bool GetHasCornerRadius(
			this IPlatformElementConfiguration<GTK, FormsElement> config)
		{
			return GetHasCornerRadius(config.Element);
		}

		public static IPlatformElementConfiguration<GTK, FormsElement> SetHasCornerRadius(
			this IPlatformElementConfiguration<GTK, FormsElement> config, bool value)
		{
			SetHasCornerRadius(config.Element, value);

			return config;
		}
	}
}