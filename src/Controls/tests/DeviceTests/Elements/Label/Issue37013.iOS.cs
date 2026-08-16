#nullable enable

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Label)]
	public class Issue37013 : ControlsHandlerTestBase
	{
		const string IssueNumber = "37013";

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

		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Label, LabelHandler>();
				});
			});
		}

		[Fact]
		public async Task VisibleSpanCenterResolvesToTapGesture()
		{
			if (!string.Equals(
				GetReplicationIssue(),
				IssueNumber,
				StringComparison.Ordinal))
			{
				return;
			}

			SetupBuilder();

			var tapGesture = new TapGestureRecognizer();
			var linkSpan = new Span
			{
				Text = "TAP THIS VISIBLE LINK",
				TextColor = Colors.Blue,
				TextDecorations = TextDecorations.Underline,
				GestureRecognizers = { tapGesture }
			};

			var label = new Label
			{
				FontSize = 16,
				HorizontalOptions = LayoutOptions.Center,
				LineHeight = 1.1,
				WidthRequest = 300,
				FormattedText = new FormattedString
				{
					Spans =
					{
						new Span
						{
							Text = "This is the first naturally wrapping paragraph used to create many rendered lines at a constrained width. The repeated prose keeps the layout deterministic while allowing the native label to choose every line break. Additional words extend the paragraph so vertical metric differences accumulate before the interactive span. This section continues with ordinary sentence text and enough content to occupy several more wrapped lines before the visible link appears.\n\nA second paragraph adds more naturally wrapped content before the link. It deliberately avoids manual line breaks within the paragraph because the reproduction depends on native wrapping metrics. More words complete this balanced section near the middle of the label.\n\n"
						},
						linkSpan,
						new Span
						{
							Text = "\n\nA matching paragraph follows the visible link and preserves its position near the center of the complete label. It also uses naturally wrapped prose so the rendered text height remains governed by the native text engine. More words complete this balanced section after the interactive span.\n\nThis final naturally wrapping paragraph supplies several additional lines below the link. The repeated prose keeps the layout deterministic while allowing the native label to choose every line break. Additional words extend the paragraph to balance the content above the link. This section continues with ordinary sentence text and enough content to occupy several more wrapped lines after the visible link."
						}
					}
				}
			};

			var labelHandler = await CreateHandlerAsync<LabelHandler>(label);
			bool resolvesToTapGesture = false;

			await CreateHandlerAndAddToWindow(label, () =>
			{
				var platformLabel = (UILabel)labelHandler.PlatformView;
				var visibleCenter = new Point(
					platformLabel.Bounds.Width / 2,
					platformLabel.Bounds.Height / 2);

				resolvesToTapGesture = label.GetChildElements(visibleCenter)?
					.Any(element => element.GestureRecognizers.Contains(tapGesture)) == true;
			});

			Assert.True(
				resolvesToTapGesture,
				"The visible span center must resolve to the span tap gesture.");
		}
	}
}
