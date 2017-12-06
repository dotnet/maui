namespace Xamarin.Forms.PlatformConfiguration.TizenSpecific
{
	using FormsElement = Forms.Image;

	public static class Image
	{
		public static readonly BindableProperty BlendColorProperty = BindableProperty.Create("BlendColor", typeof(Color), typeof(FormsElement), Color.Default);

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
	}
}
