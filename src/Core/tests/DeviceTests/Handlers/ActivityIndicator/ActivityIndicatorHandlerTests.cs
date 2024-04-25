using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.ActivityIndicator)]
	public partial class ActivityIndicatorHandlerTests : CoreHandlerTestBase<ActivityIndicatorHandler, ActivityIndicatorStub>
	{
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
	}
}