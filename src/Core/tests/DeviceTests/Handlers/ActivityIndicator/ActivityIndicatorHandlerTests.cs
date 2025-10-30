﻿using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.ActivityIndicator)]
	public partial class ActivityIndicatorHandlerTests : CoreHandlerTestBase<ActivityIndicatorHandler, ActivityIndicatorStub>
	{
#if !WINDOWS // On Windows, the platform control will return IsActive as true even when the control is not visible.
		[Theory(DisplayName = "IsRunning Should Respect IsVisible")]
		[InlineData(true,true)]
		[InlineData(true,false)]
		[InlineData(false,true)]
		[InlineData(false,false)]
		public async  Task IsRunningShouldRespectIsVisible(bool _isRunning,bool _isVisible)
		{
			var activityIndicator = new ActivityIndicatorStub
			{
				IsRunning = _isRunning,
				Visibility = _isVisible  ? Visibility.Visible :Visibility.Hidden
			};

			bool isAnimating = false;

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler(activityIndicator);
				isAnimating = GetNativeIsRunning(handler);
			});

			if (_isVisible && _isRunning)
            	Assert.True(isAnimating);
    		else
        		Assert.False(isAnimating);

		}
#endif

		[Theory(DisplayName = "IsRunning Initializes Correctly")]
		[InlineData(true)]
		[InlineData(false)]
		public async Task IsRunningInitializesCorrectly(bool isRunning)
		{
			var activityIndicator = new ActivityIndicatorStub()
			{
				IsRunning = isRunning
			};

			await ValidatePropertyInitValue(activityIndicator, () => activityIndicator.IsRunning, GetNativeIsRunning, activityIndicator.IsRunning);
		}

		[Theory(DisplayName = "Background Updates Correctly")]
		[InlineData(0xFFFF0000)]
		[InlineData(0xFF00FF00)]
		[InlineData(0xFF0000FF)]
		public async Task BackgroundUpdatesCorrectly(uint color)
		{
			var expected = Color.FromUint(color);

			var activityIndicator = new ActivityIndicatorStub()
			{
				Background = new SolidPaintStub(Color.FromUint(0xFF888888)),
				IsRunning = true
			};

			await ValidateHasColor(activityIndicator, expected, () => activityIndicator.Background = new SolidPaintStub(expected), nameof(activityIndicator.Background));
		}

#if WINDOWS
		[Theory(DisplayName = "Foreground Updates Correctly")]
		[InlineData(0xFFFF0000)]
		[InlineData(0xFF00FF00)]
		[InlineData(0xFF0000FF)]
		public async Task ForegroundUpdatesCorrectly(uint color)
		{
			var expected = Color.FromUint(color);

			var activityIndicator = new ActivityIndicatorStub()
			{
				Color = Color.FromUint(0xFF888888),
				IsRunning = true
			};

			await ValidateHasColor(activityIndicator, expected, () => activityIndicator.Color = expected, nameof(activityIndicator.Color));
		}
#endif
	}
}
