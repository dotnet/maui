#nullable enable

using System;
using System.Threading.Tasks;
using AndroidX.Core.View;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Layout)]
	public class Issue37418 : ControlsHandlerTestBase
	{
		const string IssueNumber = "37418";

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
		public async Task ResettingOffscreenTranslationDoesNotAddTopGap()
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
					handlers.AddHandler<Page, PageHandler>();
					handlers.AddHandler<Window, WindowHandlerStub>();
					handlers.AddHandler<Layout, LayoutHandler>();
					handlers.AddHandler<BoxView, BoxViewHandler>();
				});
			});

			var firstChild = new BoxView
			{
				HeightRequest = 40,
				VerticalOptions = LayoutOptions.Start
			};
			var bottomSheet = new Grid
			{
				HeightRequest = 320,
				Padding = 0,
				VerticalOptions = LayoutOptions.End,
				TranslationY = 2000,
				SafeAreaEdges = SafeAreaEdges.Container,
				Children = { firstChild }
			};
			var root = new Grid
			{
				Children = { bottomSheet }
			};
			var page = new ContentPage
			{
				SafeAreaEdges = SafeAreaEdges.None,
				Content = root
			};

			await CreateHandlerAndAddToWindow(page, async () =>
			{
				var platformSheet = bottomSheet.ToPlatform();
				await platformSheet.WaitForLayoutOrNonZeroSize();

				var windowInsets = ViewCompat.GetRootWindowInsets(platformSheet);
				Assert.NotNull(windowInsets);
				((IHandleWindowInsets)platformSheet).HandleWindowInsets(platformSheet, windowInsets);

				await InvokeOnMainThreadAsync(() => bottomSheet.TranslationY = 0);

				var nativeUpdate = new TaskCompletionSource();
				platformSheet.Post(nativeUpdate.SetResult);
				await nativeUpdate.Task;

				Assert.True(
					platformSheet.PaddingTop == 0,
					"The bottom sheet must not retain a top gap after TranslationY is reset to zero.");
			});
		}
	}
}
