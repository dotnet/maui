using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class WindowHandlerTests : CoreHandlerTestBase
	{
		[Fact]
		public async Task TitleSetsOnWindow()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var activity = MauiContext.Context.GetActivity();
				Assert.NotNull(activity);
				var testWindow = activity.GetWindow() as Window;
				Assert.NotNull(testWindow);

				testWindow.Title = "Test Title";
				Assert.Equal("Test Title", activity.Title);
				testWindow.Title = null;
				Assert.Equal(activity.Title, ApplicationModel.AppInfo.Current.Name);
			});
		}
	}
}