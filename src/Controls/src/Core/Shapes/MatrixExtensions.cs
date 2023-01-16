#nullable disable
using System.Numerics;

namespace Microsoft.Maui.Controls.Shapes
{
	public static class MatrixExtensions
	{
		public static Matrix ToMatrix(this Matrix3x2 matrix3x2)
		{
			return new Matrix
			{
				M11 = matrix3x2.M11,
				M12 = matrix3x2.M12,
				M21 = matrix3x2.M21,
				M22 = matrix3x2.M22,
				OffsetX = matrix3x2.M31,
				OffsetY = matrix3x2.M32
			};
		}

		public static Matrix3x2 ToMatrix3X2(this Matrix matrix)
		{
			return new Matrix3x2
			{
				M11 = (float)matrix.M11,
				M12 = (float)matrix.M12,
				M21 = (float)matrix.M21,
				M22 = (float)matrix.M22,
				M31 = (float)matrix.OffsetX,
				M32 = (float)matrix.OffsetY
			};
		}
	}
}