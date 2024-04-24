namespace Microsoft.Maui.AppiumTests
{
	internal class Matrix : object
	{
		public double M00, M01, M02, M03;
		public double M10, M11, M12, M13;
		public double M20, M21, M22, M23;
		public double M30, M31, M32, M33;

		public override bool Equals(object? obj)
		{
			if (obj == null || obj is not Matrix transform)
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
}