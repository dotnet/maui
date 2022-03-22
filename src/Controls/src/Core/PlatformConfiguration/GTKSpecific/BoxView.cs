namespace Microsoft.Maui.Controls.PlatformConfiguration.GTKSpecific
{
	using FormsElement = Maui.Controls.BoxView;

	/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.GTKSpecific/BoxView.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.GTKSpecific.BoxView']/Docs" />
	public static class BoxView
	{
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.GTKSpecific/BoxView.xml" path="//Member[@MemberName='HasCornerRadiusProperty']/Docs" />
		public static readonly BindableProperty HasCornerRadiusProperty =
			BindableProperty.Create("HasCornerRadius", typeof(bool),
				typeof(BoxView), default(bool));

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.GTKSpecific/BoxView.xml" path="//Member[@MemberName='GetHasCornerRadius'][1]/Docs" />
		public static bool GetHasCornerRadius(BindableObject element)
		{
			return (bool)element.GetValue(HasCornerRadiusProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.GTKSpecific/BoxView.xml" path="//Member[@MemberName='SetHasCornerRadius'][1]/Docs" />
		public static void SetHasCornerRadius(BindableObject element, bool tabPosition)
		{
			element.SetValue(HasCornerRadiusProperty, tabPosition);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.GTKSpecific/BoxView.xml" path="//Member[@MemberName='GetHasCornerRadius'][2]/Docs" />
		public static bool GetHasCornerRadius(
			this IPlatformElementConfiguration<GTK, FormsElement> config)
		{
			return GetHasCornerRadius(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.GTKSpecific/BoxView.xml" path="//Member[@MemberName='SetHasCornerRadius'][2]/Docs" />
		public static IPlatformElementConfiguration<GTK, FormsElement> SetHasCornerRadius(
			this IPlatformElementConfiguration<GTK, FormsElement> config, bool value)
		{
			SetHasCornerRadius(config.Element, value);

			return config;
		}
	}
}
