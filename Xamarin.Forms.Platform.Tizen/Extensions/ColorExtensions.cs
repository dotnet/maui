using EColor = ElmSharp.Color;

namespace Xamarin.Forms.Platform.Tizen
{
	public static class ColorExtensions
	{
		/// <summary>
		/// Creates an instance of ElmSharp.Color class based on provided Xamarin.Forms.Color instance
		/// </summary>
		/// <returns>ElmSharp.Color instance representing a color which corresponds to the provided Xamarin.Forms.Color</returns>
		/// <param name="c">The Xamarin.Forms.Color instance which will be converted to a ElmSharp.Color</param>
		public static EColor ToNative(this Color c)
		{
			if (c.IsDefault)
			{
				// Trying to convert the default color, this may result in black color.
				return EColor.Default;
			}
			else
			{
				return new EColor((int)(255.0 * c.R), (int)(255.0 * c.G), (int)(255.0 * c.B), (int)(255.0 * c.A));
			}
		}

		/// <summary>
		/// Returns a string representing the provided ElmSharp.Color instance in a hexagonal notation
		/// </summary>
		/// <returns>string value containing the encoded color</returns>
		/// <param name="c">The ElmSharp.Color class instance which will be serialized</param>
		internal static string ToHex(this EColor c)
		{
			if (c.IsDefault)
			{
				Log.Warn("Trying to convert the default color to hexagonal notation, it does not works as expected.");
			}
			return string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", c.R, c.G, c.B, c.A);
		}
	}
}
