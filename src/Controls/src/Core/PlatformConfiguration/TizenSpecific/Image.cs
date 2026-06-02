#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific
{
	using Microsoft.Maui.Graphics;
	using FormsElement = Maui.Controls.Image;

	/// <summary>Provides access to the blend color for images on the Tizen platform.</summary>
	public static class Image
	{
		/// <summary>Bindable property for attached property <c>BlendColor</c>.</summary>
		public static readonly BindableProperty BlendColorProperty = BindableProperty.Create("BlendColor", typeof(Color), typeof(FormsElement), null);

		/// <summary>Bindable property for attached property <c>File</c>.</summary>
		public static readonly BindableProperty FileProperty = BindableProperty.Create("File", typeof(string), typeof(FormsElement), default(string));

		/// <summary>Gets the blend color for the image.</summary>
		/// <param name="element">The image element whose blend color to get.</param>
		/// <returns>The blend color.</returns>
		public static Color GetBlendColor(BindableObject element)
		{
			return (Color)element.GetValue(BlendColorProperty);
		}

		/// <summary>Sets the blend color for the image.</summary>
		/// <param name="element">The image element whose blend color to set.</param>
		/// <param name="color">The blend color.</param>
		public static void SetBlendColor(BindableObject element, Color color)
		{
			element.SetValue(BlendColorProperty, color);
		}

		/// <summary>Gets the blend color for the image.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns>The blend color.</returns>
		public static Color GetBlendColor(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetBlendColor(config.Element);
		}

		/// <summary>Sets the blend color for the image.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="color">The blend color.</param>
		/// <returns>The updated platform configuration.</returns>
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetBlendColor(this IPlatformElementConfiguration<Tizen, FormsElement> config, Color color)
		{
			SetBlendColor(config.Element, color);
			return config;
		}

		/// <summary>Gets the file path for the image.</summary>
		/// <param name="element">The image element whose file path to get.</param>
		/// <returns>The file path.</returns>
		public static string GetFile(BindableObject element)
		{
			return (string)element.GetValue(FileProperty);
		}

		/// <summary>Sets the file path for the image.</summary>
		/// <param name="element">The image element whose file path to set.</param>
		/// <param name="file">The file path.</param>
		public static void SetFile(BindableObject element, string file)
		{
			element.SetValue(FileProperty, file);
		}

		/// <summary>Gets the file path for the image.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns>The file path.</returns>
		public static string GetFile(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetFile(config.Element);
		}

		/// <summary>Sets the file path for the image.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="file">The file path.</param>
		/// <returns>The updated platform configuration.</returns>
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetFile(this IPlatformElementConfiguration<Tizen, FormsElement> config, string file)
		{
			SetFile(config.Element, file);
			return config;
		}
	}
}
