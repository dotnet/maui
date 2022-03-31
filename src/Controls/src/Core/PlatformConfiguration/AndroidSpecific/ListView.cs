namespace Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific
{
	using FormsElement = Maui.Controls.ListView;

	/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/ListView.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.ListView']/Docs" />
	public static class ListView
	{
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/ListView.xml" path="//Member[@MemberName='IsFastScrollEnabledProperty']/Docs" />
		public static readonly BindableProperty IsFastScrollEnabledProperty = BindableProperty.Create("IsFastScrollEnabled", typeof(bool), typeof(ListView), false);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/ListView.xml" path="//Member[@MemberName='GetIsFastScrollEnabled']/Docs" />
		public static bool GetIsFastScrollEnabled(BindableObject element)
		{
			return (bool)element.GetValue(IsFastScrollEnabledProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/ListView.xml" path="//Member[@MemberName='SetIsFastScrollEnabled'][1]/Docs" />
		public static void SetIsFastScrollEnabled(BindableObject element, bool value)
		{
			element.SetValue(IsFastScrollEnabledProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/ListView.xml" path="//Member[@MemberName='IsFastScrollEnabled']/Docs" />
		public static bool IsFastScrollEnabled(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetIsFastScrollEnabled(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/ListView.xml" path="//Member[@MemberName='SetIsFastScrollEnabled'][2]/Docs" />
		public static IPlatformElementConfiguration<Android, FormsElement> SetIsFastScrollEnabled(this IPlatformElementConfiguration<Android, FormsElement> config, bool value)
		{
			SetIsFastScrollEnabled(config.Element, value);
			return config;
		}
	}
}
