using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Map)]
	public partial class MapTests : ControlsHandlerTestBase
	{
		// Regression test for https://github.com/dotnet/maui/issues/37096
		[Fact]
		public async Task PoppingModalPageWithMapDoesNotCrash()
		{
			var rootPage = new ContentPage();
			var mapPage = new ContentPage
			{
				Content = new Map { IsEnabled = false }
			};

			await CreateHandlerAndAddToWindow<IWindowHandler>(new Window(new NavigationPage(rootPage)), async _ =>
			{
				await rootPage.Navigation.PushModalAsync(mapPage);
				await OnLoadedAsync(mapPage);

				await rootPage.Navigation.PopModalAsync();
				await OnUnloadedAsync(mapPage);
			});
		}
	}
}
