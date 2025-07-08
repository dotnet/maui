using System;

namespace Microsoft.Maui.Platform
{
	public static class CharacterSpacingExtensions
	{
		public static int ToEm(this double pt)
		{
			return Convert.ToInt32(pt * 0.0624f * 1000); // Coefficient for converting Pt to Em. The value is uniform spacing between characters, in units of 1/1000 of an em.
		}

		/// <summary>
		/// Converts character spacing from Em units back to the original double value.
		/// </summary>
		/// <param name="emValue">The character spacing value in Em units (1/1000 of an em)</param>
		/// <returns>The original character spacing value in points</returns>
		/// <remarks>
		/// This method performs the inverse operation of ToEm().
		/// Em units are defined as 1/1000 of an em, where the conversion coefficient is 0.0624.
		/// The calculation reverses: originalValue * 0.0624 * 1000 = emValue
		/// So: originalValue = emValue / (0.0624 * 1000)
		/// </remarks>
		public static double FromEm(this int emValue)
		{
			return emValue / (0.0624 * 1000);
		}
	}
}