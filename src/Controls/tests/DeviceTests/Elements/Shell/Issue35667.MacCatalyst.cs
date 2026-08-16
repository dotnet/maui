#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Hosting;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	public class Issue35667 : ControlsHandlerTestBase
	{
		const string IssueNumber = "35667";

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
		public async Task SearchHandlerTransformsEnteredTextToUppercase()
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
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationRenderer));
				});
			});

			var page = new ContentPage();
			var searchHandler = new SearchHandler
			{
				SearchBoxVisibility = SearchBoxVisibility.Expanded,
				TextTransform = TextTransform.Uppercase
			};
			Shell.SetSearchHandler(page, searchHandler);

			var shell = new Shell
			{
				FlyoutBehavior = FlyoutBehavior.Disabled,
				Items =
				{
					new ShellContent { Content = page }
				}
			};

			await CreateHandlerAndAddToWindow<ShellRenderer>(shell, async handler =>
			{
				await OnLoadedAsync(page);

				IShellContext shellContext = handler;
				var shellItemRenderer = Assert.IsType<ShellItemRenderer>(shellContext.CurrentShellItemRenderer);
				var sectionRenderer = Assert.IsType<ShellSectionRenderer>(shellItemRenderer.CurrentRenderer);
				var searchController = sectionRenderer.TopViewController.NavigationItem.SearchController;
				Assert.NotNull(searchController);

				UISearchBar searchBar = searchController.SearchBar;
				const string enteredText = "maui";
				searchBar.Text = enteredText;
				var searchBarDelegate = searchBar.Delegate;
				Assert.NotNull(searchBarDelegate);
				searchBarDelegate.TextChanged(searchBar, enteredText);

				Assert.True(
					string.Equals("MAUI", searchBar.Text, StringComparison.Ordinal),
					"SearchHandler text should be transformed to uppercase.");
			});
		}
	}
}
