using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using static Xamarin.Forms.Core.UITests.NumericExtensions;
using static Xamarin.Forms.Core.UITests.ParsingUtils;

namespace Xamarin.Forms.ControlGallery.iOS.Tests
{
	[TestFixture]
	public class ScaleTests : PlatformTestFixture
	{
		static IEnumerable ScaleCases
		{
			get
			{
				foreach (var element in BasicViews)
				{
					element.Scale = 2.0;
					yield return CreateTestCase(element);
				}
			}
		}

		[Test, Category("Scale"), TestCaseSource(nameof(ScaleCases))]
		[Description("View scale should match renderer scale")]
		public async Task ScaleConsistent(View view)
		{
			var transform = await GetRendererProperty(view, r => r.NativeView.Layer.Transform, requiresLayout: true);
			var actual = ParseCATransform3D(transform.ToString());
			var expected = BuildScaleMatrix((float)view.Scale);
			Assert.That(actual, Is.EqualTo(expected));
		}
	}
}