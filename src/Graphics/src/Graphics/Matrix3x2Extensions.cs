using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

//[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Microsoft.Maui.Graphics.Tests")]

namespace Microsoft.Maui.Graphics
{
	static class Matrix3x2Extensions
	{
		public static bool IsZero(this in Matrix3x2 matrix)
		{
			if (matrix.M11 != 0) return false;
			if (matrix.M12 != 0) return false;
			if (matrix.M21 != 0) return false;
			if (matrix.M22 != 0) return false;
			if (matrix.M31 != 0) return false;
			if (matrix.M32 != 0) return false;
			return true;
		}

		public static bool IsFinite(this in Matrix3x2 matrix)
		{
			bool result = true;

#if NETSTANDARD2_0 || TIZEN40
			result &= !(float.IsNaN(matrix.M11) || float.IsInfinity(matrix.M11));
			result &= !(float.IsNaN(matrix.M12) || float.IsInfinity(matrix.M12));
			result &= !(float.IsNaN(matrix.M21) || float.IsInfinity(matrix.M21));
			result &= !(float.IsNaN(matrix.M22) || float.IsInfinity(matrix.M22));
			result &= !(float.IsNaN(matrix.M31) || float.IsInfinity(matrix.M31));
			result &= !(float.IsNaN(matrix.M32) || float.IsInfinity(matrix.M32));
#else
			result &= float.IsFinite(matrix.M11);
			result &= float.IsFinite(matrix.M12);
			result &= float.IsFinite(matrix.M21);
			result &= float.IsFinite(matrix.M22);
			result &= float.IsFinite(matrix.M31);
			result &= float.IsFinite(matrix.M32);
#endif
			return result;
		}

		public static Vector2 GetScale(this in Matrix3x2 matrix)
		{
			var sx = matrix.M12 == 0 ? Math.Abs(matrix.M11) : new Vector2(matrix.M11, matrix.M12).Length();
			var sy = matrix.M21 == 0 ? Math.Abs(matrix.M22) : new Vector2(matrix.M21, matrix.M22).Length();
			if (matrix.GetDeterminant() < 0) sy = -sy;
			return new Vector2(sx, sy);
		}

		public static float GetRotation(this in Matrix3x2 matrix)
		{
			return (float)Math.Atan2(matrix.M12, matrix.M11);
		}

		public static Vector2 GetTranslation(this in Matrix3x2 matrix)
		{
			return matrix.Translation;
		}

		public static Matrix3x2 WithScale(this Matrix3x2 matrix, Vector2 scale)
		{			
			var sx = matrix.M12 == 0 ? Math.Abs(matrix.M11) : new Vector2(matrix.M11, matrix.M12).Length();
			var sy = matrix.M21 == 0 ? Math.Abs(matrix.M22) : new Vector2(matrix.M21, matrix.M22).Length();
			// if (matrix.GetDeterminant() < 0) sy = -sy;

			scale /= new Vector2(sx, sy);

			matrix.M11 *= scale.X;
			matrix.M12 *= scale.X;
			matrix.M21 *= scale.Y;
			matrix.M22 *= scale.Y;
			return matrix;

			// var t = matrix.Translation;
			// var r = matrix.GetRotation();
			// return CreateMatrix3x2(scale, r, t);
		}

		public static Matrix3x2 WithoutScale(this in Matrix3x2 matrix)
		{
			return matrix.WithScale(Vector2.One);
		}

		public static Matrix3x2 WithRotation(this in Matrix3x2 matrix, float radians)
		{
			var t = matrix.Translation;
			var s = matrix.GetScale();
			return CreateMatrix3x2(s, radians, t);
		}

		public static Matrix3x2 WithoutRotation(this in Matrix3x2 matrix)
		{
			var t = matrix.Translation;
			var s = matrix.GetScale();
			return CreateMatrix3x2(s, 0, t);
		}

		public static Matrix3x2 WithTranslation(this Matrix3x2 matrix, Vector2 translation)
		{
			matrix.Translation = translation;
			return matrix;
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
		internal static Matrix3x2 CreateMatrix3x2(Vector2 scale, float rotation, Vector2 translation)
		{
			var m = Matrix3x2.CreateRotation(rotation);
			m.M11 *= scale.X;
			m.M12 *= scale.X;
			m.M21 *= scale.Y;
			m.M22 *= scale.Y;
			m.M31 = translation.X;
			m.M32 = translation.Y;
			return m;
		}		

		public static float GetLengthScale(this in Matrix3x2 matrix)
		{
			var determinant = matrix.GetDeterminant();
			var areaScale = Math.Abs(determinant);
			return (float)Math.Sqrt(areaScale);
		}

		public static void CopyTo(this in Matrix3x2 matrix, float[] dst, int offset = 0, int count = 6)
		{
			count = Math.Min(dst.Length, count);

			dst[offset + 0] = matrix.M11;
			dst[offset + 1] = matrix.M12;
			dst[offset + 2] = matrix.M21;
			dst[offset + 3] = matrix.M22;
			if (count > 4)
			{
				dst[offset + 4] = matrix.M31;
				dst[offset + 5] = matrix.M32;
			}
		}

		public static void DeconstructScales(this in Matrix3x2 value, out float scale, out float scalex, out float scaley)
		{
			var det = value.GetDeterminant();
			scale = (float)Math.Sqrt(Math.Abs(det));
			scalex = value.M12 == 0 ? Math.Abs(value.M11) : new Vector2(value.M11, value.M12).Length();
			scaley = value.M21 == 0 ? Math.Abs(value.M22) : new Vector2(value.M21, value.M22).Length();
			if (det < 0) scaley = -scaley;
		}
	}
}
