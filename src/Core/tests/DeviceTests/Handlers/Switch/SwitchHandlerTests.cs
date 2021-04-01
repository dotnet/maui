using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Switch)]
	public partial class SwitchHandlerTests : HandlerTestBase<SwitchHandler, SwitchStub>
	{
		[Fact(DisplayName = "Is Toggled Initializes Correctly")]
		public async Task IsToggledInitializesCorrectly()
		{
			var switchStub = new SwitchStub()
			{
				IsToggled = true
			};

			await ValidatePropertyInitValue(switchStub, () => switchStub.IsToggled, GetNativeIsChecked, switchStub.IsToggled);
		}

		[Theory(DisplayName = "Track Color Initializes Correctly")]
		[InlineData(true)]
		[InlineData(false)]
		public async Task TrackColorInitializesCorrectly(bool isToggled)
		{
			var switchStub = new SwitchStub()
			{
				IsToggled = isToggled,
				TrackColor = Colors.Red
			};

			await ValidateTrackColor(switchStub, Colors.Red);
		}

		[Fact(DisplayName = "Track Color Updates Correctly")]
		public async Task TrackColorUpdatesCorrectly()
		{
			var switchStub = new SwitchStub()
			{
				IsToggled = true
			};

			await ValidateTrackColor(switchStub, Colors.Red, () => switchStub.TrackColor = Colors.Red);
		}

		[Fact(DisplayName = "ThumbColor Initializes Correctly")]
		public async Task ThumbColorInitializesCorrectly()
		{
			var switchStub = new SwitchStub()
			{
				IsToggled = true,
				ThumbColor = Colors.Blue
			};

			await ValidateThumbColor(switchStub, Colors.Blue);
		}

		[Fact(DisplayName = "Track Color Updates Correctly")]
		public async Task ThumbColorUpdatesCorrectly()
		{
			var switchStub = new SwitchStub()
			{
				IsToggled = true
			};

			await ValidateThumbColor(switchStub, Colors.Red, () => switchStub.ThumbColor = Colors.Red);
		}
	}
}