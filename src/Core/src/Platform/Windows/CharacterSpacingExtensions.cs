using System;

namespace Microsoft.Maui.Platform
{
	public static class CharacterSpacingExtensions
	{
		public static int ToEm(this double pt)
		{
			return Convert.ToInt32(pt * 0.0624f * 1000); // Coefficient for converting Pt to Em. The value is uniform spacing between characters, in units of 1/1000 of an em.
		}
	}
}