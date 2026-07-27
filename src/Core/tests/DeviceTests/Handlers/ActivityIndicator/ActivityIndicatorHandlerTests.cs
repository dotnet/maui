using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.ActivityIndicator)]
	public partial class ActivityIndicatorHandlerTests : CoreHandlerTestBase<ActivityIndicatorHandler, ActivityIndicatorStub>
	{
#if !WINDOWS // On Windows, the platform control will return IsActive as true even when the control is not visible.
		[Theory(DisplayName = "IsRunning Should Respect Visibility At Init")]
		[InlineData(true, true)]
		[InlineData(true, false)]
		[InlineData(false, true)]
		[InlineData(false, false)]
		public async Task IsRunningShouldRespectVisibilityAtInit(bool isRunning, bool isVisible)
		{
			var activityIndicator = new ActivityIndicatorStub
			{
				IsRunning = isRunning,
				Visibility = isVisible ? Visibility.Visible : Visibility.Hidden
			};

			bool isAnimating = false;

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler(activityIndicator);
				isAnimating = GetNativeIsRunning(handler);
			});

			if (isVisible && isRunning)
				Assert.True(isAnimating);
			else
				Assert.False(isAnimating);
		}

		[Fact(DisplayName = "Setting IsRunning After Init Should Respect Hidden Visibility")]
		public async Task SettingIsRunningAfterInitShouldRespectHiddenVisibility()
		{
			var activityIndicator = new ActivityIndicatorStub
			{
				IsRunning = false,
				Visibility = Visibility.Hidden
			};

			bool isAnimating = false;

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler(activityIndicator);

				// Simulate runtime: set IsRunning=true while Visibility=Hidden
				activityIndicator.IsRunning = true;
				handler.UpdateValue(nameof(IActivityIndicator.IsRunning));

				isAnimating = GetNativeIsRunning(handler);
			});

			Assert.False(isAnimating, "ActivityIndicator should not animate when Visibility is Hidden");
		}

		[Fact(DisplayName = "Setting IsRunning After Init Should Respect Collapsed Visibility")]
		public async Task SettingIsRunningAfterInitShouldRespectCollapsedVisibility()
		{
			var activityIndicator = new ActivityIndicatorStub
			{
				IsRunning = false,
				Visibility = Visibility.Collapsed
			};

			bool isAnimating = false;

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler(activityIndicator);

				// Simulate runtime: set IsRunning=true while Visibility=Collapsed
				activityIndicator.IsRunning = true;
				handler.UpdateValue(nameof(IActivityIndicator.IsRunning));

				isAnimating = GetNativeIsRunning(handler);
			});

			Assert.False(isAnimating, "ActivityIndicator should not animate when Visibility is Collapsed");
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
