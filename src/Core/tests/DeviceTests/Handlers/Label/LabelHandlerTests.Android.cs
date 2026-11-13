using System;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;
using ATextAlignemnt = global::Android.Views.TextAlignment;
using Color = Microsoft.Maui.Graphics.Color;

namespace Microsoft.Maui.DeviceTests
{
	public partial class LabelHandlerTests
	{
		[Fact(DisplayName = "Horizontal TextAlignment Initializes Correctly")]
		public async Task HorizontalTextAlignmentInitializesCorrectly()
		{
			var xplatHorizontalTextAlignment = TextAlignment.End;

			var labelStub = new LabelStub()
			{
				Text = "Test",
				HorizontalTextAlignment = xplatHorizontalTextAlignment
			};

			var values = await GetValueAsync(labelStub, (handler) =>
			{
				return new
				{
					ViewValue = labelStub.HorizontalTextAlignment,
					PlatformViewValue = GetNativeHorizontalTextAlignment(handler)
				};
			});

			Assert.Equal(xplatHorizontalTextAlignment, values.ViewValue);

			(var gravity, var textAlignment) = values.PlatformViewValue;

			// Device Tests runner has RTL support enabled, so we expect TextAlignment values
			// (If it didn't, we'd have to fall back to gravity)
			var expectedValue = ATextAlignemnt.ViewEnd;

			Assert.Equal(expectedValue, textAlignment);
		}

		[Fact(DisplayName = "Padding Initializes Correctly")]
		public async Task PaddingInitializesCorrectly()
		{
			var label = new LabelStub()
			{
				Text = "Test",
				Padding = new Thickness(5, 10, 15, 20)
			};

			var handler = await CreateHandlerAsync(label);
			var (left, top, right, bottom) = GetNativePadding((TextView)handler.PlatformView);

			var context = handler.PlatformView.Context;

			var expectedLeft = context.ToPixels(5);
			var expectedTop = context.ToPixels(10);
			var expectedRight = context.ToPixels(15);
			var expectedBottom = context.ToPixels(20);

			Assert.Equal(expectedLeft, left);
			Assert.Equal(expectedTop, top);
			Assert.Equal(expectedRight, right);
			Assert.Equal(expectedBottom, bottom);
		}

		[Fact(DisplayName = "TextDecorations Initializes Correctly")]
		public async Task TextDecorationsInitializesCorrectly()
		{
			var xplatTextDecorations = TextDecorations.Underline;

			var labelHandler = new LabelStub()
			{
				TextDecorations = xplatTextDecorations
			};

			var values = await GetValueAsync(labelHandler, (handler) =>
			{
				return new
				{
					ViewValue = labelHandler.TextDecorations,
					PlatformViewValue = GetNativeTextDecorations(handler)
				};
			});

			PaintFlags expectedValue = PaintFlags.UnderlineText;

			Assert.Equal(xplatTextDecorations, values.ViewValue);
			values.PlatformViewValue.AssertHasFlag(expectedValue);
		}

		[Fact(DisplayName = "LineHeight Spaces All Lines Evenly")]
		public async Task LineHeightSpacesAllLinesEvenly()
		{
			var label = new LabelStub()
			{
				Text = "First\nSecond\nThird",
				Font = Font.SystemFontOfSize(30),
				LineHeight = 3
			};

			var (expectedGap, firstGap, secondGap) = await GetValueAsync(label, handler => GetBaselineGaps(handler));

			// The first line also holds the font's top padding, which must not make the
			// space below it larger than the space below the other lines.
			Assert.InRange(firstGap, expectedGap - 1, expectedGap + 1);
			Assert.InRange(secondGap, expectedGap - 1, expectedGap + 1);
		}

		[Fact(DisplayName = "LineHeight Spaces Wrapped Lines Evenly")]
		public async Task LineHeightSpacesWrappedLinesEvenly()
		{
			// One paragraph that wraps, unlike the newline-separated lines above.
			var label = new LabelStub()
			{
				Text = "First line second line third line fourth line",
				Font = Font.SystemFontOfSize(30),
				LineHeight = 2
			};

			// Narrow enough to wrap, wide enough to break only at the spaces
			var (expectedGap, firstGap, secondGap) = await GetValueAsync(label, handler => GetBaselineGaps(handler, widthPixels: 500));

			Assert.InRange(firstGap, expectedGap - 1, expectedGap + 1);
			Assert.InRange(secondGap, expectedGap - 1, expectedGap + 1);
		}

		[Fact(DisplayName = "LineHeight Follows Font Size Changes")]
		public async Task LineHeightFollowsFontSizeChanges()
		{
			var label = new LabelStub()
			{
				Text = "First\nSecond\nThird",
				Font = Font.SystemFontOfSize(10),
				LineHeight = 3
			};

			var handler = await CreateHandlerAsync(label);

			var (expectedGap, _, secondGap) = await InvokeOnMainThreadAsync(() =>
			{
				label.Font = Font.SystemFontOfSize(30);
				handler.UpdateValue(nameof(ITextStyle.Font));
				return GetBaselineGaps(handler);
			});

			Assert.InRange(secondGap, expectedGap - 1, expectedGap + 1);
		}

		[Fact(DisplayName = "LineHeight Follows Native Text Size Changes")]
		public async Task LineHeightFollowsNativeTextSizeChanges()
		{
			var label = new LabelStub()
			{
				Text = "First\nSecond\nThird",
				Font = Font.SystemFontOfSize(10),
				LineHeight = 3
			};

			var handler = await CreateHandlerAsync(label);

			// Platform customizations, such as a Font mapping appended by the app,
			// can change the native text size after MAUI has mapped LineHeight.
			var (expectedGap, _, secondGap) = await InvokeOnMainThreadAsync(() =>
			{
				var textView = GetPlatformLabel(handler);
				textView.SetTextSize(global::Android.Util.ComplexUnitType.Px, textView.TextSize * 3);
				return GetBaselineGaps(handler);
			});

			Assert.InRange(secondGap, expectedGap - 1, expectedGap + 1);
		}

		[Fact(DisplayName = "LineHeight Keeps Existing Transformation Method")]
		public async Task LineHeightKeepsExistingTransformationMethod()
		{
			var label = new LabelStub()
			{
				Text = "First\nSecond\nThird",
				Font = Font.SystemFontOfSize(30),
				LineHeight = 2
			};

			var handler = await CreateHandlerAsync(label);

			// A customized label can install its own transformation, such as all caps.
			var (text, expectedGap, firstGap, secondGap) = await InvokeOnMainThreadAsync(() =>
			{
				var textView = GetPlatformLabel(handler);
				textView.SetAllCaps(true);
				label.LineHeight = 3;
				handler.UpdateValue(nameof(ILabel.LineHeight));
				var (expected, first, second) = GetBaselineGaps(handler);
				return (textView.Layout.Text, expected, first, second);
			});

			Assert.Equal("FIRST\nSECOND\nTHIRD", text);
			Assert.InRange(firstGap, expectedGap - 1, expectedGap + 1);
			Assert.InRange(secondGap, expectedGap - 1, expectedGap + 1);
		}

		[Fact(DisplayName = "LineHeight Overrides Native Line Spacing")]
		public async Task LineHeightOverridesNativeLineSpacing()
		{
			var label = new LabelStub()
			{
				Text = "First\nSecond\nThird",
				Font = Font.SystemFontOfSize(30),
				LineHeight = 2
			};

			var handler = await CreateHandlerAsync(label);

			// Spacing set on the native view, for example by a custom handler, must not add to LineHeight.
			var (expectedGap, firstGap, secondGap) = await InvokeOnMainThreadAsync(() =>
			{
				GetPlatformLabel(handler).SetLineSpacing(10, 1.5f);
				label.LineHeight = 3;
				handler.UpdateValue(nameof(ILabel.LineHeight));
				return GetBaselineGaps(handler);
			});

			Assert.InRange(firstGap, expectedGap - 1, expectedGap + 1);
			Assert.InRange(secondGap, expectedGap - 1, expectedGap + 1);
		}

		[Fact(DisplayName = "LineHeight Keeps Line Spacing Set By An Appended Mapping")]
		public async Task LineHeightKeepsLineSpacingSetByAnAppendedMapping()
		{
			var label = new LabelStub()
			{
				Text = "First\nSecond\nThird",
				Font = Font.SystemFontOfSize(30),
				LineHeight = 2
			};

			// An app can customize the native line spacing in a mapping that runs after MAUI's.
			var mapper = new PropertyMapper<ILabel, LabelHandler>(LabelHandler.Mapper);
			mapper.AppendToMapping(nameof(ILabel.LineHeight), (handler, view) => handler.PlatformView.SetLineSpacing(20, 1));

			var (extra, multiplier) = await InvokeOnMainThreadAsync(() =>
			{
				var handler = new LabelHandler(mapper);
				InitializeViewHandler(label, handler);
				handler.GetDesiredSize(double.PositiveInfinity, double.PositiveInfinity);
				return (handler.PlatformView.LineSpacingExtra, handler.PlatformView.LineSpacingMultiplier);
			});

			Assert.Equal(20, extra);
			Assert.Equal(1, multiplier);
		}

		[Fact(DisplayName = "LineHeight Scales Html Lines By Their Own Height")]
		public async Task LineHeightScalesHtmlLinesByTheirOwnHeight()
		{
			// Html.FromHtml enlarges the heading with a size span, so its lines are
			// taller than the TextView's base font.
			const string html = "<h1>First<br>Second<br>Third</h1>";

			var (_, _, naturalGap) = await GetHtmlBaselineGaps(html, lineHeight: 1);
			var (_, _, gap) = await GetHtmlBaselineGaps(html, lineHeight: 2);

			// Spanned text keeps the line spacing multiplier, so only the gaps after the
			// first line are compared: they must double with the heading's own height.
			Assert.InRange(gap, 2 * naturalGap - 1, 2 * naturalGap + 1);
		}

		TextView GetPlatformLabel(LabelHandler labelHandler) =>
			labelHandler.PlatformView;

		string GetNativeText(LabelHandler labelHandler) =>
			GetPlatformLabel(labelHandler).Text;

		Color GetNativeTextColor(LabelHandler labelHandler) =>
			((uint)GetPlatformLabel(labelHandler).CurrentTextColor).ToColor();

		(GravityFlags gravity, ATextAlignemnt alignment) GetNativeHorizontalTextAlignment(LabelHandler labelHandler)
		{
			var textView = GetPlatformLabel(labelHandler);
			return (textView.Gravity, textView.TextAlignment);
		}

		(double left, double top, double right, double bottom) GetNativePadding(global::Android.Views.View view)
		{
			return (view.PaddingLeft, view.PaddingTop, view.PaddingRight, view.PaddingBottom);
		}

		double GetNativeCharacterSpacing(LabelHandler labelHandler) =>
			Math.Round(GetPlatformLabel(labelHandler).LetterSpacing / UnitExtensions.EmCoefficient, EmCoefficientPrecision);

		PaintFlags GetNativeTextDecorations(LabelHandler labelHandler) =>
			GetPlatformLabel(labelHandler).PaintFlags;

		float GetNativeLineHeight(LabelHandler labelHandler)
		{
			var textView = GetPlatformLabel(labelHandler);
			if (textView.LineSpacingMultiplier != 1)
				return textView.LineSpacingMultiplier;

			var fontMetrics = textView.Paint.GetFontMetricsInt();
			return 1 + textView.LineSpacingExtra / (fontMetrics.Descent - fontMetrics.Ascent);
		}

		// Distances between the first three baselines of a label, plus the distance
		// expected from the font height and the label's LineHeight.
		(int expectedGap, int firstGap, int secondGap) GetBaselineGaps(LabelHandler labelHandler, int widthPixels = 10000)
		{
			// A MAUI layout pass asks the handler for its size first, which refreshes the line spacing.
			// The stubs have an explicit size, so measure the platform view directly afterwards.
			labelHandler.GetDesiredSize(double.PositiveInfinity, double.PositiveInfinity);

			var textView = GetPlatformLabel(labelHandler);
			textView.Measure(
				global::Android.Views.View.MeasureSpec.MakeMeasureSpec(widthPixels, MeasureSpecMode.AtMost),
				global::Android.Views.View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified));

			var layout = textView.Layout;
			var fontMetrics = textView.Paint.GetFontMetricsInt();
			var expectedGap = (int)Math.Round((fontMetrics.Descent - fontMetrics.Ascent) * labelHandler.VirtualView.LineHeight);

			return (expectedGap,
				layout.GetLineBaseline(1) - layout.GetLineBaseline(0),
				layout.GetLineBaseline(2) - layout.GetLineBaseline(1));
		}

		Task<(int expectedGap, int firstGap, int secondGap)> GetHtmlBaselineGaps(string html, double lineHeight)
		{
			var label = new LabelStub() { Text = html, LineHeight = lineHeight };

			return GetValueAsync(label, handler =>
			{
				GetPlatformLabel(handler).UpdateTextHtml(label);
				return GetBaselineGaps(handler);
			});
		}
	}
}