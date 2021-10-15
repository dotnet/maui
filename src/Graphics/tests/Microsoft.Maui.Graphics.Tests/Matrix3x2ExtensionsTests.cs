using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Xunit;

namespace Microsoft.Maui.Graphics.Tests
{
	public class Matrix3x2ExtensionsTests
	{
		public static IEnumerable<object[]> ScaleRotationData()
		{
			yield return new object[] { 0, 1, 1 };
			yield return new object[] { 0, -1, -1 };
			yield return new object[] { 1, 2, 3 };
			yield return new object[] { -2, 1, 1 };
			yield return new object[] { -1, 5, 2 };
			yield return new object[] { -3, 1, -2 };
			yield return new object[] { 0, -1, 2 };
			yield return new object[] { 0, 1, -2 };
			yield return new object[] { 3, -1, -2 };
			yield return new object[] { 1, -2, -2 };
			yield return new object[] { -1, -2, -2 };
		}

		[Fact]
		public void TestEquivalence()
		{
			// the identity matrix should be equivalent to a matrix
			// with negative scale and rotated 180 degrees

			var a = Matrix3x2.Identity;

			var b = Matrix3x2.Identity;
			b *= Matrix3x2.CreateScale(-1, -1);
			b *= Matrix3x2.CreateRotation((float)Math.PI);

			AssertEqual(a, b, 6);
		}

		[Theory]
		[MemberData(nameof(ScaleRotationData))]
		public void TestDeconstruct(float rotation, float scaleX, float scaleY)
		{
			var translation = new Vector2(5, 9);

			var reference = Matrix3x2.Identity;
			reference *= Matrix3x2.CreateScale(scaleX, scaleY);
			reference *= Matrix3x2.CreateRotation(rotation);
			reference *= Matrix3x2.CreateTranslation(translation);
			ReferenceDeconstruct(reference, out var refScale, out var refRot, out var refTrans);

			// deconstruct

			var s = reference.GetScale();
			var r = reference.GetRotation();
			var t = reference.GetTranslation();
			
			AssertEqual(refScale, s, 5);
			Assert.Equal(refRot, r, 5);
			AssertEqual(refTrans, t, 5);			

			var m = Matrix3x2Extensions.CreateMatrix3x2(s, r, t);
			AssertEqual(reference, m, 5);
		}

		[Theory]
		[MemberData(nameof(ScaleRotationData))]
		public void TestRotationAndScale(float rotation, float scaleX, float scaleY)
		{
			var translation = new Vector2(5, 9);

			var reference = Matrix3x2.Identity;
			reference *= Matrix3x2.CreateScale(scaleX, scaleY);
			reference *= Matrix3x2.CreateRotation(rotation);
			reference *= Matrix3x2.CreateTranslation(translation);			

			ReferenceDeconstruct(reference, out var refScale, out var refRot, out var refTrans);			

			// concatenation
			// notice that AffineTransform concatenation order
			// is in reverse order compared to Matrix3x2

			var m1 = Matrix3x2.Identity;
			m1 *= Matrix3x2.CreateScale(scaleX, scaleY);
			m1 *= Matrix3x2.CreateRotation(rotation);
			m1 *= Matrix3x2.CreateTranslation(translation);
			AssertOrthogonal(m1, 7);
			AssertEqual(reference, m1, 7);

			// concatenation and rotation assign

			m1 = Matrix3x2.Identity;
			m1 *= Matrix3x2.CreateScale(scaleX, scaleY);			
			m1 = m1.WithRotation(scaleX >= 0 ? rotation : rotation + (float)Math.PI);
			m1 *= Matrix3x2.CreateTranslation(translation);
			AssertOrthogonal(m1, 7);
			AssertEqual(reference, m1, 4);

			// concatenation and scale assign

			m1 = Matrix3x2.Identity;
			m1 = m1.WithScale(new Vector2(scaleX, scaleY));
			m1 *= Matrix3x2.CreateRotation(rotation);
			m1 *= Matrix3x2.CreateTranslation(translation);
			AssertOrthogonal(m1, 7);
			AssertEqual(reference, m1, 7);

			// deconstruct/reconstruct

			m1 = Matrix3x2Extensions.CreateMatrix3x2(new Vector2(scaleX, scaleY), rotation, translation);
			var s = m1.GetScale();
			var r = m1.GetRotation();
			var t = m1.GetTranslation();
			m1 = Matrix3x2Extensions.CreateMatrix3x2(s, r, t);
			AssertOrthogonal(m1, 7);
			AssertEqual(reference, m1, 5);			
		}

		[Fact]
		public void TestSettingScale()
		{
			var m = Matrix3x2.CreateRotation(2);

			m = m.WithScale(new Vector2(3, 4));
			Assert.Equal(2, m.GetRotation(), 4);
			AssertEqual(new Vector2(3, 4), m.GetScale(), 5);

			m = m.WithScale(new Vector2(2, 2));
			Assert.Equal(2, m.GetRotation(), 4);
			AssertEqual(new Vector2(2, 2), m.GetScale(), 5);
		}

		[Fact]
		public void TestAverageScale()
		{
			for (float r = 0; r < 3; r += 1f)
			{
				var rm = Matrix3x2.CreateRotation(r);

				Assert.Equal(1, (Matrix3x2.CreateScale(1, 1) * rm).GetLengthScale(), 6);
				Assert.Equal(1, (Matrix3x2.CreateScale(-1, -1) * rm).GetLengthScale(), 6);
				Assert.Equal(2, (Matrix3x2.CreateScale(2, 2) * rm).GetLengthScale(), 6);
				Assert.Equal(1.414214f, (Matrix3x2.CreateScale(1, 2) * rm).GetLengthScale(), 5);
				Assert.Equal(1.414214f, (Matrix3x2.CreateScale(2, 1) * rm).GetLengthScale(), 5);
				Assert.Equal(1.414214f, (Matrix3x2.CreateScale(-2, 1) * rm).GetLengthScale(), 5);				
			}
		}


		private static void AssertEqual(Vector2 a, Vector2 b, int precision)
		{
			Assert.Equal(a.X, b.X, precision);
			Assert.Equal(a.Y, b.Y, precision);
		}

		private static void AssertEqual(Matrix3x2 a, Matrix3x2 b, int precision)
		{
			Assert.Equal(a.M11, b.M11, precision);
			Assert.Equal(a.M21, b.M21, precision);
			Assert.Equal(a.M31, b.M31, precision);
			Assert.Equal(a.M12, b.M12, precision);
			Assert.Equal(a.M22, b.M22, precision);
			Assert.Equal(a.M32, b.M32, precision);
		}

		private static void AssertOrthogonal(Matrix3x2 transform, int precission)
		{
			var x = Vector2.Normalize(new Vector2(transform.M11, transform.M12));
			var y = Vector2.Normalize(new Vector2(transform.M21, transform.M22));
			var d = Vector2.Dot(x, y);
			if (d < -1) d = -1;
			if (d > 1) d = 1;

			var skewAngle = (float)((Math.PI / 2) - Math.Acos(d));

			Assert.Equal(0, skewAngle, precission);
		}

		private static void ReferenceDeconstruct(Matrix3x2 m, out Vector2 scale, out float rotation, out Vector2 translation)
		{
			// Matrix3x2 lacks decompose, but Matrix4x4 has it.

			Matrix4x4.Decompose(new Matrix4x4(m), out var s, out Quaternion r, out var t);

			scale = new Vector2(s.X, s.Y);
			translation = new Vector2(t.X, t.Y);

			var hand = Vector3.Transform(Vector3.UnitX, r);

			rotation = (float)Math.Atan2(hand.Y, hand.X);
		}
	}
}
