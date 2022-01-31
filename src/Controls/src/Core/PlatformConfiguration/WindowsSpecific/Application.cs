namespace Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific
{
	using Microsoft.Extensions.DependencyInjection;
	using FormsElement = Maui.Controls.Application;

	/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/Application.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.Application']/Docs" />
	public static class Application
	{
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/Application.xml" path="//Member[@MemberName='ImageDirectoryProperty']/Docs" />
		public static readonly BindableProperty ImageDirectoryProperty =
			BindableProperty.Create("ImageDirectory", typeof(string), typeof(FormsElement), string.Empty,
				propertyChanged: OnImageDirectoryChanged);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/Application.xml" path="//Member[@MemberName='SetImageDirectory'][0]/Docs" />
		public static void SetImageDirectory(BindableObject element, string value)
		{
			element.SetValue(ImageDirectoryProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/Application.xml" path="//Member[@MemberName='GetImageDirectory']/Docs" />
		public static string GetImageDirectory(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return (string)config.Element.GetValue(ImageDirectoryProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/Application.xml" path="//Member[@MemberName='GetImageDirectory'][0]/Docs" />
		public static string GetImageDirectory(BindableObject element)
		{
			return (string)element.GetValue(ImageDirectoryProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/Application.xml" path="//Member[@MemberName='SetImageDirectory']/Docs" />
		public static IPlatformElementConfiguration<Windows, FormsElement> SetImageDirectory(
			this IPlatformElementConfiguration<Windows, FormsElement> config, string value)
		{
			config.Element.SetValue(ImageDirectoryProperty, value);
			return config;
		}

		static void OnImageDirectoryChanged(BindableObject bindable, object oldValue, object newValue)
		{
#if WINDOWS
			var config = MauiWinUIApplication.Current.Services.GetService<IImageSourceServiceConfiguration>();
			config?.SetImageDirectory(newValue?.ToString());
#endif
		}
	}
}