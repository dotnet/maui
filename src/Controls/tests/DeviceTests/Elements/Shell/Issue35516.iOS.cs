using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Hosting;
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
		public async Task SettingQueryUpdatesNativeSearchBar()
		{
			if (!string.Equals(
				GetReplicationIssue(),
				IssueNumber,
				StringComparison.Ordinal))
			{
				return;
			}

#if !MACCATALYST
			return;
#else
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers => SetupShellHandlers(handlers));
			});

			var searchHandler = new SearchHandler
			{
				Placeholder = "Search query",
				SearchBoxVisibility = SearchBoxVisibility.Expanded
			};
			var page = new ContentPage();
			Shell.SetSearchHandler(page, searchHandler);

			var shell = new Shell
			{
				CurrentItem = page,
				FlyoutBehavior = FlyoutBehavior.Disabled
			};

			await CreateHandlerAndAddToWindow<ShellRenderer>(shell, async handler =>
			{
				await OnLoadedAsync(page);

				var shellContext = (IShellContext)handler;
				var itemRenderer = Assert.IsType<ShellItemRenderer>(shellContext.CurrentShellItemRenderer);
				var sectionRenderer = Assert.IsType<ShellSectionRenderer>(itemRenderer.CurrentRenderer);
				var nativeSearchBar = sectionRenderer.TopViewController?.NavigationItem.SearchController?.SearchBar;
				Assert.NotNull(nativeSearchBar);

				searchHandler.Query = "Hello World";

				Assert.True(
					string.Equals("Hello World", nativeSearchBar.Text, StringComparison.Ordinal),
					"Native SearchHandler text should update to 'Hello World'.");
			});
#endif
		}
	}
}
