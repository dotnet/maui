#nullable enable
namespace Microsoft.Maui.Graphics
{
	public static partial class PaintExtensions
	{
		public static bool IsNullOrEmpty(this Paint? paint)
		{
			if (paint is SolidPaint solidPaint)
				return solidPaint == null || solidPaint.Color == null;

			if (paint is GradientPaint gradientPaint)
				return gradientPaint == null || gradientPaint.GradientStops.Length == 0;

			if (paint is ImagePaint imagePaint)
				return imagePaint == null || imagePaint.Image == null;

			if (paint is PatternPaint patternPaint)
				return patternPaint == null || patternPaint.Pattern == null;

			return paint == null;
		}
	}
}