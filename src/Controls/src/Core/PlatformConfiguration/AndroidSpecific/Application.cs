namespace Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific
{
	using FormsElement = Maui.Controls.Application;

	/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/WindowSoftInputModeAdjust.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.WindowSoftInputModeAdjust']/Docs" />
	public enum WindowSoftInputModeAdjust
	{
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/WindowSoftInputModeAdjust.xml" path="//Member[@MemberName='Pan']/Docs" />
		Pan,
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/WindowSoftInputModeAdjust.xml" path="//Member[@MemberName='Resize']/Docs" />
		Resize,
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/WindowSoftInputModeAdjust.xml" path="//Member[@MemberName='Unspecified']/Docs" />
		Unspecified
	}

	/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/Application.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.Application']/Docs" />
	public static class Application
	{
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/Application.xml" path="//Member[@MemberName='WindowSoftInputModeAdjustProperty']/Docs" />
		public static readonly BindableProperty WindowSoftInputModeAdjustProperty =
			BindableProperty.Create("WindowSoftInputModeAdjust", typeof(WindowSoftInputModeAdjust),
			typeof(Application), WindowSoftInputModeAdjust.Pan);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/Application.xml" path="//Member[@MemberName='GetWindowSoftInputModeAdjust'][1]/Docs" />
		public static WindowSoftInputModeAdjust GetWindowSoftInputModeAdjust(BindableObject element)
		{
			return (WindowSoftInputModeAdjust)element.GetValue(WindowSoftInputModeAdjustProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/Application.xml" path="//Member[@MemberName='SetWindowSoftInputModeAdjust']/Docs" />
		public static void SetWindowSoftInputModeAdjust(BindableObject element, WindowSoftInputModeAdjust value)
		{
			element.SetValue(WindowSoftInputModeAdjustProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/Application.xml" path="//Member[@MemberName='GetWindowSoftInputModeAdjust'][2]/Docs" />
		public static WindowSoftInputModeAdjust GetWindowSoftInputModeAdjust(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetWindowSoftInputModeAdjust(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/Application.xml" path="//Member[@MemberName='UseWindowSoftInputModeAdjust']/Docs" />
		public static IPlatformElementConfiguration<Android, FormsElement> UseWindowSoftInputModeAdjust(this IPlatformElementConfiguration<Android, FormsElement> config, WindowSoftInputModeAdjust value)
		{
			SetWindowSoftInputModeAdjust(config.Element, value);
			return config;
		}
	}
}
