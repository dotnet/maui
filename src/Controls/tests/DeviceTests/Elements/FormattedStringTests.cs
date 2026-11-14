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

#if __IOS__ || __ANDROID__
		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task TextDecorationColorColorsTheUnderline(bool hasDecorationColor)
		{
			var formattedString = new FormattedString();
			formattedString.Spans.Add(new Span { Text = "plain " });
			formattedString.Spans.Add(new Span
			{
				Text = "underlined",
				TextDecorations = TextDecorations.Underline,
				TextDecorationColor = hasDecorationColor ? Colors.Orange : null,
			});

			var label = new Label();
			var handler = await CreateHandlerAsync<LabelHandler>(label);
			var fontManager = handler.Services.GetRequiredService<IFontManager>();

			await InvokeOnMainThreadAsync(() =>
			{
#if __IOS__
				var attributed = formattedString.ToNSAttributedString(fontManager);
				var start = (nint)"plain ".Length;
				var underlineColor = attributed.GetAttribute(UIKit.UIStringAttributeKey.UnderlineColor, start, out _) as UIKit.UIColor;
				var underlineStyle = attributed.GetAttribute(UIKit.UIStringAttributeKey.UnderlineStyle, start, out _) as Foundation.NSNumber;

				Assert.NotNull(underlineStyle);
				if (hasDecorationColor)
				{
					Assert.NotNull(underlineColor);
					underlineColor.GetRGBA(out var r, out var g, out var b, out var a);
					Assert.Equal(Colors.Orange, new Color((float)r, (float)g, (float)b, (float)a));
				}
				else
				{
					Assert.Null(underlineColor);
				}

				// The undecorated span gets no decoration color.
				Assert.Null(attributed.GetAttribute(UIKit.UIStringAttributeKey.UnderlineColor, 0, out _));
#elif __ANDROID__
				var spannable = formattedString.ToSpannableString(fontManager);
				var start = "plain ".Length;
				var end = spannable.Length();
				var colored = spannable.GetSpans(start, end, Java.Lang.Class.FromType(typeof(PlatformUnderlineSpan)));
				var plain = spannable.GetSpans(start, end, Java.Lang.Class.FromType(typeof(global::Android.Text.Style.UnderlineSpan)));

				Assert.Equal(hasDecorationColor ? 1 : 0, colored?.Length ?? 0);
				Assert.Equal(hasDecorationColor ? 0 : 1, plain?.Length ?? 0);
#endif
			});
		}
#endif

#if __IOS__ || __ANDROID__
		[Theory]
		[InlineData(-1d)]
		[InlineData(1d)]
		[InlineData(2d)]
		public async Task TextDecorationThicknessSetsTheUnderlineThickness(double thickness)
		{
			var formattedString = new FormattedString();
			formattedString.Spans.Add(new Span
			{
				Text = "underlined",
				TextDecorations = TextDecorations.Underline,
				TextDecorationThickness = thickness,
			});

			var label = new Label();
			var handler = await CreateHandlerAsync<LabelHandler>(label);
			var fontManager = handler.Services.GetRequiredService<IFontManager>();

			await InvokeOnMainThreadAsync(() =>
			{
#if __IOS__
				// UIKit only has styles: Thick from 1.5 up, Single below (and by default).
				var attributed = formattedString.ToNSAttributedString(fontManager);
				var underlineStyle = attributed.GetAttribute(UIKit.UIStringAttributeKey.UnderlineStyle, 0, out _) as Foundation.NSNumber;

				Assert.NotNull(underlineStyle);
				Assert.Equal((long)(thickness >= 1.5 ? Foundation.NSUnderlineStyle.Thick : Foundation.NSUnderlineStyle.Single), underlineStyle.Int64Value);
#elif __ANDROID__
				// A thickness (even without a color) uses the platform span; the default keeps UnderlineSpan.
				var spannable = formattedString.ToSpannableString(fontManager);
				var custom = spannable.GetSpans(0, spannable.Length(), Java.Lang.Class.FromType(typeof(PlatformUnderlineSpan)));
				var plain = spannable.GetSpans(0, spannable.Length(), Java.Lang.Class.FromType(typeof(global::Android.Text.Style.UnderlineSpan)));

				Assert.Equal(thickness > 0 ? 1 : 0, custom?.Length ?? 0);
				Assert.Equal(thickness > 0 ? 0 : 1, plain?.Length ?? 0);
#endif
			});
		}
#endif

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