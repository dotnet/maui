#nullable enable

using System;
using System.Threading.Tasks;
using CoreGraphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.ContentView)]
	public class Issue19340 : ControlsHandlerTestBase
	{
		const string IssueNumber = "19340";

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
		public async Task EmbeddedContentViewHasPositiveFittedHeight()
		{
			if (!string.Equals(
				GetReplicationIssue(),
				IssueNumber,
				StringComparison.Ordinal))
			{
				return;
			}

			var heights = await InvokeOnMainThreadAsync(() =>
			{
				var button = new Button
				{
					Text = "Reference Button"
				};
				var label = new Label
				{
					Text = "ContentView child"
				};
				var entry = new Entry
				{
					Placeholder = "Embedded entry"
				};
				var layout = new VerticalStackLayout
				{
					Children =
					{
						label,
						entry
					}
				};
				var contentView = new ContentView
				{
					Content = layout
				};

				CreateHandler<LabelHandler>(label);
				CreateHandler<EntryHandler>(entry);
				CreateHandler<LayoutHandler>(layout);
				var buttonHandler = CreateHandler<ButtonHandler>(button);
				var contentViewHandler = CreateHandler<ContentViewHandler>(contentView);

				var nativeStack = new UIStackView(new UIView[]
				{
					buttonHandler.PlatformView,
					contentViewHandler.PlatformView
				})
				{
					Axis = UILayoutConstraintAxis.Vertical,
					Alignment = UIStackViewAlignment.Fill,
					Distribution = UIStackViewDistribution.Fill,
					Spacing = 8
				};

				var fittedSize = nativeStack.SystemLayoutSizeFittingSize(UIView.UILayoutFittingCompressedSize);
				nativeStack.Frame = new CGRect(0, 0, fittedSize.Width, fittedSize.Height);
				nativeStack.LayoutIfNeeded();

				return (
					Button: buttonHandler.PlatformView.Frame.Height,
					ContentView: contentViewHandler.PlatformView.Frame.Height);
			});

			Assert.True(heights.Button > 0, "Expected reference Button to have a positive height.");
			Assert.True(heights.ContentView > 0, "Expected embedded ContentView to have a positive height.");
		}
	}
}
