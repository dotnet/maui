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
				IsOn = true
			};

			await ValidatePropertyInitValue(switchStub, () => switchStub.IsOn, GetNativeIsOn, switchStub.IsOn);
		}

		[Theory(DisplayName = "Track Color Initializes Correctly")]
		[InlineData(true)]
		[InlineData(false)]
		public async Task TrackColorInitializesCorrectly(bool isToggled)
		{
			var switchStub = new SwitchStub()
			{
				IsOn = isToggled,
				TrackColor = Colors.Red
			};

			await ValidateTrackColor(switchStub, Colors.Red);
		}

		[Fact(DisplayName = "Track Color Updates Correctly")]
		public async Task TrackColorUpdatesCorrectly()
		{
			var switchStub = new SwitchStub()
			{
				IsOn = true
			};

			await ValidateTrackColor(switchStub, Colors.Red, () => switchStub.TrackColor = Colors.Red);
		}

		[Fact(DisplayName = "ThumbColor Initializes Correctly")]
		public async Task ThumbColorInitializesCorrectly()
		{
			var switchStub = new SwitchStub()
			{
				IsOn = true,
				ThumbColor = Colors.Blue
			};

			await ValidateThumbColor(switchStub, Colors.Blue);
		}

		[Fact(DisplayName = "Track Color Updates Correctly")]
		public async Task ThumbColorUpdatesCorrectly()
		{
			var switchStub = new SwitchStub()
			{
				IsOn = true
			};

			await ValidateThumbColor(switchStub, Colors.Red, () => switchStub.ThumbColor = Colors.Red);
		}

		[Fact(DisplayName = "Updating Native Is On property updates Virtual View"
#if __IOS__
			  ,Skip = "iOS doesn't throw ValueChanged events when changing property via code."
#endif
			)]
		public async Task NativeIsOnPropagatesToVirtual()
		{
			var switchStub = new SwitchStub()
			{
				IsOn = false
			};

			bool isOn = false;
			switchStub.IsOnDelegate += () =>
			{
				isOn = switchStub.IsOn;
			};

			await SetValueAsync(switchStub, true, SetIsOn);

			Assert.True(isOn);
		}
	}
}