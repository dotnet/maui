#nullable disable

namespace Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific
{
	using FormsElement = Maui.Controls.WebView;

	/// <summary>Controls whether JavaScript alerts are enabled for a web view.</summary>
	public static class WebView
	{
		/// <summary>Bindable property for <see cref="IsJavaScriptAlertEnabled"/>.</summary>
		public static readonly BindableProperty IsJavaScriptAlertEnabledProperty = BindableProperty.Create("IsJavaScriptAlertEnabled", typeof(bool), typeof(WebView), false);

		/// <summary>Returns a Boolean value that tells whether the web view allows JavaScript alerts.</summary>
		/// <param name="element">The web view element whose JavaScript alert permissions to return.</param>
		/// <returns>A Boolean value that tells whether the web view allows JavaScript alerts.</returns>
		public static bool GetIsJavaScriptAlertEnabled(BindableObject element)
		{
			return (bool)element.GetValue(IsJavaScriptAlertEnabledProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/WebView.xml" path="//Member[@MemberName='SetIsJavaScriptAlertEnabled'][1]/Docs/*" />
		public static void SetIsJavaScriptAlertEnabled(BindableObject element, bool value)
		{
			element.SetValue(IsJavaScriptAlertEnabledProperty, value);
		}

		/// <summary>Returns a Boolean value that tells whether the web view allows JavaScript alerts.</summary>
		/// <param name="config">The platform configuration for the web view element whose JavaScript alert permissions to return.</param>
		/// <returns>A Boolean value that tells whether the web view allows JavaScript alerts.</returns>
		public static bool IsJavaScriptAlertEnabled(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return GetIsJavaScriptAlertEnabled(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/WebView.xml" path="//Member[@MemberName='SetIsJavaScriptAlertEnabled'][2]/Docs/*" />
		public static IPlatformElementConfiguration<Windows, FormsElement> SetIsJavaScriptAlertEnabled(this IPlatformElementConfiguration<Windows, FormsElement> config, bool value)
		{
			SetIsJavaScriptAlertEnabled(config.Element, value);
			return config;
		}

		/// <summary>Bindable property for attached property <c>ExecutionMode</c>.</summary>
		public static readonly BindableProperty ExecutionModeProperty = BindableProperty.Create("ExecutionMode", typeof(WebViewExecutionMode), typeof(WebView), WebViewExecutionMode.SameThread);

		public static WebViewExecutionMode GetExecutionMode(BindableObject element)
		{
			return (WebViewExecutionMode)element.GetValue(ExecutionModeProperty);
		}

		public static void SetExecutionMode(BindableObject element, WebViewExecutionMode value)
		{
			element.SetValue(ExecutionModeProperty, value);
		}

		public static WebViewExecutionMode GetExecutionMode(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return GetExecutionMode(config.Element);
		}

		public static IPlatformElementConfiguration<Windows, FormsElement> SetExecutionMode(this IPlatformElementConfiguration<Windows, FormsElement> config, WebViewExecutionMode value)
		{
			SetExecutionMode(config.Element, value);
			return config;
		}
	}
}
