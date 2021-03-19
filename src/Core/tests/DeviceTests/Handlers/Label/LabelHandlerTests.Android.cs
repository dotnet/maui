using System;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Text;
using Android.Views;
using Android.Widget;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Xunit;
using ATextAlignemnt = Android.Views.TextAlignment;

namespace Microsoft.Maui.DeviceTests
{
	public partial class LabelHandlerTests
	{
		[Theory(DisplayName = "Font Family Initializes Correctly")]
		[InlineData(null)]
		[InlineData("monospace")]
		[InlineData("Dokdo")]
		public async Task FontFamilyInitializesCorrectly(string family)
		{
			var label = new LabelStub()
			{
				Text = "Test",
				Font = Font.OfSize(family, 10)
			};

			var handler = await CreateHandlerAsync(label);
			var nativeLabel = GetNativeLabel(handler);

			var fontManager = handler.Services.GetRequiredService<IFontManager>();

			var nativeFont = fontManager.GetTypeface(Font.OfSize(family, 0.0));

			Assert.Equal(nativeFont, nativeLabel.Typeface);

			if (string.IsNullOrEmpty(family))
				Assert.Equal(fontManager.DefaultTypeface, nativeLabel.Typeface);
			else
				Assert.NotEqual(fontManager.DefaultTypeface, nativeLabel.Typeface);
		}

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
					NativeViewValue = GetNativeTextAlignment(handler)
				};
			});

			Assert.Equal(xplatHorizontalTextAlignment, values.ViewValue);

			(var gravity, var textAlignment) = values.NativeViewValue;

			// Device Tests runner has RTL support enabled, so we expect TextAlignment values
			// (If it didn't, we'd have to fall back to gravity)
			var expectedValue = ATextAlignemnt.ViewEnd;

			Assert.Equal(expectedValue, textAlignment);
		}

		[Fact(DisplayName = "Negative MaxLines value with wrap is correct")]
		public async Task NegativeMaxValueWithWrapIsCorrect()
		{
			var label = new LabelStub()
			{
				Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit",
				MaxLines = -1,
				LineBreakMode = LineBreakMode.WordWrap
			};

			var nativeValue = await GetValueAsync(label, GetNativeMaxLines);

			Assert.Equal(int.MaxValue, nativeValue);
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
			var (left, top, right, bottom) = GetNativePadding((TextView)handler.NativeView);

			var context = handler.View.Context;

			var expectedLeft = context.ToPixels(5);
			var expectedTop = context.ToPixels(10);
			var expectedRight = context.ToPixels(15);
			var expectedBottom = context.ToPixels(20);

			Assert.Equal(expectedLeft, left);
			Assert.Equal(expectedTop, top);
			Assert.Equal(expectedRight, right);
			Assert.Equal(expectedBottom, bottom);
		}

		[Fact(DisplayName = "[LabelHandler] TextDecorations Initializes Correctly")]
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
					NativeViewValue = GetNativeTextDecorations(handler)
				};
			});

			PaintFlags expectedValue = PaintFlags.UnderlineText;

			Assert.Equal(xplatTextDecorations, values.ViewValue);
			values.NativeViewValue.AssertHasFlag(expectedValue);
		}

		[Fact(DisplayName = "LineHeight Initializes Correctly")]
		public async Task LineHeightInitializesCorrectly()
		{
			var xplatLineHeight = 1.5d;

			var labelHandler = new LabelStub()
			{
				LineHeight = xplatLineHeight
			};

			var values = await GetValueAsync(labelHandler, (handler) =>
			{
				return new
				{
					ViewValue = labelHandler.LineHeight,
					NativeViewValue = GetNativeLineHeight(handler)
				};
			});

			float expectedValue = 1.5f;

			Assert.Equal(xplatLineHeight, values.ViewValue);
			Assert.Equal(expectedValue, values.NativeViewValue);
		}

		TextView GetNativeLabel(LabelHandler labelHandler) =>
			(TextView)labelHandler.View;

		string GetNativeText(LabelHandler labelHandler) =>
			GetNativeLabel(labelHandler).Text;

		Color GetNativeTextColor(LabelHandler labelHandler) =>
			((uint)GetNativeLabel(labelHandler).CurrentTextColor).ToColor();

		double GetNativeUnscaledFontSize(LabelHandler labelHandler)
		{
			var textView = GetNativeLabel(labelHandler);
			return textView.TextSize / textView.Resources.DisplayMetrics.Density;
		}

		bool GetNativeIsBold(LabelHandler labelHandler) =>
			GetNativeLabel(labelHandler).Typeface.IsBold;

		bool GetNativeIsItalic(LabelHandler labelHandler) =>
			GetNativeLabel(labelHandler).Typeface.IsItalic;

		(GravityFlags gravity, ATextAlignemnt alignment) GetNativeTextAlignment(LabelHandler labelHandler)
		{
			var textView = GetNativeLabel(labelHandler);
			return (textView.Gravity, textView.TextAlignment);
		}

		int GetNativeMaxLines(LabelHandler labelHandler) =>
			GetNativeLabel(labelHandler).MaxLines;

		Task ValidateNativeBackgroundColor(ILabel label, Color color)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				return GetNativeLabel(CreateHandler(label)).AssertContainsColor(color);
			});
		}

		(double left, double top, double right, double bottom) GetNativePadding(Android.Views.View view)
		{
			return (view.PaddingLeft, view.PaddingTop, view.PaddingRight, view.PaddingBottom);
		}

		double GetNativeCharacterSpacing(LabelHandler labelHandler) =>
			Math.Round(GetNativeLabel(labelHandler).LetterSpacing / UnitExtensions.EmCoefficient, EmCoefficientPrecision);

		TextUtils.TruncateAt GetNativeLineBreakMode(LabelHandler labelHandler) =>
			GetNativeLabel(labelHandler).Ellipsize;

		PaintFlags GetNativeTextDecorations(LabelHandler labelHandler) =>
			GetNativeLabel(labelHandler).PaintFlags;

		float GetNativeLineHeight(LabelHandler labelHandler) =>
			GetNativeLabel(labelHandler).LineSpacingMultiplier;
	}
}