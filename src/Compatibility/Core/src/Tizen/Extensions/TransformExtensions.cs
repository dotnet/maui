using Microsoft.Maui.Controls.Shapes;
using SkiaSharp;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	public static class TransformExtensions
	{
		public static SKMatrix ToSkia(this Transform transform)
		{
			SKMatrix skMatrix = SKMatrix.CreateIdentity();

			if (transform == null)
				return skMatrix;

			Matrix matrix = transform.Value;

			skMatrix.Values = new float[] {
				(float)matrix.M11,
				(float)matrix.M21,
				Forms.ConvertToScaledPixel(matrix.OffsetX),
				(float)matrix.M12,
				(float)matrix.M22,
				Forms.ConvertToScaledPixel(matrix.OffsetY),
				0,
				0,
				1 };

			return skMatrix;
		}
	}
}
