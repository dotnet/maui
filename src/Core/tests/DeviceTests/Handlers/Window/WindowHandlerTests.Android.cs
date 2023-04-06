using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Xunit;
using AActivity = Android.App.Activity;

namespace Microsoft.Maui.DeviceTests
{
	public partial class WindowHandlerTests : CoreHandlerTestBase
	{
		[Fact]
		public async Task TitleSetsOnWindow()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var testWindow = Application.Current.Windows[0];
				var activity = testWindow.Handler.PlatformView as AActivity;
				Assert.True(activity is not null, "Activity is Null");
				Assert.True(testWindow is not null, "Window is Null");

				testWindow.Title = "Test Title";
				Assert.Equal("Test Title", activity.Title);
				testWindow.Title = null;
				Assert.Equal(activity.Title, ApplicationModel.AppInfo.Current.Name);
			});
		}
	}
}