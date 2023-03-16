using System.Threading.Tasks;
using AndroidX.ViewPager2.Widget;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Hosting;
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

		[Fact(DisplayName = "Custom RecyclerView Adapter Doesn't Crash")]
		public async Task CustomRecyclerViewAdapterDoesNotCrash()
		{
			SetupBuilder(builder =>
			{
				builder.ConfigureMauiHandlers(handler =>
				{
					handler.AddHandler<TabbedPage, CustomTestAdapterHandler>();
				});
			});

			var tabbedPage = CreateBasicTabbedPage();

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(tabbedPage), async (handler) =>
			{
				// If you currently try to modify the children too early.
				// This will sometimes cause `NotifyDataSourceChanged` on the
				// adapter to get called while it's already processing
				await Task.Delay(50);

				tabbedPage.Children.Add(new ContentPage());

				// make sure changes have time to propagate
				await Task.Delay(50);
			});
		}
	}
}