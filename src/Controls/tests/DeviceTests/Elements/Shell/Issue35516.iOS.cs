#nullable enable

using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	public class Issue35516 : ControlsHandlerTestBase
	{
		const string IssueNumber = "35516";

		static string? GetReplicationIssue()
		{
#if ANDROID
			return global::Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner
				.MauiTestInstrumentation.Current?.Arguments?.GetString("MAUI_REPRODUCTION_ISSUE");
#elif IOS || MACCATALYST
			return global::Foundation.NSProcessInfo.ProcessInfo.Environment["MAUI_REPRODUCTION_ISSUE"]?.ToString();
#else
			return Environment.GetEnvironmentVariable("MAUI_REPRODUCTION_ISSUE");
#endif
		}

		[Fact]
		public async Task UpdatingQueryUpdatesNativeSearchBoxText()
		{
			if (!string.Equals(
				GetReplicationIssue(),
				IssueNumber,
				StringComparison.Ordinal))
			{
				return;
			}

			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					SetupShellHandlers(handlers);
				});
			});

			var page = new ContentPage { Title = "Search Query Test" };
			var searchHandler = new SearchHandler
			{
				AutomationId = "Issue35516SearchHandler",
				SearchBoxVisibility = SearchBoxVisibility.Expanded
			};
			Shell.SetSearchHandler(page, searchHandler);

			var shell = new Shell
			{
				FlyoutBehavior = FlyoutBehavior.Disabled,
				Items =
				{
					new ShellContent
					{
						Content = page,
						Title = "Search Query Test"
					}
				}
			};

			await CreateHandlerAndAddToWindow<ShellRenderer>(shell, async handler =>
			{
				await OnLoadedAsync(page);
				await OnNavigatedToAsync(page);

				var pageHandler = Assert.IsAssignableFrom<IPlatformViewHandler>(page.Handler);
				var pagerParent = pageHandler.PlatformView
					.FindParent(view => view.NextResponder is UITabBarController);
				Assert.NotNull(pagerParent);

				var tabController = Assert.IsType<ShellItemRenderer>(pagerParent.NextResponder);
				var section = Assert.IsType<ShellSectionRenderer>(tabController.SelectedViewController);
				var root = Assert.IsType<ShellSectionRootRenderer>(section.ViewControllers[0]);
				var searchController = root.NavigationItem.SearchController;
				Assert.NotNull(searchController);

				searchHandler.Query = "Hello World";

				Assert.True(
					string.Equals(searchController.SearchBar.Text, "Hello World", StringComparison.Ordinal),
					"Issue35516: native search box text should update to 'Hello World'.");
			});
		}
	}
}
