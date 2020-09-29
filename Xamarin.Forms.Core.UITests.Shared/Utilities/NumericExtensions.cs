using System;

namespace Xamarin.Forms.Core.UITests
{
	internal class Matrix : Object
	{
		public double M00, M01, M02, M03;
		public double M10, M11, M12, M13;
		public double M20, M21, M22, M23;
		public double M30, M31, M32, M33;

		public void Log()
		{

			//Logger.LogLine ();

			//Logger.LogLine (string.Format ("{0,-3}, {1,-3}, {2,-3}, {3,-3}", M00, M01, M02, M03));
			//Logger.LogLine (string.Format ("{0,-3}, {1,-3}, {2,-3}, {3,-3}", M10, M11, M12, M13));
			//Logger.LogLine (string.Format ("{0,-3}, {1,-3}, {2,-3}, {3,-3}", M20, M21, M22, M23));
			//Logger.LogLine (string.Format ("{0,-3}, {1,-3}, {2,-3}, {3,-3}", M30, M31, M32, M33));

			//Logger.LogLine ();
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;

			var transform = obj as Matrix;
			if ((Object)transform == null)
				return false;

			const double tolerance = 0.01;
			bool result =
				Math.Abs(M00 - transform.M00) < tolerance &&
				Math.Abs(M01 - transform.M01) < tolerance &&
				Math.Abs(M02 - transform.M02) < tolerance &&
				Math.Abs(M03 - transform.M03) < tolerance &&
				Math.Abs(M10 - transform.M10) < tolerance &&
				Math.Abs(M11 - transform.M11) < tolerance &&
				Math.Abs(M12 - transform.M12) < tolerance &&
				Math.Abs(M13 - transform.M13) < tolerance &&
				Math.Abs(M20 - transform.M20) < tolerance &&
				Math.Abs(M21 - transform.M21) < tolerance &&
				Math.Abs(M22 - transform.M22) < tolerance &&
				Math.Abs(M23 - transform.M23) < tolerance &&
				Math.Abs(M30 - transform.M30) < tolerance &&
				Math.Abs(M31 - transform.M31) < tolerance &&
				Math.Abs(M32 - transform.M32) < tolerance &&
				Math.Abs(M33 - transform.M33) < tolerance;

			return result;
		}

		public override int GetHashCode()
		{
			return 0;
		}
	}

	internal enum Axis
	{
		X,
		Y,
		Z
	}

	internal static class NumericExtensions
	{
		public static double ToRadians(this float val)
		{
			return (Math.PI / 180.0) * val;
		}

		public static Matrix CalculateRotationMatrixForDegrees(float degrees, Axis rotationAxis)
		{
			var angle = degrees.ToRadians();

			var transform = new Matrix();
			if (rotationAxis == Axis.X)
			{
				transform.M00 = 1;
				transform.M01 = 0;
				transform.M02 = 0;
				transform.M03 = 0;
				transform.M10 = 0;
				transform.M11 = (float)Math.Cos(angle);
				transform.M12 = (float)Math.Sin(angle);
				transform.M13 = 0;
				transform.M20 = 0;
				transform.M21 = -(float)Math.Sin(angle);
				transform.M22 = (float)Math.Cos(angle);
				transform.M23 = 0;
				transform.M30 = 0;
				transform.M31 = 0;
				transform.M32 = 0;
				transform.M33 = 1;
			}
			else if (rotationAxis == Axis.Y)
			{
				transform.M00 = (float)Math.Cos(angle);
				transform.M01 = 0;
				transform.M02 = -(float)Math.Sin(angle);
				transform.M03 = 0;
				transform.M10 = 0;
				transform.M11 = 1;
				transform.M12 = 0;
				transform.M13 = 0;
				transform.M20 = (float)Math.Sin(angle);
				transform.M21 = 0;
				transform.M22 = (float)Math.Cos(angle);
				transform.M23 = 0;
				transform.M30 = 0;
				transform.M31 = 0;
				transform.M32 = 0;
				transform.M33 = 1;
			}
			else
			{
				transform.M00 = (float)Math.Cos(angle);
				transform.M01 = (float)Math.Sin(angle);
				transform.M02 = 0;
				transform.M03 = 0;
				transform.M10 = -(float)Math.Sin(angle);
				transform.M11 = (float)Math.Cos(angle);
				transform.M12 = 0;
				transform.M13 = 0;
				transform.M20 = 0;
				transform.M21 = 0;
				transform.M22 = 1;
				transform.M23 = 0;
				transform.M30 = 0;
				transform.M31 = 0;
				transform.M32 = 0;
				transform.M33 = 1;
			}

			return transform;
		}

		public static Matrix BuildScaleMatrix(float scale)
		{
			var transform = new Matrix();

			transform.M00 = scale;
			transform.M01 = 0;
			transform.M02 = 0;
			transform.M03 = 0;
			transform.M10 = 0;
			transform.M11 = scale;
			transform.M12 = 0;
			transform.M13 = 0;
			transform.M20 = 0;
			transform.M21 = 0;
			transform.M22 = scale;
			transform.M23 = 0;
			transform.M30 = 0;
			transform.M31 = 0;
			transform.M32 = 0;
			transform.M33 = 1;

			return transform;
		}

	}

}