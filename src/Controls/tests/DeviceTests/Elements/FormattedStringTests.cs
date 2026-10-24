using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Label)]
	public partial class FormattedStringTests : ControlsHandlerTestBase
	{
		[Fact]
		public async Task NativeFormattedStringContainsSpan()
		{
			var formattedString = new FormattedString();
			formattedString.Spans.Add(new Span { Text = "HELLO" });
			formattedString.Spans.Add(new Span { Text = "WORLD" });

			var label = new Label();
			var handler = await CreateHandlerAsync<LabelHandler>(label);

			var fontManager = handler.Services.GetRequiredService<IFontManager>();

			var plainText = formattedString.ToString();
			var actual = await InvokeOnMainThreadAsync(() =>
			{
				var result = string.Empty;
#if __IOS__
				var attributed = formattedString.ToNSAttributedString(fontManager);

				attributed.EnumerateAttributes(new Foundation.NSRange(0, attributed.Length),
					 Foundation.NSAttributedStringEnumeration.None, new Foundation.NSAttributedRangeCallback((Foundation.NSDictionary dict, Foundation.NSRange range, ref bool flag) =>
					 {
						 result += attributed.Substring(range.Location, range.Length)?.Value;
					 }));
#elif __ANDROID__
				var spannable = formattedString.ToSpannableString(fontManager);

				result = spannable.ToString();
#elif WINDOWS
				var tuples = formattedString.ToRunAndColorsTuples(fontManager: fontManager);

				foreach (var t in tuples)
					result += t.Item1.Text;
#endif
				return result;
			});

			Assert.Equal("HELLOWORLD", actual);
		}

		// Covers https://github.com/dotnet/maui/issues/36519
		// Updating Label.FormattedText must clear any highlighter/background-color state applied
		// by the previous FormattedText. On Windows specifically, the native TextBlock's
		// TextHighlighters collection was never cleared, so stale highlighter ranges (from
		// BackgroundColor/TextColor spans) survived and bled onto the new text, leaking
		// TextHighlighter instances. This test verifies the rendered output on all platforms,
		// with an additional native-collection check on Windows.
		[Fact]
		public async Task UpdatingFormattedTextClearsPreviousHighlights()
		{
			var label = new Label
			{
				WidthRequest = 200,
				HeightRequest = 50,
				FontSize = 16,
				FormattedText = new FormattedString
				{
					Spans =
					{
						new Span { Text = "Highlighted", BackgroundColor = Colors.Red },
						new Span { Text = " text", TextColor = Colors.Blue },
					}
				},
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler<LabelHandler>(label);

#if WINDOWS
				// Sanity check: highlighters were created for the initial FormattedText
				Assert.NotEmpty(handler.PlatformView.TextHighlighters);
#endif
				await handler.PlatformView.AssertContainsColor(Colors.Red, MauiContext);

				// Update to a FormattedString with no highlighted spans at all
				label.FormattedText = new FormattedString
				{
					Spans =
					{
						new Span { Text = "Plain text with no highlights" },
					}
				};

#if WINDOWS
				Assert.Empty(handler.PlatformView.TextHighlighters);
#endif
				await handler.PlatformView.AssertDoesNotContainColor(Colors.Red, MauiContext);
			});
		}
	}
}