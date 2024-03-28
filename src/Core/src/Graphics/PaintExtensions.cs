#nullable enable
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Graphics
{
	public static partial class PaintExtensions
	{
		public static Color? ToColor(this Paint? paint)
		{
			if (paint is SolidPaint solidPaint)
				return solidPaint.Color;

			if (paint is GradientPaint gradientPaint)
				return gradientPaint.GradientStops?[0]?.Color;

			if (paint is ImagePaint)
				return null;

			if (paint is PatternPaint)
				return null;

			return null;
		}

		public static bool IsNullOrEmpty([NotNullWhen(true)] this Paint? paint)
		{
			if (paint is SolidPaint solidPaint)
				return solidPaint is null || solidPaint.Color is null;

			if (paint is GradientPaint gradientPaint)
				return gradientPaint is null || gradientPaint.GradientStops.Length == 0 || gradientPaint.StartColor is null || gradientPaint.EndColor is null;

			if (paint is ImagePaint imagePaint)
				return imagePaint is null || imagePaint.Image is null;

			if (paint is PatternPaint patternPaint)
				return patternPaint is null || patternPaint.Pattern is null;

			return paint is null;
		}

		internal static bool IsTransparent(this Paint? paint)
		{
			if (paint is SolidPaint solidPaint)
				return solidPaint.Color == Colors.Transparent;

			return false;
		}
	}
}