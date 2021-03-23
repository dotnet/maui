using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using UIKit;
using static Microsoft.Maui.Controls.Compatibility.UITests.NumericExtensions;
using static Microsoft.Maui.Controls.Compatibility.UITests.ParsingUtils;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS.UnitTests
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
			var view = new Microsoft.Maui.Controls.Shapes.Rectangle
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
