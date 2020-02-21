using System.Collections;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.ControlGallery.Android.Tests
{
	[TestFixture]
	public class OpacityTests : PlatformTestFixture
	{
		static IEnumerable TestCases
		{
			get
			{
				foreach (var element in BasicElements)
				{
					element.Opacity = 0.35;
					yield return CreateTestCase(element);
				}
			}
		}

		[Test, Category("Opacity"), TestCaseSource(nameof(TestCases))]
		[Description("VisualElement opacity should match renderer opacity")]
		public void OpacityConsistent(VisualElement element)
		{
			using (var renderer = GetRenderer(element))
			{
				var expected = element.Opacity;
				var nativeView = renderer.View;

				ParentView(nativeView);

				Assert.That((double)renderer.View.Alpha, Is.EqualTo(expected).Within(0.001d));

				UnparentView(nativeView);
			}
		}
	}
}