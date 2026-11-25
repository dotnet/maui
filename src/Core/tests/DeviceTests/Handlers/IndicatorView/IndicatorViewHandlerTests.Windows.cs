using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Media;
using Xunit;
using WShape = Microsoft.UI.Xaml.Shapes.Shape;

namespace Microsoft.Maui.DeviceTests
{
	public partial class IndicatorViewHandlerTests
	{
		[Fact]
		public async Task PositionBeyondMaximumVisibleSelectsLastVisibleIndicator()
		{
			var indicatorView = new IndicatorViewStub
			{
				Count = 5,
				MaximumVisible = 3,
				Position = 4,
				IndicatorColor = new SolidPaint(Colors.Red),
				SelectedIndicatorColor = new SolidPaint(Colors.Blue)
			};

			var colors = await GetValueAsync(indicatorView, handler =>
				handler.PlatformView.Items
					.Cast<WShape>()
					.Select(shape => Assert.IsType<SolidColorBrush>(shape.Fill).Color)
					.ToArray());

			Assert.Equal(
				new[] { Colors.Red.ToWindowsColor(), Colors.Red.ToWindowsColor(), Colors.Blue.ToWindowsColor() },
				colors);
		}
	}
}