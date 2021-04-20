using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Frame)]
	public partial class FrameHandlerTests : HandlerTestBase<FrameHandler, FrameStub>
	{
		[Fact(DisplayName = "BackgroundColor Initializes Correctly")]
		public async Task BackgroundColorInitializesCorrectly()
		{
			var frame = new FrameStub()
			{
				BackgroundColor = Colors.Yellow,
				Content = new LabelStub { Text = "Test" }
			};

			await ValidateBackgroundColor(frame, Colors.Yellow, () => frame.BackgroundColor = Colors.Yellow);
		}

		[Fact(DisplayName = "BorderColor Initializes Correctly")]
		public async Task BorderColorInitializesCorrectly()
		{
			var frame = new FrameStub()
			{
				BackgroundColor = Colors.White,
				BorderColor = Colors.Orange,
				CornerRadius = 10,
				HasShadow = true,
				Content = new LabelStub { Text = "Test" }
			};

			await ValidateBorderColor(frame, Colors.Orange, () => frame.BorderColor = Colors.Orange);
		}

		[Theory(DisplayName = "HasShadow Initializes Correctly")]
		[InlineData(true)]
		[InlineData(false)]
		public async Task HasShadowInitializesCorrectly(bool hasShadow)
		{
			var frame = new FrameStub()
			{
				BackgroundColor = Colors.White,
				BorderColor = Colors.Orange,
				CornerRadius = 10,
				HasShadow = hasShadow,
				Content = new LabelStub { Text = "Test" }
			};

			var handler = await CreateHandlerAsync(frame);

			Assert.Equal(hasShadow, HasNativeShadow(handler));
		}
	}
}