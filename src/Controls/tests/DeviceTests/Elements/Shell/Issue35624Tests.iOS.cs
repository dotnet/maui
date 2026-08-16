#nullable enable

using System;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	public class Issue35624 : ControlsHandlerTestBase
	{
		const string IssueNumber = "35624";

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
		public async Task SearchHandlerAppliesCharacterSpacing()
		{
			if (!string.Equals(
				GetReplicationIssue(),
				IssueNumber,
				StringComparison.Ordinal))
			{
				return;
			}

			EnsureHandlerCreated(builder => builder.SetupShellHandlers());

			const double expected = 8;
			var page = new ContentPage();
			var searchHandler = new SearchHandler
			{
				CharacterSpacing = expected,
				Query = "SPACING",
				SearchBoxVisibility = SearchBoxVisibility.Expanded
			};
			Shell.SetSearchHandler(page, searchHandler);

			var shell = new Shell
			{
				FlyoutBehavior = FlyoutBehavior.Disabled,
				CurrentItem = new ShellContent
				{
					Content = page
				}
			};

			double actual = 0;
			await CreateHandlerAndAddToWindow<ShellRenderer>(shell, async handler =>
			{
				await OnNavigatedToAsync(page);

				var shellContext = (IShellContext)handler;
				var itemRenderer = (ShellItemRenderer)shellContext.CurrentShellItemRenderer;
				var sectionRenderer = (ShellSectionRenderer)itemRenderer.CurrentRenderer;
				var rootRenderer = (ShellSectionRootRenderer)sectionRenderer.ViewControllers[0];
				UISearchBar searchBar = rootRenderer.NavigationItem.SearchController.SearchBar;
				UITextField textField = searchBar.FindDescendantView<UITextField>();
				actual = textField.AttributedText.GetCharacterSpacing();
			});

			Assert.True(
				actual == expected,
				$"SearchHandler CharacterSpacing expected {expected:0} but was {actual:0}.");
		}
	}
}
