using System;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform.iOS;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class LabelHandlerTests
	{
		[Theory(DisplayName = "Font Family Initializes Correctly")]
		[InlineData(null)]
		[InlineData("Times New Roman")]
		[InlineData("Dokdo")]
		public async Task FontFamilyInitializesCorrectly(string family)
		{
			var label = new LabelStub()
			{
				Text = "Test",
				Font = Font.OfSize(family, 10)
			};

			var (services, nativeFont) = await GetValueAsync(label, handler => (handler.Services, GetNativeLabel(handler).Font));

			var fontManager = services.GetRequiredService<IFontManager>();

			var expectedNativeFont = fontManager.GetFont(Font.OfSize(family, 0.0));

			Assert.Equal(expectedNativeFont.FamilyName, nativeFont.FamilyName);
			if (string.IsNullOrEmpty(family))
				Assert.Equal(fontManager.DefaultFont.FamilyName, nativeFont.FamilyName);
			else
				Assert.NotEqual(fontManager.DefaultFont.FamilyName, nativeFont.FamilyName);
		}

		[Fact(DisplayName = "Horizontal TextAlignment Updates Correctly")]
		public async Task HorizontalTextAlignmentInitializesCorrectly()
		{
			var xplatHorizontalTextAlignment = TextAlignment.End;

			var labelStub = new LabelStub()
			{
				Text = "Test",
				HorizontalTextAlignment = xplatHorizontalTextAlignment
			};

			UITextAlignment expectedValue = UITextAlignment.Right;

			var values = await GetValueAsync(labelStub, (handler) =>
			{
				return new
				{
					ViewValue = labelStub.HorizontalTextAlignment,
					NativeViewValue = GetNativeHorizontalTextAlignment(handler)
				};
			});

			Assert.Equal(xplatHorizontalTextAlignment, values.ViewValue);
			values.NativeViewValue.AssertHasFlag(expectedValue);
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
			var insets = ((MauiLabel)handler.NativeView).TextInsets;

			Assert.Equal(5, insets.Left);
			Assert.Equal(10, insets.Top);
			Assert.Equal(15, insets.Right);
			Assert.Equal(20, insets.Bottom);
		}

		[Fact(DisplayName = "TextDecorations Initializes Correctly")]
		public async Task TextDecorationsInitializesCorrectly()
		{
			var xplatTextDecorations = TextDecorations.Underline;

			var labelHandler = new LabelStub()
			{
				Text = "Test", // Native values won't actually apply unless there's text
				TextDecorations = xplatTextDecorations
			};

			var values = await GetValueAsync(labelHandler, (handler) =>
			{
				return new
				{
					ViewValue = labelHandler.TextDecorations,
					GetNativeLabel(handler).AttributedText
				};
			});

			Assert.Equal(xplatTextDecorations, values.ViewValue);
			values.AttributedText.AssertHasUnderline();
		}

		[Fact]
		[Category(TestCategory.TextFormatting)]
		public async Task CanSetAlignmentAndLineHeight()
		{
			// Verifying that setting LineHeight (which requires an attributed string on iOS)
			// doesn't cancel out the text alignment value (which can be set without an attributed string)

			var xplatHorizontalTextAlignment = TextAlignment.End;
			double xplatLineHeight = 2;

			var label = new LabelStub()
			{
				Text = "Test",
				HorizontalTextAlignment = xplatHorizontalTextAlignment,
				LineHeight = xplatLineHeight
			};

			var expectedAlignment = UITextAlignment.Right;
			var expectedLineHeight = xplatLineHeight;

			var handler = await CreateHandlerAsync(label);
			var actualAlignment = await InvokeOnMainThreadAsync(() => GetNativeHorizontalTextAlignment(handler));
			var actualLineHeight = await InvokeOnMainThreadAsync(() => GetNativeLineHeight(handler));

			Assert.Equal(expectedLineHeight, actualLineHeight);
			Assert.Equal(expectedAlignment, actualAlignment);
		}

		[Fact]
		[Category(TestCategory.TextFormatting)]
		public async Task TextDecorationsAppliedWhenTextAdded()
		{
			TextDecorations xplatTextDecorations = TextDecorations.Underline;

			var label = new LabelStub() { TextDecorations = xplatTextDecorations }; // No text set

			var handler = await CreateHandlerAsync(label);

			label.Text = "Now we have text";
			await InvokeOnMainThreadAsync(() => handler.UpdateValue(nameof(label.Text)));

			var attributedText = await InvokeOnMainThreadAsync(() => GetAttributedText(handler));

			attributedText.AssertHasUnderline();
		}

		[Fact]
		[Category(TestCategory.TextFormatting)]
		public async Task LineHeightSurvivesTextDecorations()
		{
			TextDecorations xplatTextDecorations = TextDecorations.Underline;
			double xplatLineHeight = 2;
			var expectedLineHeight = xplatLineHeight;

			var label = new LabelStub() { Text = "test", LineHeight = xplatLineHeight };

			var handler = await CreateHandlerAsync(label);

			label.TextDecorations = xplatTextDecorations;
			await InvokeOnMainThreadAsync(() => handler.UpdateValue(nameof(label.TextDecorations)));

			var actualLineHeight = await InvokeOnMainThreadAsync(() => GetNativeLineHeight(handler));
			var attributedText = await InvokeOnMainThreadAsync(() => GetAttributedText(handler));

			Assert.Equal(expectedLineHeight, actualLineHeight);
			attributedText.AssertHasUnderline();
		}

		[Theory(DisplayName = "HorizontalTextAlignment adjusts for FlowDirection")]
		[InlineData(TextAlignment.Start, FlowDirection.LeftToRight, UITextAlignment.Left)]
		[InlineData(TextAlignment.End, FlowDirection.LeftToRight, UITextAlignment.Right)]
		[InlineData(TextAlignment.Start, FlowDirection.RightToLeft, UITextAlignment.Right)]
		[InlineData(TextAlignment.End, FlowDirection.RightToLeft, UITextAlignment.Left)]
		public async Task HorizontalTextAlignmentAdjustsForFlowDirection(TextAlignment alignment, FlowDirection flowDirection, UITextAlignment expected)
		{
			var label = new LabelStub
			{
				Text = "This is TEXT!",
				HorizontalTextAlignment = alignment,
				FlowDirection = flowDirection
			};


			var values = await GetValueAsync(label, (handler) =>
			{
				return new
				{
					ViewValue = label.HorizontalTextAlignment,
					NativeViewValue = GetNativeHorizontalTextAlignment(handler)
				};
			});

			Assert.Equal(expected, values.NativeViewValue);
		}

		UILabel GetNativeLabel(LabelHandler labelHandler) =>
			(UILabel)labelHandler.NativeView;

		string GetNativeText(LabelHandler labelHandler) =>
			GetNativeLabel(labelHandler).Text;

		Color GetNativeTextColor(LabelHandler labelHandler) =>
			GetNativeLabel(labelHandler).TextColor.ToColor();

		double GetNativeUnscaledFontSize(LabelHandler labelHandler) =>
			GetNativeLabel(labelHandler).Font.PointSize;

		bool GetNativeIsBold(LabelHandler labelHandler) =>
			GetNativeLabel(labelHandler).Font.FontDescriptor.SymbolicTraits.HasFlag(UIFontDescriptorSymbolicTraits.Bold);

		bool GetNativeIsItalic(LabelHandler labelHandler) =>
			GetNativeLabel(labelHandler).Font.FontDescriptor.SymbolicTraits.HasFlag(UIFontDescriptorSymbolicTraits.Italic);

		int GetNativeMaxLines(LabelHandler labelHandler) =>
 			(int)GetNativeLabel(labelHandler).Lines;

		double GetNativeCharacterSpacing(LabelHandler labelHandler)
		{
			var nativeLabel = GetNativeLabel(labelHandler);
			var text = nativeLabel.AttributedText;
			return text.GetCharacterSpacing();
		}

		async Task<NSAttributedString> GetAttributedText(LabelHandler labelHandler)
		{
			return await InvokeOnMainThreadAsync(() =>
			{
				var label = GetNativeLabel(labelHandler);
				return label.AttributedText;
			});
		}

		UITextAlignment GetNativeHorizontalTextAlignment(LabelHandler labelHandler) =>
			GetNativeLabel(labelHandler).TextAlignment;

		UILineBreakMode GetNativeLineBreakMode(LabelHandler labelHandler) =>
			GetNativeLabel(labelHandler).LineBreakMode;

		double GetNativeLineHeight(LabelHandler labelHandler)
		{
			var attrText = GetNativeLabel(labelHandler).AttributedText;

			if (attrText == null)
				return new nfloat(-1.0f);

			var paragraphStyle = (NSParagraphStyle)attrText.GetAttribute(UIStringAttributeKey.ParagraphStyle, 0, out _);

			if (paragraphStyle == null)
				return new nfloat(-1.0f);

			return paragraphStyle.LineHeightMultiple;
		}

		Task ValidateHasColor(ILabel label, Color color, Action action = null)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeLabel = GetNativeLabel(CreateHandler(label));
				action?.Invoke();
				nativeLabel.AssertContainsColor(color);
			});
		}
	}
}