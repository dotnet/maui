using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	public class Issue35516 : ControlsHandlerTestBase
	{
		[Fact]
		public async Task SettingQueryUpdatesNativeSearchFieldText()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers => SetupShellHandlers(handlers));
			});

			const string query = "Hello World";
			var searchHandler = new SearchHandler
			{
				SearchBoxVisibility = SearchBoxVisibility.Expanded
			};
			var page = new ContentPage();
			Shell.SetSearchHandler(page, searchHandler);

			var shell = new Shell
			{
				FlyoutBehavior = FlyoutBehavior.Disabled,
				Items =
				{
					new ShellContent
					{
						Content = page
					}
				}
			};

			await CreateHandlerAndAddToWindow<ShellRenderer>(shell, async handler =>
			{
				await OnNavigatedToAsync(page);

				UISearchBar nativeSearchBar = null;
				await AssertEventually(
					() =>
					{
						nativeSearchBar = handler.View?.FindDescendantView<UISearchBar>();
						return nativeSearchBar is not null;
					},
					message: "The native Shell search field should be available.");

				searchHandler.Query = query;

				Assert.True(
					nativeSearchBar.Text == query,
					"SearchHandler.Query should update the native search field text.");
			});
		}
	}
}
