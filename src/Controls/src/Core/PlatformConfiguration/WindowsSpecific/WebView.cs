
namespace Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific
{
	using FormsElement = Maui.Controls.WebView;

	/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/WebView.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.WebView']/Docs" />
	public static class WebView
	{
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/WebView.xml" path="//Member[@MemberName='IsJavaScriptAlertEnabledProperty']/Docs" />
		public static readonly BindableProperty IsJavaScriptAlertEnabledProperty = BindableProperty.Create("IsJavaScriptAlertEnabled", typeof(bool), typeof(WebView), false);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/WebView.xml" path="//Member[@MemberName='GetIsJavaScriptAlertEnabled']/Docs" />
		public static bool GetIsJavaScriptAlertEnabled(BindableObject element)
		{
			return (bool)element.GetValue(IsJavaScriptAlertEnabledProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/WebView.xml" path="//Member[@MemberName='SetIsJavaScriptAlertEnabled'][1]/Docs" />
		public static void SetIsJavaScriptAlertEnabled(BindableObject element, bool value)
		{
			element.SetValue(IsJavaScriptAlertEnabledProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/WebView.xml" path="//Member[@MemberName='IsJavaScriptAlertEnabled']/Docs" />
		public static bool IsJavaScriptAlertEnabled(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return GetIsJavaScriptAlertEnabled(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/WebView.xml" path="//Member[@MemberName='SetIsJavaScriptAlertEnabled'][2]/Docs" />
		public static IPlatformElementConfiguration<Windows, FormsElement> SetIsJavaScriptAlertEnabled(this IPlatformElementConfiguration<Windows, FormsElement> config, bool value)
		{
			SetIsJavaScriptAlertEnabled(config.Element, value);
			return config;
		}



		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/WebView.xml" path="//Member[@MemberName='ExecutionModeProperty']/Docs" />
		public static readonly BindableProperty ExecutionModeProperty = BindableProperty.Create("ExecutionMode", typeof(WebViewExecutionMode), typeof(WebView), WebViewExecutionMode.SameThread);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/WebView.xml" path="//Member[@MemberName='GetExecutionMode'][1]/Docs" />
		public static WebViewExecutionMode GetExecutionMode(BindableObject element)
		{
			return (WebViewExecutionMode)element.GetValue(ExecutionModeProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/WebView.xml" path="//Member[@MemberName='SetExecutionMode'][1]/Docs" />
		public static void SetExecutionMode(BindableObject element, WebViewExecutionMode value)
		{
			element.SetValue(ExecutionModeProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/WebView.xml" path="//Member[@MemberName='GetExecutionMode'][2]/Docs" />
		public static WebViewExecutionMode GetExecutionMode(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return GetExecutionMode(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/WebView.xml" path="//Member[@MemberName='SetExecutionMode'][2]/Docs" />
		public static IPlatformElementConfiguration<Windows, FormsElement> SetExecutionMode(this IPlatformElementConfiguration<Windows, FormsElement> config, WebViewExecutionMode value)
		{
			SetExecutionMode(config.Element, value);
			return config;
		}
	}
}
