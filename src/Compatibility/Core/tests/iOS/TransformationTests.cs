using System.Collections;
using System.Threading.Tasks;
using CoreAnimation;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS.UnitTests
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
				M11 = -1.4984f,
				M12 = -3.7087f,
				M21 = 1.8544f,
				M22 = -0.7492f,
				M33 = 2f,
				M41 = 10f,
				M42 = 30f,
				M44 = 1f,
			};
			var actual = await GetRendererProperty(view, r => r.PlatformView.Layer.Transform, requiresLayout: true);
			AssertTransform3DEqual(actual, expected, 0.0001);
		}

		private static void AssertTransform3DEqual(CATransform3D actual, CATransform3D expected, double delta)
		{
			Assert.That((double)actual.M11, Is.EqualTo((double)expected.M11).Within(delta));
			Assert.That((double)actual.M12, Is.EqualTo((double)expected.M12).Within(delta));
			Assert.That((double)actual.M13, Is.EqualTo((double)expected.M13).Within(delta));
			Assert.That((double)actual.M14, Is.EqualTo((double)expected.M14).Within(delta));
			Assert.That((double)actual.M21, Is.EqualTo((double)expected.M21).Within(delta));
			Assert.That((double)actual.M22, Is.EqualTo((double)expected.M22).Within(delta));
			Assert.That((double)actual.M23, Is.EqualTo((double)expected.M23).Within(delta));
			Assert.That((double)actual.M24, Is.EqualTo((double)expected.M24).Within(delta));
			Assert.That((double)actual.M31, Is.EqualTo((double)expected.M31).Within(delta));
			Assert.That((double)actual.M32, Is.EqualTo((double)expected.M32).Within(delta));
			Assert.That((double)actual.M33, Is.EqualTo((double)expected.M33).Within(delta));
			Assert.That((double)actual.M34, Is.EqualTo((double)expected.M34).Within(delta));
			Assert.That((double)actual.M41, Is.EqualTo((double)expected.M41).Within(delta));
			Assert.That((double)actual.M42, Is.EqualTo((double)expected.M42).Within(delta));
			Assert.That((double)actual.M43, Is.EqualTo((double)expected.M43).Within(delta));
			Assert.That((double)actual.M44, Is.EqualTo((double)expected.M44).Within(delta));
		}
	}
}