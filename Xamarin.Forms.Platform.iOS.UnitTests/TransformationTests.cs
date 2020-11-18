using System.Collections;
using System.Threading.Tasks;
using CoreAnimation;
using NUnit.Framework;

namespace Xamarin.Forms.Platform.iOS.UnitTests
{
	[TestFixture]
	public class TransformationTests : PlatformTestFixture
	{
		static IEnumerable TransformationCases
		{
			get
			{
				foreach (var element in BasicViews)
				{
					element.TranslationX = 10.0;
					element.TranslationY = 30.0;
					element.Rotation = 248.0;
					element.Scale = 2.0;
					element.ScaleX = 2.0;
					yield return CreateTestCase(element);
				}
			}
		}

		[Test, Category("Transformation"), TestCaseSource(nameof(TransformationCases))]
		[Description("View transformation should match renderer transformation")]
		public async Task TransformationConsistent(View view)
		{
			var expected = new CATransform3D
			{
				m11 = -1.4984f,
				m12 = -3.7087f,
				m21 = 1.8544f,
				m22 = -0.7492f,
				m33 = 2f,
				m41 = 10f,
				m42 = 30f,
				m44 = 1f,
			};
			var actual = await GetRendererProperty(view, r => r.NativeView.Layer.Transform, requiresLayout: true);
			AssertTransform3DEqual(actual, expected, 0.0001);
		}

		private static void AssertTransform3DEqual(CATransform3D actual, CATransform3D expected, double delta)
		{
			Assert.That((double)actual.m11, Is.EqualTo((double)expected.m11).Within(delta));
			Assert.That((double)actual.m12, Is.EqualTo((double)expected.m12).Within(delta));
			Assert.That((double)actual.m13, Is.EqualTo((double)expected.m13).Within(delta));
			Assert.That((double)actual.m14, Is.EqualTo((double)expected.m14).Within(delta));
			Assert.That((double)actual.m21, Is.EqualTo((double)expected.m21).Within(delta));
			Assert.That((double)actual.m22, Is.EqualTo((double)expected.m22).Within(delta));
			Assert.That((double)actual.m23, Is.EqualTo((double)expected.m23).Within(delta));
			Assert.That((double)actual.m24, Is.EqualTo((double)expected.m24).Within(delta));
			Assert.That((double)actual.m31, Is.EqualTo((double)expected.m31).Within(delta));
			Assert.That((double)actual.m32, Is.EqualTo((double)expected.m32).Within(delta));
			Assert.That((double)actual.m33, Is.EqualTo((double)expected.m33).Within(delta));
			Assert.That((double)actual.m34, Is.EqualTo((double)expected.m34).Within(delta));
			Assert.That((double)actual.m41, Is.EqualTo((double)expected.m41).Within(delta));
			Assert.That((double)actual.m42, Is.EqualTo((double)expected.m42).Within(delta));
			Assert.That((double)actual.m43, Is.EqualTo((double)expected.m43).Within(delta));
			Assert.That((double)actual.m44, Is.EqualTo((double)expected.m44).Within(delta));
		}
	}
}