namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Specifies the blend mode to use when compositing images or drawings.
	/// </summary>
	public enum BlendMode
	{
		/// <summary>
		/// Normal blending mode which simply draws the source over the destination.
		/// </summary>
		Normal,

		/// <summary>
		/// Multiplies the colors of the source with the destination, resulting in a darker image.
		/// </summary>
		Multiply,

		/// <summary>
		/// Multiplies the complements of the source and destination colors, then complements the result, resulting in a lighter image.
		/// </summary>
		Screen,

		/// <summary>
		/// Multiplies or screens the colors depending on the destination color, preserving highlights and shadows.
		/// </summary>
		Overlay,

		/// <summary>
		/// Selects the darker of the source and destination colors for each pixel.
		/// </summary>
		Darken,

		/// <summary>
		/// Selects the lighter of the source and destination colors for each pixel.
		/// </summary>
		Lighten,

		/// <summary>
		/// Brightens the destination color to reflect the source color, resulting in a lighter image.
		/// </summary>
		ColorDodge,

		/// <summary>
		/// Darkens the destination color to reflect the source color, resulting in a darker image.
		/// </summary>
		ColorBurn,

		/// <summary>
		/// Darkens or lightens the colors depending on the source color, creating a subtle effect.
		/// </summary>
		SoftLight,

		/// <summary>
		/// Similar to overlay but with source and destination reversed, creating a more dramatic effect.
		/// </summary>
		HardLight,

		/// <summary>
		/// Subtracts the darker of the two colors from the lighter color, creating an inverted effect.
		/// </summary>
		Difference,

		/// <summary>
		/// Similar to difference but with lower contrast, resulting in a more muted effect.
		/// </summary>
		Exclusion,

		/// <summary>
		/// Preserves the hue of the source color while using the saturation and luminosity of the destination.
		/// </summary>
		Hue,

		/// <summary>
		/// Preserves the saturation of the source color while using the hue and luminosity of the destination.
		/// </summary>
		Saturation,

		/// <summary>
		/// Preserves the hue and saturation of the source color while using the luminosity of the destination.
		/// </summary>
		Color,

		/// <summary>
		/// Preserves the luminosity of the source color while using the hue and saturation of the destination.
		/// </summary>
		Luminosity,

		/// <summary>
		/// Clears the destination area, making it fully transparent.
		/// </summary>
		Clear,

		/// <summary>
		/// Copies the source over the destination, ignoring the destination.
		/// </summary>
		Copy,

		/// <summary>
		/// Shows the source where both the source and destination overlap, otherwise displays transparency.
		/// </summary>
		SourceIn,

		/// <summary>
		/// Shows the source where it doesn't overlap with the destination, otherwise displays transparency.
		/// </summary>
		SourceOut,

		/// <summary>
		/// Shows the source where it overlaps with non-transparent parts of the destination.
		/// </summary>
		SourceAtop,

		/// <summary>
		/// Shows the destination over the source.
		/// </summary>
		DestinationOver,

		/// <summary>
		/// Shows the destination where both the source and destination overlap, otherwise displays transparency.
		/// </summary>
		DestinationIn,

		/// <summary>
		/// Shows the destination where it doesn't overlap with the source, otherwise displays transparency.
		/// </summary>
		DestinationOut,

		/// <summary>
		/// Shows the destination where it overlaps with non-transparent parts of the source.
		/// </summary>
		DestinationAtop,

		/// <summary>
		/// Shows the source where it doesn't overlap with the destination and shows the destination where it doesn't overlap with the source.
		/// </summary>
		Xor,

		/// <summary>
		/// Adds the source and destination colors, favoring darker results.
		/// </summary>
		PlusDarker,

		/// <summary>
		/// Adds the source and destination colors, favoring lighter results.
		/// </summary>
		PlusLighter
	}
}
