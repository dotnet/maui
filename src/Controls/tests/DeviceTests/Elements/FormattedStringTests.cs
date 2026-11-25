using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
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
		public static IEnumerable<object[]> TextDecorationCases()
		{
			foreach (var decorations in new[] { TextDecorations.None, TextDecorations.Underline, TextDecorations.Strikethrough, TextDecorations.Underline | TextDecorations.Strikethrough })
				foreach (var hasDecorationColor in new[] { false, true })
					foreach (var thickness in new[] { -1d, 0d, 0.25d, 1d, 1.5d, 2d })
						yield return new object[] { decorations, hasDecorationColor, thickness };
		}

		[Theory]
		[MemberData(nameof(TextDecorationCases))]
		public async Task TextDecorationsApplyNativeColorAndThickness(TextDecorations decorations, bool hasDecorationColor, double thickness)
		{
			var formattedString = new FormattedString();
			formattedString.Spans.Add(new Span { Text = "plain " });
			formattedString.Spans.Add(new Span
			{
				Text = "decorated",
				TextColor = Colors.Blue,
				TextDecorations = decorations,
				TextDecorationColor = hasDecorationColor ? Colors.Orange : null,
				TextDecorationThickness = thickness,
			});

			var label = new Label();
			var handler = await CreateHandlerAsync<LabelHandler>(label);
			var fontManager = handler.Services.GetRequiredService<IFontManager>();

			await InvokeOnMainThreadAsync(() =>
			{
#if __IOS__
				var attributed = formattedString.ToNSAttributedString(fontManager);
				var start = (nint)"plain ".Length;
				foreach (var (decoration, colorKey, styleKey) in new[]
				{
					(TextDecorations.Underline, UIKit.UIStringAttributeKey.UnderlineColor, UIKit.UIStringAttributeKey.UnderlineStyle),
					(TextDecorations.Strikethrough, UIKit.UIStringAttributeKey.StrikethroughColor, UIKit.UIStringAttributeKey.StrikethroughStyle),
				})
				{
					var color = attributed.GetAttribute(colorKey, start, out _) as UIKit.UIColor;
					var style = attributed.GetAttribute(styleKey, start, out _) as Foundation.NSNumber;
					var expectedStyle = decorations.HasFlag(decoration)
						? (thickness >= 1.5 ? Foundation.NSUnderlineStyle.Thick : Foundation.NSUnderlineStyle.Single)
						: Foundation.NSUnderlineStyle.None;

					Assert.Equal((long)expectedStyle, style?.Int64Value ?? 0);
					if (hasDecorationColor && decorations.HasFlag(decoration))
					{
						Assert.NotNull(color);
						color.GetRGBA(out var r, out var g, out var b, out var a);
						Assert.Equal(Colors.Orange, new Color((float)r, (float)g, (float)b, (float)a));
					}
					else
					{
						Assert.Null(color);
					}

					Assert.Null(attributed.GetAttribute(colorKey, 0, out _));
				}

				var foreground = attributed.GetAttribute(UIKit.UIStringAttributeKey.ForegroundColor, start, out _) as UIKit.UIColor;
				Assert.Equal(Colors.Blue.ToPlatform(), foreground);
#elif __ANDROID__
				var context = handler.MauiContext.Context;
				using var spannable = formattedString.ToSpannableString(fontManager, context);
				var start = "plain ".Length;
				var end = spannable.Length();
				var custom = spannable.GetSpans(start, end, Java.Lang.Class.FromType(typeof(PlatformUnderlineSpan)));
				var standard = spannable.GetSpans(start, end, Java.Lang.Class.FromType(typeof(global::Android.Text.Style.UnderlineSpan)));
				var strikethrough = spannable.GetSpans(start, end, Java.Lang.Class.FromType(typeof(global::Android.Text.Style.StrikethroughSpan)));
				var hasUnderline = decorations.HasFlag(TextDecorations.Underline);
				var usesCustomUnderline = hasUnderline && (hasDecorationColor || thickness > 0);

				Assert.Equal(usesCustomUnderline ? 1 : 0, custom.Length);
				Assert.Equal(hasUnderline && !usesCustomUnderline ? 1 : 0, standard.Length);
				Assert.Equal(decorations.HasFlag(TextDecorations.Strikethrough) ? 1 : 0, strikethrough.Length);

				using var paint = new global::Android.Text.TextPaint
				{
					Color = Colors.Blue.ToPlatform(),
					TextSize = 20,
				};
				if (usesCustomUnderline)
				{
					// Start with a standard underline so API 29+ must explicitly clear its flag.
					paint.UnderlineText = global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.Q;
					Assert.IsType<PlatformUnderlineSpan>(Assert.Single(custom)).UpdateDrawState(paint);
				}
				else if (hasUnderline)
				{
					Assert.IsType<global::Android.Text.Style.UnderlineSpan>(Assert.Single(standard)).UpdateDrawState(paint);
				}

				Assert.Equal(Colors.Blue.ToPlatform(), paint.Color);
				if (usesCustomUnderline && global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.Q)
				{
					Assert.False(paint.UnderlineText);
					Assert.Equal((hasDecorationColor ? Colors.Orange : Colors.Blue).ToPlatform().ToArgb(), paint.UnderlineColor);
					// The Java field is not bound because Paint already exposes the font's UnderlineThickness property.
					using var thicknessField = paint.Class.GetField("underlineThickness");
					var actualThickness = thicknessField.GetFloat(paint);
					var expectedThickness = thickness > 0 ? (float)(thickness * context.Resources.DisplayMetrics.Density) : System.Math.Max(paint.UnderlineThickness, 1f);
					Assert.True(actualThickness > 0, $"Expected positive underline thickness, got {actualThickness}; font thickness: {paint.UnderlineThickness}.");
					Assert.Equal(expectedThickness, actualThickness, 5);
				}
				else
				{
					Assert.Equal(hasUnderline, paint.UnderlineText);
					if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.Q)
						Assert.Equal(0, paint.UnderlineColor);
				}
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