using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.Platform.Android.UnitTests
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
		public async Task OpacityConsistent(VisualElement element)
		{
			var expected = element.Opacity;
			var actual = await GetRendererProperty(element, ver => ver.View.Alpha, requiresParent: true);
			Assert.That((double)actual, Is.EqualTo(expected).Within(0.001d));
		}
	}
}