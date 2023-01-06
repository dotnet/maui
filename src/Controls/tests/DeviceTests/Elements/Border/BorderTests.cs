using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
#if !TIZEN
	[Category(TestCategory.Border)]
	public partial class BorderTests : ControlsHandlerTestBase
	{
		[Fact(DisplayName = "Rounded Rectangle Border occupies correct space")]
		public async Task RoundedRectangleBorderLayoutIsCorrect()
		{
			var expected = Colors.Red;

			var container = new Grid();
			container.WidthRequest = 100;
			container.HeightRequest = 100;

			var border = new Border()
			{
				Stroke = Colors.Red,
				StrokeThickness = 1,
				BackgroundColor = Colors.Red,
				HeightRequest = 100,
				WidthRequest = 100
			};

			await AssertColorAtPoint(border, expected, typeof(BorderHandler), 10, 10);
		}
	}
#endif
}
