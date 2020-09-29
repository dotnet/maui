
namespace Xamarin.Forms.PlatformConfiguration.TizenSpecific
{
	using FormsElement = Forms.Switch;

	public static class Switch
	{
		public static readonly BindableProperty ColorProperty = BindableProperty.Create(nameof(Color), typeof(Color), typeof(FormsElement), Color.Default);

		public static Color GetColor(BindableObject element)
		{
			return (Color)element.GetValue(ColorProperty);
		}

		public static void SetColor(BindableObject element, Color color)
		{
			element.SetValue(ColorProperty, color);
		}

		public static Color GetColor(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetColor(config.Element);
		}

		public static IPlatformElementConfiguration<Tizen, FormsElement> SetColor(this IPlatformElementConfiguration<Tizen, FormsElement> config, Color color)
		{
			SetColor(config.Element, color);
			return config;
		}
	}
}