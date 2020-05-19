using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace System.Maui.Platform.iOS.UnitTests
{
	[TestFixture]
	public class OpacityTests : PlatformTestFixture
	{
		static readonly double TestOpacity = 0.4;

		static IEnumerable TestCases
		{
			get
			{
				foreach (var element in BasicViews)
				{
					element.Opacity = TestOpacity;
					yield return CreateTestCase(element);
				}
			}
		}

		[Test, Category("Opacity"), TestCaseSource(nameof(TestCases))]
		[Description("VisualElement opacity should match renderer opacity")]
		public async Task OpacityConsistent(View view)
		{
			var expected = view.Opacity;
			var actual = await GetRendererProperty(view, r => r.NativeView.Alpha, requiresLayout: true);

			// Deliberately casting this to double because Within doesn't seem to grasp nfloat
			// If you write this the other way around (casting expected to an nfloat), it fails
			Assert.That((double)actual, Is.EqualTo(expected).Within(0.001d));
		}
	}
}