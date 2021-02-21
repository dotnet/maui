using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Platform.Handlers.DeviceTests.Stubs;
using Xunit;

namespace Xamarin.Platform.Handlers.DeviceTests
{
	public partial class LabelHandlerTests : HandlerTestBase<LabelHandler>
	{
		[Fact]
		public async Task BackgroundColorInitializesCorrectly()
		{
			var label = new LabelStub()
			{
				BackgroundColor = Color.Blue,
				Text = "Test"
			};

			await ValidateNativeBackgroundColor(label, Color.Blue);
		}

		[Fact]
		public async Task TextInitializesCorrectly()
		{
			var label = new LabelStub()
			{
				Text = "Test"
			};

			await ValidatePropertyInitValue(label, () => label.Text, GetNativeText, label.Text);
		}

		[Fact]
		public async Task TextColorInitializesCorrectly()
		{
			var label = new LabelStub()
			{
				Text = "Test",
				TextColor = Color.Red
			};

			await ValidatePropertyInitValue(label, () => label.TextColor, GetNativeTextColor, label.TextColor);
		}
	}
}