#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific
{
	using FormsElement = Maui.Controls.Label;

	/// <summary>Provides access to the font weight for labels on the Tizen platform.</summary>
	public static class Label
	{
		/// <summary>Bindable property for <see cref="FontWeight"/>.</summary>
		public static readonly BindableProperty FontWeightProperty = BindableProperty.Create("FontWeight", typeof(string), typeof(FormsElement), FontWeight.None);

		/// <summary>Gets the font weight for the label.</summary>
		/// <param name="element">The label element whose font weight to get.</param>
		/// <returns>The font weight value.</returns>
		public static string GetFontWeight(BindableObject element)
		{
			return (string)element.GetValue(FontWeightProperty);
		}

		/// <summary>Sets the font weight for the label.</summary>
		/// <param name="element">The label element whose font weight to set.</param>
		/// <param name="weight">The font weight value.</param>
		public static void SetFontWeight(BindableObject element, string weight)
		{
			element.SetValue(FontWeightProperty, weight);
		}

		/// <summary>Gets the font weight for the label.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns>The font weight value.</returns>
		public static string GetFontWeight(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetFontWeight(config.Element);
		}

		/// <summary>Sets the font weight for the label.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="weight">The font weight value.</param>
		/// <returns>The updated platform configuration.</returns>
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetFontWeight(this IPlatformElementConfiguration<Tizen, FormsElement> config, string weight)
		{
			SetFontWeight(config.Element, weight);
			return config;
		}
	}
}
