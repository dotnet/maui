#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific
{
	using Microsoft.Extensions.DependencyInjection;
	using FormsElement = Maui.Controls.Application;

	/// <summary>Provides Windows-specific configuration for the application's image directory.</summary>
	public static class Application
	{
		/// <summary>Bindable property for attached property <c>ImageDirectory</c>.</summary>
		public static readonly BindableProperty ImageDirectoryProperty =
			BindableProperty.Create("ImageDirectory", typeof(string), typeof(FormsElement), string.Empty,
				propertyChanged: OnImageDirectoryChanged);

		/// <summary>Sets the directory path for application images on Windows.</summary>
		/// <param name="element">The element to set the image directory on.</param>
		/// <param name="value">The directory path.</param>
		public static void SetImageDirectory(BindableObject element, string value)
		{
			element.SetValue(ImageDirectoryProperty, value);
		}

		/// <summary>Gets the directory path for application images on Windows.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns>The image directory path.</returns>
		public static string GetImageDirectory(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return (string)config.Element.GetValue(ImageDirectoryProperty);
		}

		/// <summary>Gets the directory path for application images on Windows.</summary>
		/// <param name="element">The element to get the image directory from.</param>
		/// <returns>The image directory path.</returns>
		public static string GetImageDirectory(BindableObject element)
		{
			return (string)element.GetValue(ImageDirectoryProperty);
		}

		/// <summary>Sets the directory path for application images on Windows.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="value">The directory path.</param>
		/// <returns>The updated platform configuration.</returns>
		public static IPlatformElementConfiguration<Windows, FormsElement> SetImageDirectory(
			this IPlatformElementConfiguration<Windows, FormsElement> config, string value)
		{
			config.Element.SetValue(ImageDirectoryProperty, value);
			return config;
		}

		static void OnImageDirectoryChanged(BindableObject bindable, object oldValue, object newValue)
		{
			// TODO: somehow pass this onto Core
		}
	}
}
