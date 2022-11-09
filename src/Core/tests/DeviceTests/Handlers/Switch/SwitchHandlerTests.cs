using System;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Switch)]
	public partial class SwitchHandlerTests : CoreHandlerTestBase<SwitchHandler, SwitchStub>
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

		[Fact(DisplayName = "Is Toggled Does Not Set Same Value")]
		public async Task IsToggledDoesNotSetSameValue()
		{
			var fireCount = 0;

			var switchStub = new SwitchStub()
			{
				IsOn = true,
				IsOnDelegate = () => fireCount++
			};

			await InvokeOnMainThreadAsync(() => CreateHandler(switchStub));

			Assert.Equal(0, fireCount);
		}

		[Theory(DisplayName = "Track Color Initializes Correctly"
#if WINDOWS
			, Skip = "There seems to be an issue, so disable for now: https://github.com/dotnet/maui/issues/9113"
#endif
			)]
		[InlineData(true)]
		//[InlineData(false)] // Track color is not always visible when off
		public async Task TrackColorInitializesCorrectly(bool isToggled)
		{
			var switchStub = new SwitchStub()
			{
				IsOn = isToggled,
				TrackColor = Colors.Red
			};

			await ValidateTrackColor(switchStub, Colors.Red);
		}

		[Fact(DisplayName = "Track Color Updates Correctly"
#if WINDOWS
			, Skip = "There seems to be an issue, so disable for now: https://github.com/dotnet/maui/issues/9113"
#endif
			)]
		public async Task TrackColorUpdatesCorrectly()
		{
			var switchStub = new SwitchStub()
			{
				IsOn = true
			};

			await ValidateTrackColor(switchStub, Colors.Red, () => switchStub.TrackColor = Colors.Red);
		}

		[Fact(DisplayName = "ThumbColor Initializes Correctly", Skip = "There seems to be an issue, so disable for now: https://github.com/dotnet/maui/issues/1275")]
		public async Task ThumbColorInitializesCorrectly()
		{
			var switchStub = new SwitchStub()
			{
				IsOn = true,
				ThumbColor = Colors.Blue
			};

			await ValidateThumbColor(switchStub, Colors.Blue);
		}


		[Fact(DisplayName = "Null Thumb Color Doesn't Crash")]
		public async Task NullThumbColorDoesntCrash()
		{
			var switchStub = new SwitchStub()
			{
				IsOn = true,
				ThumbColor = null,
			};

			await CreateHandlerAsync(switchStub);
		}

		[Fact(DisplayName = "Thumb Color Updates Correctly")]
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
			  , Skip = "iOS doesn't throw ValueChanged events when changing property via code."
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

		[Fact(DisplayName = "Track Color's view is set when toggled on"
#if !__IOS__
			  , Skip = "Testing iOS Track Changes."
#endif
			)]
		public async Task OnTrackColorSetVisually()
		{
			var switchStub = new SwitchStub()
			{
				IsOn = false,
				TrackColor = Colors.Red,
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				await ValidatePropertyUpdatesValue(
					view: switchStub,
					property: nameof(ISwitch.IsOn),
					GetPlatformValue: (handler) => handler.PlatformView.On,
					expectedSetValue: true,
					expectedUnsetValue: false);

				await ValidateVisualTrackColor(switchStub, UIColor.Red);
			});
		}

		[Fact(DisplayName = "Track Color's view is default color when toggled off"
#if !__IOS__
			, Skip = "Testing iOS UIView Track Changes."
#endif
			)]
		public async Task OffTrackColorSetToDefaultColor()
		{
			var switchStub = new SwitchStub()
			{
				IsOn = true,
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				await ValidatePropertyUpdatesValue(
					view: switchStub,
					property: nameof(ISwitch.IsOn),
					GetPlatformValue: (handler) => handler.PlatformView.On,
					expectedSetValue: false,
					expectedUnsetValue: true);

				var color = OperatingSystem.IsIOSVersionAtLeast(13) ? UIColor.SecondarySystemFill : UIColor.FromRGBA(120, 120, 128, 40);

				await ValidateVisualTrackColor(switchStub, color);
			});
		}

		[Fact(DisplayName = "Apple Changed UITrack Subviews"
#if !__IOS__
			, Skip = "Testing iOS UIView Track Changes."
#endif
	)]
		public async Task UIViewSubviewExists()
		{
			var switchStub = new SwitchStub();

			await InvokeOnMainThreadAsync(async () =>
			{
				await ValidateTrackSubViewExists(switchStub);
			});
		}
	}
}