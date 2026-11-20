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

		bool GetNativeIsTextSelectable(LabelHandler labelHandler) =>
			GetPlatformLabel(labelHandler).IsTextSelectable;

		[Fact(DisplayName = "Disabling IsTextSelectable Restores Interaction State")]
		public async Task DisablingIsTextSelectableRestoresInteractionState()
		{
			var label = new LabelStub() { Text = "Test" };

			var handler = await CreateHandlerAsync(label);

			var values = await InvokeOnMainThreadAsync(() =>
			{
				var textView = GetPlatformLabel(handler);
				var initial = (textView.Focusable, textView.Clickable, textView.LongClickable);

				label.IsTextSelectable = true;
				handler.UpdateValue(nameof(ILabel.IsTextSelectable));
				var selectable = (textView.Focusable, textView.Clickable, textView.LongClickable);

				label.IsTextSelectable = false;
				handler.UpdateValue(nameof(ILabel.IsTextSelectable));
				var restored = (textView.Focusable, textView.Clickable, textView.LongClickable);

				return (initial, selectable, restored);
			});

			Assert.Equal((true, true, true), values.selectable);
			Assert.Equal(values.initial, values.restored);
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

		float GetNativeLineHeight(LabelHandler labelHandler) =>
			GetPlatformLabel(labelHandler).LineSpacingMultiplier;
	}
}