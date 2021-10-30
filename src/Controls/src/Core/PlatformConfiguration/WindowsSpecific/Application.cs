namespace Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific
{
	using Microsoft.Extensions.DependencyInjection;
	using FormsElement = Maui.Controls.Application;

	public static class Application
	{
		public static readonly BindableProperty ImageDirectoryProperty =
			BindableProperty.Create("ImageDirectory", typeof(string), typeof(FormsElement), string.Empty,
				propertyChanged: OnImageDirectoryChanged);

		public static void SetImageDirectory(BindableObject element, string value)
		{
			element.SetValue(ImageDirectoryProperty, value);
		}

		public static string GetImageDirectory(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return (string)config.Element.GetValue(ImageDirectoryProperty);
		}

		public static string GetImageDirectory(BindableObject element)
		{
			return (string)element.GetValue(ImageDirectoryProperty);
		}

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