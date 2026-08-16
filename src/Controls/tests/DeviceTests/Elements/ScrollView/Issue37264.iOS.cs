#nullable enable annotations

using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.ScrollView)]
	public class Issue37264 : ControlsHandlerTestBase
	{
		const string IssueNumber = "37264";

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
		public async Task SoftInputDoesNotHorizontallyInsetContent()
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
					handlers.AddHandler<Label, LabelHandler>();
					handlers.AddHandler<ScrollView, ScrollViewHandler>();
					handlers.AddHandler<VerticalStackLayout, LayoutHandler>();
					handlers.AddHandler<Grid, LayoutHandler>();
				});
			});

			var topLabel = new Label { Text = "Top", HorizontalOptions = LayoutOptions.Fill };
			var bottomLabel = new Label { Text = "Bottom", HorizontalOptions = LayoutOptions.Fill };
			var content = new VerticalStackLayout
			{
				topLabel,
				new Label { Text = "ScrollView content", HeightRequest = 180 },
				bottomLabel
			};
			var scrollView = new ScrollView
			{
				SafeAreaEdges = SafeAreaEdges.None,
				Content = content
			};
			var page = new ContentPage
			{
				SafeAreaEdges = SafeAreaEdges.None,
				Content = new Grid
				{
					SafeAreaEdges = SafeAreaEdges.None,
					Children = { scrollView }
				}
			};

			await CreateHandlerAndAddToWindow(page, () =>
			{
				var viewController = ((IPlatformViewHandler)page.Handler).ViewController;
				viewController.AdditionalSafeAreaInsets = new UIEdgeInsets(0, 20, 0, 20);
				viewController.View.SetNeedsLayout();
				viewController.View.LayoutIfNeeded();

				scrollView.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.SoftInput);
				scrollView.InvalidateMeasure();
				viewController.View.SetNeedsLayout();
				viewController.View.LayoutIfNeeded();

				bool contentIsFullWidth =
					Math.Abs(topLabel.Width - scrollView.Width) <= 1 &&
					Math.Abs(bottomLabel.Width - scrollView.Width) <= 1;

				Assert.True(contentIsFullWidth,
					"SoftInput safe area must not horizontally inset ScrollView content.");
			});
		}
	}
}
