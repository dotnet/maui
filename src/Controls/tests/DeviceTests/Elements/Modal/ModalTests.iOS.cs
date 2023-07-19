using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ModalTests : ControlsHandlerTestBase
	{
		[Theory]
		[ClassData(typeof(PageTypes))]
		public async Task PushModalUsingTransparencies(Page rootPage, Page modalPage)
		{
			SetupBuilder();

			var expected = Colors.Red;

			rootPage.BackgroundColor = expected;
			modalPage.BackgroundColor = Colors.Transparent;

			await CreateHandlerAndAddToWindow<IWindowHandler>(rootPage,
				async (handler) =>
				{
					var currentPage = rootPage.GetCurrentPage();
					await currentPage.Navigation.PushModalAsync(modalPage);
					await OnLoadedAsync(modalPage);
					Assert.Single(currentPage.Navigation.ModalStack);

					var rootView = handler.PlatformView;
					Assert.NotNull(rootView);

					var currentView = currentPage.Handler.PlatformView as UIView;
					Assert.NotNull(currentView);
					Assert.NotNull(currentView.Window);
				});
		}
	}
}
