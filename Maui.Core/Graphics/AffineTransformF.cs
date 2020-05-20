using System;
// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable MemberCanBePrivate.Global

namespace System.Maui.Graphics {
	public class AffineTransformF
	{
		private const double Zero = 1E-10f;

		private double _m00;
		private double _m01;
		private double _m02;
		private double _m10;
		private double _m11;
		private double _m12;

		public AffineTransformF()
		{
			_m00 = _m11 = 1.0f;
			_m10 = _m01 = _m02 = _m12 = 0.0f;
		}

		public AffineTransformF(AffineTransformF t)
		{
			_m00 = t._m00;
			_m10 = t._m10;
			_m01 = t._m01;
			_m11 = t._m11;
			_m02 = t._m02;
			_m12 = t._m12;
		}

		public AffineTransformF(double m00, double m10, double m01, double m11, double m02, double m12)
		{
			_m00 = m00;
			_m10 = m10;
			_m01 = m01;
			_m11 = m11;
			_m02 = m02;
			_m12 = m12;
		}

		public AffineTransformF(double[] matrix)
		{
			_m00 = matrix[0];
			_m10 = matrix[1];
			_m01 = matrix[2];
			_m11 = matrix[3];
			if (matrix.Length > 4)
			{
				_m02 = matrix[4];
				_m12 = matrix[5];
			}
		}

		public void SetTransform(double m00, double m10, double m01, double m11, double m02, double m12)
		{
			_m00 = m00;
			_m10 = m10;
			_m01 = m01;
			_m11 = m11;
			_m02 = m02;
			_m12 = m12;
		}

		public void SetTransform(AffineTransformF t)
		{
			SetTransform(t._m00, t._m10, t._m01, t._m11, t._m02, t._m12);
		}

		public void SetToIdentity()
		{
			_m00 = _m11 = 1.0f;
			_m10 = _m01 = _m02 = _m12 = 0.0f;
		}

		public void SetToTranslation(double mx, double my)
		{
			_m00 = _m11 = 1.0f;
			_m01 = _m10 = 0.0f;
			_m02 = mx;
			_m12 = my;
		}

		public void SetToScale(double scx, double scy)
		{
			_m00 = scx;
			_m11 = scy;
			_m10 = _m01 = _m02 = _m12 = 0.0f;
		}

		public void SetToShear(double shx, double shy)
		{
			_m00 = _m11 = 1.0f;
			_m02 = _m12 = 0.0f;
			_m01 = shx;
			_m10 = shy;
		}

		public void SetToRotation(double angle)
		{
			double sin = (double)Math.Sin(angle);
			double cos = (double)Math.Cos(angle);
			if (Math.Abs(cos) < Zero)
			{
				cos = 0.0f;
				sin = sin > 0.0f ? 1.0f : -1.0f;
			}
			else if (Math.Abs(sin) < Zero)
			{
				sin = 0.0f;
				cos = cos > 0.0f ? 1.0f : -1.0f;
			}

			_m00 = _m11 = cos;
			_m01 = -sin;
			_m10 = sin;
			_m02 = _m12 = 0.0f;
		}

		public void SetToRotation(double angle, double px, double py)
		{
			SetToRotation(angle);
			_m02 = px * (1.0f - _m00) + py * _m10;
			_m12 = py * (1.0f - _m00) - px * _m10;
		}

		public static AffineTransformF GetTranslateInstance(double mx, double my)
		{
			var t = new AffineTransformF();
			t.SetToTranslation(mx, my);
			return t;
		}

		public static AffineTransformF GetScaleInstance(double scx, double scY)
		{
			var t = new AffineTransformF();
			t.SetToScale(scx, scY);
			return t;
		}

		public static AffineTransformF GetShearInstance(double shx, double shy)
		{
			var m = new AffineTransformF();
			m.SetToShear(shx, shy);
			return m;
		}

		public static AffineTransformF GetRotateInstance(double angle)
		{
			var t = new AffineTransformF();
			t.SetToRotation(angle);
			return t;
		}

		public static AffineTransformF GetRotateInstance(double angle, double x, double y)
		{
			var t = new AffineTransformF();
			t.SetToRotation(angle, x, y);
			return t;
		}

		public void Translate(double mx, double my)
		{
			Concatenate(GetTranslateInstance(mx, my));
		}

		public void Scale(double scx, double scy)
		{
			Concatenate(GetScaleInstance(scx, scy));
		}

		public void Shear(double shx, double shy)
		{
			Concatenate(GetShearInstance(shx, shy));
		}

		public void RotateInDegrees(double angleInDegrees)
		{
			Rotate(GraphicsOperations.DegreesToRadians(angleInDegrees));
		}

		public void RotateInDegrees(double angleInDegrees, double px, double py)
		{
			Rotate(GraphicsOperations.DegreesToRadians(angleInDegrees), px, py);
		}

		public void Rotate(double angleInRadians)
		{
			Concatenate(GetRotateInstance(angleInRadians));
		}

		public void Rotate(double angleInRadians, double px, double py)
		{
			Concatenate(GetRotateInstance(angleInRadians, px, py));
		}

		private AffineTransformF Multiply(AffineTransformF t1, AffineTransformF t2)
		{
			return new AffineTransformF(
				t1._m00 * t2._m00 + t1._m10 * t2._m01, // m00
				t1._m00 * t2._m10 + t1._m10 * t2._m11, // m01
				t1._m01 * t2._m00 + t1._m11 * t2._m01, // m10
				t1._m01 * t2._m10 + t1._m11 * t2._m11, // m11
				t1._m02 * t2._m00 + t1._m12 * t2._m01 + t2._m02, // m02
				t1._m02 * t2._m10 + t1._m12 * t2._m11 + t2._m12); // m12
		}

		public void Concatenate(AffineTransformF t)
		{
			SetTransform(Multiply(t, this));
		}

		public Point Transform(Point src)
		{
			return Transform(src.X, src.Y);
		}

		public Point Transform(double x, double y)
		{
			return new Point(x * _m00 + y * _m01 + _m02, x * _m10 + y * _m11 + _m12);
		}

		public bool IsIdentity => _m00 == 1.0f && _m11 == 1.0f && _m10 == 0.0f && _m01 == 0.0f && _m02 == 0.0f && _m12 == 0.0f;
	}
}
