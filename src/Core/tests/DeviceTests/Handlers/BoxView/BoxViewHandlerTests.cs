using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.BoxView)]
	public partial class BoxViewHandlerTests : CoreHandlerTestBase<ShapeViewHandler, BoxViewStub>
	{
		[Theory(DisplayName = "BoxView Initializes Correctly")]
		[InlineData(0xFF0000)]
		[InlineData(0x00FF00)]
		[InlineData(0x0000FF)]
		public async Task BoxViewInitializesCorrectly(uint color)
		{
			var expected = Color.FromUint(color);

			var boxView = new BoxViewStub()
			{
				Color = expected,
				Height = 100,
				Width = 200
			};

			await ValidateHasColor(boxView, expected);
		}
	}
}