using SharpDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Graphics.SharpDX
{
    internal static class FontExtensions
    {
		public static FontStyle ToFontStyle(this FontStyleType styleType)
			=> styleType switch
			{
				FontStyleType.Normal =>	FontStyle.Normal,
				FontStyleType.Italic => FontStyle.Italic,
				FontStyleType.Oblique => FontStyle.Oblique,
				_ => FontStyle.Normal,
			};

		public static FontWeight ToFontWeight(this int weight)
		{
			if (weight < 150)
				return FontWeight.Thin;
			if (weight < 250)
				return FontWeight.ExtraLight;
			if (weight < 325)
				return FontWeight.Light;
			if (weight < 375)
				return FontWeight.SemiLight;
			if (weight < 450)
				return FontWeight.Normal;
			if (weight < 550)
				return FontWeight.Medium;
			if (weight < 650)
				return FontWeight.SemiBold;
			if (weight < 750)
				return FontWeight.Bold;
			if (weight < 850)
				return FontWeight.ExtraBold;
			if (weight < 925)
				return FontWeight.Heavy;
			if (weight >= 950)
				return FontWeight.UltraBlack;

			return FontWeight.Normal;
		}
	}
}
