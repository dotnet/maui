using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using UIKit;
using Xamarin.Forms.Shapes;
using static Xamarin.Forms.Core.UITests.NumericExtensions;
using static Xamarin.Forms.Core.UITests.ParsingUtils;

namespace Xamarin.Forms.Platform.iOS.UnitTests
{
	[TestFixture]
	public class ShapeTests : PlatformTestFixture
	{
		public ShapeTests()
		{
		}

		[Test, Category("Shape")]
		[Description("Reused ShapeView Renderers Correctly")]
		public async Task ReusedShapeViewReRenderers()
		{
			var view = new Xamarin.Forms.Shapes.Rectangle
			{
				Fill = SolidColorBrush.Purple,
				HeightRequest = 21,
				WidthRequest = 21,
				Stroke = SolidColorBrush.Purple
			};

			var expected = await GetRendererProperty(view, (ver) => ver.NativeView.ToBitmap(), requiresLayout: true);

			var actual = await GetRendererProperty(view, (ver) => ver.NativeView.ToBitmap(), requiresLayout: true);

			await expected.AssertEqualsAsync(actual);
		}
	}
}
