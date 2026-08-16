#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Platform;
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
		public async Task UppercaseTextTransformAppliesToEnteredText()
		{
			if (!string.Equals(
				GetReplicationIssue(),
				IssueNumber,
				StringComparison.Ordinal))
			{
				return;
			}

			EnsureHandlerCreated(builder => builder.SetupShellHandlers());

			var contentPage = new ContentPage();
			var searchHandler = new SearchHandler
			{
				SearchBoxVisibility = SearchBoxVisibility.Expanded,
				TextTransform = TextTransform.Uppercase
			};
			Shell.SetSearchHandler(contentPage, searchHandler);

			var shell = new Shell
			{
				Items =
				{
					new ShellContent
					{
						Content = contentPage,
						Title = "Search"
					}
				}
			};

			await CreateHandlerAndAddToWindow<ShellRenderer>(shell, async handler =>
			{
				await OnLoadedAsync(contentPage);
				await OnNavigatedToAsync(contentPage);

				var searchBar = handler.ViewController.View.FindDescendantView<UISearchBar>();
				Assert.NotNull(searchBar);

				var textField = searchBar.FindDescendantView<UITextField>();
				Assert.NotNull(textField);

				textField.BecomeFirstResponder();
				textField.InsertText("Maui");

				Assert.True(
					string.Equals("MAUI", textField.Text, StringComparison.Ordinal),
					$"SearchHandler should display 'MAUI' when TextTransform is Uppercase, but displayed '{textField.Text}'.");
			});
		}
	}
}
