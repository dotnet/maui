using SkiaSharp;

namespace Microsoft.Maui.Graphics.Skia
{
	/// <summary>
	/// Provides extension methods for converting between .NET MAUI Graphics colors and SkiaSharp colors.
	/// </summary>
	public static class SKColorExtensions
	{
		/// <summary>
		/// Converts a .NET MAUI Graphics color to a SkiaSharp color.
		/// </summary>
		/// <param name="target">The color to convert.</param>
		/// <param name="alpha">An optional alpha multiplier to apply to the resulting color (0-1).</param>
		/// <returns>A SkiaSharp color that corresponds to the specified color.</returns>
		public static SKColor ToColor(this Color target, float alpha = 1)
		{
			var r = (byte)(target.Red * 255f);
			var g = (byte)(target.Green * 255f);
			var b = (byte)(target.Blue * 255f);
			var a = (byte)(target.Alpha * 255f * alpha);

			return new SKColor(r, g, b, a);
		}
	}
}
