#nullable disable

namespace Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific
{
	using Microsoft.Maui.Graphics;
	using FormsElement = Maui.Controls.Switch;

	/// <summary>Provides Tizen-specific platform configuration for switch controls.</summary>
	public static class Switch
	{
		/// <summary>Bindable property for <see cref="Color"/>.</summary>
		public static readonly BindableProperty ColorProperty = BindableProperty.Create(nameof(Color), typeof(Color), typeof(FormsElement), null);

		/// <summary>Gets the color for the switch.</summary>
		/// <param name="element">The switch element whose color to get.</param>
		/// <returns>The switch color.</returns>
		public static Color GetColor(BindableObject element)
		{
			return (Color)element.GetValue(ColorProperty);
		}

		/// <summary>Sets the color for the switch.</summary>
		/// <param name="element">The switch element whose color to set.</param>
		/// <param name="color">The switch color.</param>
		public static void SetColor(BindableObject element, Color color)
		{
			element.SetValue(ColorProperty, color);
		}

		/// <summary>Gets the color for the switch.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns>The switch color.</returns>
		public static Color GetColor(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetColor(config.Element);
		}

		/// <summary>Sets the color for the switch.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="color">The switch color.</param>
		/// <returns>The updated platform configuration.</returns>
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetColor(this IPlatformElementConfiguration<Tizen, FormsElement> config, Color color)
		{
			SetColor(config.Element, color);
			return config;
		}
	}
}
