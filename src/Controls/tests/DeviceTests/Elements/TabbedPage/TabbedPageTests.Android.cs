using System.Threading.Tasks;
using AndroidX.ViewPager2.Widget;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class TabbedPageTests
	{
		[Fact(DisplayName = "Using SelectedTab Color doesnt crash")]
		public async Task SelectedTabColorNoDoesntCrash()
		{
			SetupBuilder();

			var tabbedPage = CreateBasicTabbedPage();
			tabbedPage.SelectedTabColor = Colors.Red;

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(tabbedPage), (handler) =>
			{
				var platformView = tabbedPage.Handler.PlatformView as ViewPager2;
				Assert.NotNull(platformView);
				return Task.CompletedTask;
			});
		}
	}
}