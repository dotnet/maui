using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class IndicatorViewHandlerTests
	{
		[Theory(DisplayName = "IndicatorView Background Color Initializes Correctly on Windows")]
		[InlineData(0xFFFF0000)]
		[InlineData(0xFF00FF00)]
		[InlineData(0xFF0000FF)]
		public async Task BackgroundColorInitializesCorrectly(uint color)
		{
			var expected = Color.FromUint(color);

			var indicatorView = new IndicatorViewStub()
			{
				Count = 3,
				Position = 0,
				Background = new SolidPaintStub(expected),
			};

			await ValidateHasColor(indicatorView, expected);
		}

		[Theory(DisplayName = "IndicatorView Background Color Updates Correctly on Windows")]
		[InlineData(0xFFFF0000)]
		[InlineData(0xFF00FF00)]
		[InlineData(0xFF0000FF)]
		public async Task BackgroundColorUpdatesCorrectly(uint color)
		{
			var expected = Color.FromUint(color);

			var indicatorView = new IndicatorViewStub()
			{
				Count = 3,
				Position = 0,
				Background = new SolidPaintStub(Colors.Grey),
			};

			await ValidateHasColor(indicatorView, expected, () => indicatorView.Background = new SolidPaintStub(expected), nameof(indicatorView.Background));
		}
	}
}