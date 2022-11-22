namespace Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific
{
	using FormsElement = Maui.Controls.RefreshView;

	/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/RefreshView.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.RefreshView']/Docs/*" />
	public static class RefreshView
	{
		public enum RefreshPullDirection
		{
			LeftToRight,
			TopToBottom,
			RightToLeft,
			BottomToTop
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/RefreshView.xml" path="//Member[@MemberName='RefreshPullDirectionProperty']/Docs/*" />
		public static readonly BindableProperty RefreshPullDirectionProperty = BindableProperty.Create("RefreshPullDirection", typeof(RefreshPullDirection), typeof(FormsElement), RefreshPullDirection.TopToBottom);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/RefreshView.xml" path="//Member[@MemberName='SetRefreshPullDirection'][1]/Docs/*" />
		public static void SetRefreshPullDirection(BindableObject element, RefreshPullDirection value)
		{
			element.SetValue(RefreshPullDirectionProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/RefreshView.xml" path="//Member[@MemberName='GetRefreshPullDirection'][2]/Docs/*" />
		public static RefreshPullDirection GetRefreshPullDirection(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return GetRefreshPullDirection(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/RefreshView.xml" path="//Member[@MemberName='GetRefreshPullDirection'][1]/Docs/*" />
		public static RefreshPullDirection GetRefreshPullDirection(BindableObject element)
		{
			return (RefreshPullDirection)element.GetValue(RefreshPullDirectionProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/RefreshView.xml" path="//Member[@MemberName='SetRefreshPullDirection'][2]/Docs/*" />
		public static IPlatformElementConfiguration<Windows, FormsElement> SetRefreshPullDirection(
			this IPlatformElementConfiguration<Windows, FormsElement> config, RefreshPullDirection value)
		{
			SetRefreshPullDirection(config.Element, value);
			return config;
		}
	}

}
