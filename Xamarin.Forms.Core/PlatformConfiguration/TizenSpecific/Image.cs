namespace Xamarin.Forms.PlatformConfiguration.TizenSpecific
{
	using FormsElement = Forms.Image;

	public static class Image
	{
		public static readonly BindableProperty BlendColorProperty = BindableProperty.Create("BlendColor", typeof(Color), typeof(FormsElement), Color.Default);

		public static readonly BindableProperty FileProperty = BindableProperty.Create("File", typeof(string), typeof(FormsElement), default(string));

		public static Color GetBlendColor(BindableObject element)
		{
			return (Color)element.GetValue(BlendColorProperty);
		}

		public static void SetBlendColor(BindableObject element, Color color)
		{
			element.SetValue(BlendColorProperty, color);
		}

		public static Color GetBlendColor(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetBlendColor(config.Element);
		}

		public static IPlatformElementConfiguration<Tizen, FormsElement> SetBlendColor(this IPlatformElementConfiguration<Tizen, FormsElement> config, Color color)
		{
			SetBlendColor(config.Element, color);
			return config;
		}

		public static string GetFile(BindableObject element)
		{
			return (string)element.GetValue(FileProperty);
		}

		public static void SetFile(BindableObject element, string file)
		{
			element.SetValue(FileProperty, file);
		}

		public static string GetFile(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetFile(config.Element);
		}

		public static IPlatformElementConfiguration<Tizen, FormsElement> SetFile(this IPlatformElementConfiguration<Tizen, FormsElement> config, string file)
		{
			SetFile(config.Element, file);
			return config;
		}
	}
}