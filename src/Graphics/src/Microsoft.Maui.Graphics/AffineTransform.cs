using System;
using System.Diagnostics;
using System.Numerics;

namespace Microsoft.Maui.Graphics
{
	[DebuggerDisplay("X={Translation.X} Y={Translation.Y} W={Scaling.Width} H={Scaling.Height} R={RotationDegrees}° ")]
	public class AffineTransform
	{
		private const float Epsilon = 1E-10f;

		private float _m11;
		private float _m21;
		private float _m31;
		private float _m12;
		private float _m22;
		private float _m32;

		public AffineTransform()
		{
			_m11 = _m22 = 1.0f;
			_m12 = _m21 = _m31 = _m32 = 0.0f;
		}

		public AffineTransform(AffineTransform t)
		{
			_m11 = t._m11;
			_m12 = t._m12;
			_m21 = t._m21;
			_m22 = t._m22;
			_m31 = t._m31;
			_m32 = t._m32;
		}

		public AffineTransform(float m11, float m12, float m21, float m22, float m31, float m32)
		{
			_m11 = m11;
			_m12 = m12;
			_m21 = m21;
			_m22 = m22;
			_m31 = m31;
			_m32 = m32;
		}

		public AffineTransform(float[] matrix)
		{
			_m11 = matrix[0];
			_m12 = matrix[1];
			_m21 = matrix[2];
			_m22 = matrix[3];
			if (matrix.Length > 4)
			{
				_m31 = matrix[4];
				_m32 = matrix[5];
			}
		}

		public AffineTransform(in Matrix3x2 matrix)
		{
			_m11 = matrix.M11;
			_m12 = matrix.M12;
			_m21 = matrix.M21;
			_m22 = matrix.M22;
			_m31 = matrix.M31;
			_m32 = matrix.M32;
		}

		public float M11 => _m11;

		public float M22 => _m22;

		public float M21 => _m21;

		public float M12 => _m12;

		public float M31 => _m31;

		public float M32 => _m32;

		public PointF Translation
		{
			get => new PointF(_m31, _m32);
			set { _m31 = value.X; _m32 = value.Y; }
		}

		public float RotationDegrees
		{
			get => Geometry.RadiansToDegrees(Rotation);
			set => Rotation = Geometry.DegreesToRadians(value);
		}

		public float Rotation
		{
			get => (float)Math.Atan2(M12, M11);
			set
			{
				var t = Translation;
				var s = Scale;
				SetTo(t, value, s);
			}
		}

		public float AverageScale
		{
			get
			{
				var s = Scale;
				return (Math.Abs(s.Width) + Math.Abs(s.Height)) / 2;
			}
		}

		public SizeF Scale
		{
			get
			{
				var sx = _m12 == 0 ? Math.Abs(_m11) : new Vector2(_m11, _m12).Length();
				var sy = _m21 == 0 ? Math.Abs(_m22) : new Vector2(_m21, _m22).Length();
				if (GetDeterminant() < 0) sy = -sy;
				return new SizeF(sx, sy);
			}
			set
			{
				var t = Translation;
				var r = Rotation;
				SetTo(t, r, value);
			}
		}

		public bool IsIdentity => _m11 == 1.0f && _m22 == 1.0f && _m12 == 0.0f && _m21 == 0.0f && _m31 == 0.0f && _m32 == 0.0f;

		public void CopyTo(float[] matrix, int offset = 0, int count = 6)
		{
			count = Math.Min(matrix.Length, count);

			matrix[offset + 0] = _m11;
			matrix[offset + 1] = _m12;
			matrix[offset + 2] = _m21;
			matrix[offset + 3] = _m22;
			if (count > 4)
			{
				matrix[offset + 4] = _m31;
				matrix[offset + 5] = _m32;
			}
		}

		public float GetDeterminant()
		{
			return _m11 * _m22 - _m21 * _m12;
		}
		
		public void SetTransform(float m11, float m12, float m21, float m22, float m31, float m32)
		{
			_m11 = m11;
			_m12 = m12;
			_m21 = m21;
			_m22 = m22;
			_m31 = m31;
			_m32 = m32;
		}

		public void SetTransform(float[] matrix)
		{
			_m11 = matrix[0];
			_m12 = matrix[1];
			_m21 = matrix[2];
			_m22 = matrix[3];
			if (matrix.Length > 4)
			{
				_m31 = matrix[4];
				_m32 = matrix[5];
			}
		}

		public void SetTransform(in Matrix3x2 matrix)
		{
			_m11 = matrix.M11;
			_m12 = matrix.M12;
			_m21 = matrix.M21;
			_m22 = matrix.M22;
			_m31 = matrix.M31;
			_m32 = matrix.M32;
		}

		public void SetTransform(AffineTransform t)
		{
			SetTransform(t._m11, t._m12, t._m21, t._m22, t._m31, t._m32);
		}

		public void SetToIdentity()
		{
			_m11 = _m22 = 1.0f;
			_m12 = _m21 = _m31 = _m32 = 0.0f;
		}

		public void SetTo(PointF translation, float rotation, SizeF scale)
		{
			this.SetToTranslation(translation.X, translation.Y);
			this.ConcatenateRotation(rotation);
			this.ConcatenateScale(scale);
		}

		public void SetToTranslation(float mx, float my)
		{
			_m11 = _m22 = 1.0f;
			_m21 = _m12 = 0.0f;
			_m31 = mx;
			_m32 = my;
		}

		public void SetToScale(float scx, float scy)
		{
			_m11 = scx;
			_m22 = scy;
			_m12 = _m21 = _m31 = _m32 = 0.0f;
		}

		public void SetToShear(float shx, float shy)
		{
			_m11 = _m22 = 1.0f;
			_m31 = _m32 = 0.0f;
			_m21 = shx;
			_m12 = shy;
		}

		public void SetToRotation(float radians)
		{
			float sin = (float) Math.Sin(radians);
			float cos = (float) Math.Cos(radians);
			if (Math.Abs(cos) < Epsilon)
			{
				cos = 0.0f;
				sin = sin > 0.0f ? 1.0f : -1.0f;
			}
			else if (Math.Abs(sin) < Epsilon)
			{
				sin = 0.0f;
				cos = cos > 0.0f ? 1.0f : -1.0f;
			}

			_m11 = _m22 = cos;
			_m21 = -sin;
			_m12 = sin;
			_m31 = _m32 = 0.0f;
		}

		public void SetToRotation(float radians, float px, float py)
		{
			SetToRotation(radians);
			_m31 = px * (1.0f - _m11) + py * _m12;
			_m32 = py * (1.0f - _m11) - px * _m12;
		}

		public static AffineTransform GetInstance(PointF translation, float rotation, SizeF scale)
		{
			var t = new AffineTransform();
			t.SetTo(translation, rotation, scale);
			return t;
		}

		public static AffineTransform GetTranslateInstance(float mx, float my)
		{
			var t = new AffineTransform();
			t.SetToTranslation(mx, my);
			return t;
		}

		public static AffineTransform GetScaleInstance(float scx, float scY)
		{
			var t = new AffineTransform();
			t.SetToScale(scx, scY);
			return t;
		}

		public static AffineTransform GetShearInstance(float shx, float shy)
		{
			var m = new AffineTransform();
			m.SetToShear(shx, shy);
			return m;
		}

		public static AffineTransform GetRotateInstance(float angle)
		{
			var t = new AffineTransform();
			t.SetToRotation(angle);
			return t;
		}

		public static AffineTransform GetRotateInstance(float angle, float x, float y)
		{
			var t = new AffineTransform();
			t.SetToRotation(angle, x, y);
			return t;
		}

		public void ConcatenateTranslation(PointF point)
		{
			Concatenate(GetTranslateInstance(point.X, point.Y));
		}

		public void ConcatenateTranslation(float mx, float my)
		{
			Concatenate(GetTranslateInstance(mx, my));
		}

		public void ConcatenateScale(SizeF scale)
		{
			Concatenate(GetScaleInstance(scale.Width, scale.Height));
		}

		public void ConcatenateScale(float scx, float scy)
		{
			Concatenate(GetScaleInstance(scx, scy));
		}

		public void ConcatenateShear(float shx, float shy)
		{
			Concatenate(GetShearInstance(shx, shy));
		}

		public void ConcatenateRotationInDegrees(float degrees)
		{
			ConcatenateRotation(Geometry.DegreesToRadians(degrees));
		}

		public void ConcatenateRotationInDegrees(float degrees, float px, float py)
		{
			ConcatenateRotation(Geometry.DegreesToRadians(degrees), px, py);
		}

		public void ConcatenateRotation(float radians)
		{
			Concatenate(GetRotateInstance(radians));
		}

		public void ConcatenateRotation(float radians, float px, float py)
		{
			Concatenate(GetRotateInstance(radians, px, py));
		}

		public void Concatenate(AffineTransform t)
		{
			SetTransform(Multiply(t, this));
		}

		public void PreConcatenate(AffineTransform t)
		{
			SetTransform(Multiply(this, t));
		}

		/// <summary>
		/// Multiply two AffineTransform objects
		/// </summary>
		/// <param name="t1">the multiplicand</param>
		/// <param name="t2">the multiplier</param>
		/// <returns>an AffineTransform object that is a result of t1 multiplied by t2</returns>
		private AffineTransform Multiply(AffineTransform t1, AffineTransform t2)
		{
			return new AffineTransform(
				t1._m11 * t2._m11 + t1._m12 * t2._m21, // m11
				t1._m11 * t2._m12 + t1._m12 * t2._m22, // m21
				t1._m21 * t2._m11 + t1._m22 * t2._m21, // m12
				t1._m21 * t2._m12 + t1._m22 * t2._m22, // m22
				t1._m31 * t2._m11 + t1._m32 * t2._m21 + t2._m31, // m31
				t1._m31 * t2._m12 + t1._m32 * t2._m22 + t2._m32); // m32
		}

		public AffineTransform CreateInverse()
		{
			float det = GetDeterminant();
			if (Math.Abs(det) < Epsilon)
				throw new Exception("Determinant is zero");

			return new AffineTransform(
				_m22 / det,
				-_m12 / det,
				-_m21 / det,
				_m11 / det,
				(_m21 * _m32 - _m22 * _m31) / det,
				(_m12 * _m31 - _m11 * _m32) / det
			);
		}

		public PointF Transform(PointF src)
		{
			return Transform(src.X, src.Y);
		}

		public PointF Transform(float x, float y)
		{
			return new PointF(x * _m11 + y * _m21 + _m31, x * _m12 + y * _m22 + _m32);
		}

		public PointF InverseTransform(PointF src)
		{
			float det = GetDeterminant();
			if (Math.Abs(det) < Epsilon)
				throw new Exception("Unable to inverse this transform.");

			float x = src.X - _m31;
			float y = src.Y - _m32;

			return new PointF((x * _m22 - y * _m21) / det, (y * _m11 - x * _m12) / det);
		}

		public void Transform(float[] src, int srcOff, float[] dst, int dstOff, int length)
		{
			int step = 2;
			if (src == dst && srcOff < dstOff && dstOff < srcOff + length * 2)
			{
				srcOff = srcOff + length * 2 - 2;
				dstOff = dstOff + length * 2 - 2;
				step = -2;
			}

			while (--length >= 0)
			{
				float x = src[srcOff + 0];
				float y = src[srcOff + 1];
				dst[dstOff + 0] = x * _m11 + y * _m21 + _m31;
				dst[dstOff + 1] = x * _m12 + y * _m22 + _m32;
				srcOff += step;
				dstOff += step;
			}
		}

		public bool IsUnityTransform()
		{
			return !(HasScale() || HasRotate() || HasTranslate());
		}

		private bool HasScale()
		{
			// ReSharper disable CompareOfFloatsByEqualityOperator

			// if matrix has no rotation, we can do an exact check:

			if (!HasRotate())
			{
				return _m11 != 1.0 || _m22 != 1.0;
			}

			// when the transform is rotated, we have to deconstruct
			// the scaling and handle the check with precission loss:

			var scale = this.Scale;

			if (Math.Abs(scale.Width - 1) > Epsilon) return true;
			if (Math.Abs(scale.Height - 1) > Epsilon) return true;

			return false;
			
			// ReSharper restore CompareOfFloatsByEqualityOperator
		}

		private bool HasRotate()
		{
			// ReSharper disable CompareOfFloatsByEqualityOperator
			return _m12 != 0.0 || _m21 != 0.0;
			// ReSharper restore CompareOfFloatsByEqualityOperator
		}

		private bool HasTranslate()
		{
			// ReSharper disable CompareOfFloatsByEqualityOperator
			return _m31 != 0.0 || _m32 != 0.0;
			// ReSharper restore CompareOfFloatsByEqualityOperator
		}

		public bool OnlyTranslate()
		{
			return !HasRotate() && !HasScale();
		}

		public bool OnlyTranslateOrScale()
		{
			return !HasRotate();
		}

		public bool OnlyScale()
		{
			return !HasRotate() && !HasTranslate();
		}

		public void Deconstruct(out PointF translation, out float rotation, out SizeF scale)
		{
			scale = this.Scale;
			rotation = this.Rotation;
			translation = Translation;
		}

		public static implicit operator AffineTransform(Matrix3x2 matrix) => new AffineTransform(matrix);

		public static explicit operator Matrix3x2(AffineTransform matrix)
		{
			return new Matrix3x2(matrix._m11, matrix._m12, matrix._m21, matrix._m22, matrix._m31, matrix._m32);
		}

		/// <summary>
		/// Creates a matrix from an SRT.
		/// </summary>
		/// <param name="scale">The scale</param>
		/// <param name="rotation">The rotation, in radians</param>
		/// <param name="translation">the translation</param>
		/// <returns>A Matrix3x2</returns>
		/// <remarks>
		/// This is equivalent to:<br/>
		/// <c>
		/// m = Matrix3x2.CreateScale(scale)<br/>
		/// m *= Matri3x2.CreateRotation(rotation)<br/>
		/// m *= Matri3x2.CreateTranslation(translation)<br/>
		/// </c>		
		/// </remarks>
		public static Matrix3x2 CreateMatrix3x2(Vector2 scale, float rotation, Vector2 translation)
		{
			return Matrix3x2Extensions.CreateMatrix3x2(scale, rotation, translation);
		}


	}
}
