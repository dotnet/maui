using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using Xunit;

namespace Microsoft.Maui.Graphics.Tests
{
	public class AffineTransformTests
	{
		public static IEnumerable<object[]> AffineTransformData()
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

			var a = new AffineTransform();

			var b = new AffineTransform();
			b.ConcatenateRotationInDegrees(180);
			b.ConcatenateScale(-1, -1);			

			AssertEqual(a, b, 6);
		}

		[Theory]
		[MemberData(nameof(AffineTransformData))]
		public void TestDeconstruct(float rotation, float scaleX, float scaleY)
		{
			var translation = new Vector2(5, 9);

			var reference = Matrix3x2.Identity;
			reference *= Matrix3x2.CreateScale(scaleX, scaleY);
			reference *= Matrix3x2.CreateRotation(rotation);
			reference *= Matrix3x2.CreateTranslation(translation);
			ReferenceDeconstruct(reference, out var refScale, out var refRot, out var refTrans);

			AffineTransform at = reference;
			at.Deconstruct(out var atTrans, out var atRot, out var atScale);

			AssertEqual(refTrans, atTrans, 5);
			Assert.Equal(refRot, atRot, 5);
			AssertEqual(refScale, atScale, 5);

			at = AffineTransform.GetInstance(refTrans, refRot, (SizeF)refScale);
			AssertEqual(reference, at, 5);
		}

		[Theory]
		[MemberData(nameof(AffineTransformData))]
		public void TestRotationAndScale(float rotation, float scaleX, float scaleY)
		{
			var translation = new Vector2(5, 9);
			
			var reference = Matrix3x2.Identity;			
			reference *= Matrix3x2.CreateScale(scaleX, scaleY);
			reference *= Matrix3x2.CreateRotation(rotation);
			reference *= Matrix3x2.CreateTranslation(translation);

			ReferenceDeconstruct(reference, out var refScale, out var refRot, out var refTrans);

			// implicit conversion

			AffineTransform at = reference;
			AssertOrthogonal(at, 7);
			AssertEqual(reference, at, 9);

			// concatenation
			// notice that AffineTransform concatenation order
			// is in reverse order compared to Matrix3x2

			at = new AffineTransform();
			at.ConcatenateTranslation(translation);
			at.ConcatenateRotation(rotation);
			at.ConcatenateScale(scaleX, scaleY);
			AssertOrthogonal(at, 7);
			AssertEqual(reference, at, 7);

			// concatenation and rotate assign

			
			at = new AffineTransform();
			at.ConcatenateTranslation(translation);
			at.ConcatenateScale(scaleX, scaleY);
			at.Rotation = scaleX >= 0 ? rotation : rotation + (float)Math.PI;
			AssertOrthogonal(at, 7);
			AssertEqual(reference, at, 4);			

			// concatenation and scale assign
			
			at = new AffineTransform();
			at.ConcatenateTranslation(translation);
			at.ConcatenateRotation(rotation);
			at.Scale = new SizeF(scaleX, scaleY); // changes scaling without affecting rotation/translation
			AssertOrthogonal(at, 7);
			AssertEqual(reference, at, 7);

			// deconstruct/reconstruct

			at = AffineTransform.GetInstance(translation,rotation, new SizeF(scaleX,scaleY) );
			at.Deconstruct(out var t, out var r, out var s);			
			at = AffineTransform.GetInstance(t, r, s);
			AssertOrthogonal(at, 7);
			AssertEqual(reference, at, 5);
		}

		[Theory]
		[MemberData(nameof(AffineTransformData))]
		public void TestTransformPoint(float rotation, float scaleX, float scaleY)
		{
			var translation = new Vector2(5, 9);
			var point = new Vector2(7, -9);			

			// notice the concatenation order of Matrix3x2 and
			// AffineTransform are defined in reverse order.
			var reference = Matrix3x2.Identity;
			reference *= Matrix3x2.CreateScale(scaleX, scaleY);
			reference *= Matrix3x2.CreateRotation(rotation);
			reference *= Matrix3x2.CreateTranslation(translation);

			var pointx = Vector2.Transform(point, reference);

			// create a matrix equivalent to reference.

			var at = new AffineTransform();
			at.ConcatenateTranslation(translation);
			at.ConcatenateRotation(rotation);
			at.ConcatenateScale(scaleX, scaleY);
			AssertEqual(reference, at, 5);
			AssertEqual(pointx, at.Transform(point), 8);

			// create in one shot

			at = AffineTransform.GetInstance(translation, rotation, new SizeF(scaleX,scaleY));
			AssertEqual(reference, at, 5);
			AssertEqual(pointx, at.Transform(point), 8);

			// de/reconstruct T R S for roundtrip

			at.Deconstruct(out var t, out var r, out var s);
			at = AffineTransform.GetInstance(t, r, s);
			AssertEqual(reference, at, 5);
			AssertEqual(pointx, at.Transform(point), 4);
		}		

		private static void AssertEqual(AffineTransform a, AffineTransform b, int precision)
		{
			Assert.Equal(a.M11, b.M11, precision);
			Assert.Equal(a.M21, b.M21, precision);
			Assert.Equal(a.M31, b.M31, precision);
			Assert.Equal(a.M12, b.M12, precision);
			Assert.Equal(a.M22, b.M22, precision);
			Assert.Equal(a.M32, b.M32, precision);
		}

		private static void AssertEqual(PointF a, PointF b, int precision)
		{
			Assert.Equal(a.X, b.X, precision);
			Assert.Equal(a.Y, b.Y, precision);
		}		

		private static void AssertEqual(Vector2 a, SizeF b, int precision)
		{
			Assert.Equal(a.X, b.Width, precision);
			Assert.Equal(a.Y, b.Height, precision);
		}		
		
		private static void AssertOrthogonal(AffineTransform transform, int precission)
		{
			var x = Vector2.Normalize(new Vector2(transform.M11, transform.M12));
			var y = Vector2.Normalize(new Vector2(transform.M21, transform.M22));
			var d = Vector2.Dot(x, y);			
			if (d < -1) d = -1;
			if (d >  1) d =  1;

			var skewAngle = (float) ((Math.PI/2) - Math.Acos(d));

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
