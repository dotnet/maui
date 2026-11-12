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
			var fontMetrics = textView.Paint.GetFontMetricsInt();
			return 1 + textView.LineSpacingExtra / (fontMetrics.Descent - fontMetrics.Ascent);
		}

		// Distances between consecutive baselines of a three-line label, plus the
		// distance expected from the font height and the label's LineHeight.
		(int expectedGap, int firstGap, int secondGap) GetBaselineGaps(LabelHandler labelHandler)
		{
			var textView = GetPlatformLabel(labelHandler);
			textView.Measure(
				global::Android.Views.View.MeasureSpec.MakeMeasureSpec(10000, MeasureSpecMode.AtMost),
				global::Android.Views.View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified));

			var layout = textView.Layout;
			var fontMetrics = textView.Paint.GetFontMetricsInt();
			var expectedGap = (int)Math.Round((fontMetrics.Descent - fontMetrics.Ascent) * labelHandler.VirtualView.LineHeight);

			return (expectedGap,
				layout.GetLineBaseline(1) - layout.GetLineBaseline(0),
				layout.GetLineBaseline(2) - layout.GetLineBaseline(1));
		}
	}
}