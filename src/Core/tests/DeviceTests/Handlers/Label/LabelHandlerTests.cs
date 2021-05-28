using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Label)]
	public partial class LabelHandlerTests : HandlerTestBase<LabelHandler, LabelStub>
	{
		[Fact(DisplayName = "Background Initializes Correctly")]
		public async Task BackgroundInitializesCorrectly()
		{
			var brush = new SolidPaintStub(Colors.Blue);

			var label = new LabelStub()
			{
				Background = brush,
				Text = "Test"
			};

			await ValidateHasColor(label, Colors.Blue);
		}

		[Fact(DisplayName = "Text Initializes Correctly")]
		public async Task TextInitializesCorrectly()
		{
			var label = new LabelStub()
			{
				Text = "Test"
			};

			await ValidatePropertyInitValue(label, () => label.Text, GetNativeText, label.Text);
		}

		[Fact(DisplayName = "Text Color Initializes Correctly")]
		public async Task TextColorInitializesCorrectly()
		{
			var label = new LabelStub()
			{
				Text = "Test",
				TextColor = Colors.Red
			};

			await ValidatePropertyInitValue(label, () => label.TextColor, GetNativeTextColor, label.TextColor);
		}

		[Theory(DisplayName = "Font Size Initializes Correctly")]
		[InlineData(1)]
		[InlineData(10)]
		[InlineData(20)]
		[InlineData(100)]
		public async Task FontSizeInitializesCorrectly(int fontSize)
		{
			var label = new LabelStub()
			{
				Text = "Test",
				Font = Font.OfSize("Arial", fontSize)
			};

			await ValidatePropertyInitValue(label, () => label.Font.FontSize, GetNativeUnscaledFontSize, label.Font.FontSize);
		}

		[Theory(DisplayName = "Font Attributes Initialize Correctly")]
		[InlineData(FontWeight.Regular, false, false)]
		[InlineData(FontWeight.Bold, true, false)]
		[InlineData(FontWeight.Regular, false, true)]
		[InlineData(FontWeight.Bold, true, true)]
		public async Task FontAttributesInitializeCorrectly(FontWeight weight, bool isBold, bool isItalic)
		{
			var label = new LabelStub()
			{
				Text = "Test",
				Font = Font.OfSize("Arial", 10, weight, isItalic ? FontSlant.Italic : FontSlant.Default)
			};

			await ValidatePropertyInitValue(label, () => label.Font.Weight == FontWeight.Bold, GetNativeIsBold, isBold);
			await ValidatePropertyInitValue(label, () => label.Font.FontSlant == FontSlant.Italic, GetNativeIsItalic, isItalic);
		}

		[Fact(DisplayName = "CharacterSpacing Initializes Correctly")]
		public async Task CharacterSpacingInitializesCorrectly()
		{
			var label = new LabelStub()
			{
				Text = "Test CharacterSpacing",
				CharacterSpacing = 4.0
			};

			await ValidatePropertyInitValue(label, () => label.CharacterSpacing, GetNativeCharacterSpacing, label.CharacterSpacing);
		}

		[Theory(DisplayName = "CharacterSpacing Updates Correctly")]
		[InlineData(0, 0)]
		[InlineData(0, 5)]
		[InlineData(5, 0)]
		[InlineData(5, 5)]
		[InlineData(5, 10)]
		[InlineData(10, 5)]
		public async Task CharacterSpacingUpdatesCorrectly(double setValue, double unsetValue)
		{
			var label = new LabelStub
			{
				Text = "This is TEXT!"
			};

			await ValidatePropertyUpdatesValue(
				label,
				nameof(ILabel.CharacterSpacing),
				GetNativeCharacterSpacing,
				setValue,
				unsetValue);
		}

		[Theory(DisplayName = "Updating Font Does Not Affect CharacterSpacing")]
		[InlineData(10, 20)]
		[InlineData(20, 10)]
		public async Task FontDoesNotAffectCharacterSpacing(double initialSize, double newSize)
		{
			var label = new LabelStub
			{
				Text = "This is TEXT!",
				CharacterSpacing = 5,
				Font = Font.SystemFontOfSize(initialSize)
			};

			await ValidateUnrelatedPropertyUnaffected(
				label,
				GetNativeCharacterSpacing,
				nameof(ILabel.Font),
				() => label.Font = Font.SystemFontOfSize(newSize));
		}

		[Theory(DisplayName = "Updating Text Does Not Affect CharacterSpacing")]
		[InlineData("Short", "Longer Text")]
		[InlineData("Long thext here", "Short")]
		public async Task TextDoesNotAffectCharacterSpacing(string initialText, string newText)
		{
			var label = new LabelStub
			{
				Text = initialText,
				CharacterSpacing = 5,
			};

			await ValidateUnrelatedPropertyUnaffected(
				label,
				GetNativeCharacterSpacing,
				nameof(ILabel.Text),
				() => label.Text = newText);
		}

		[Theory(DisplayName = "Updating Font Does Not Affect HorizontalTextAlignment")]
		[InlineData(10, 20)]
		[InlineData(20, 10)]
		public async Task FontDoesNotAffectHorizontalTextAlignment(double initialSize, double newSize)
		{
			var label = new LabelStub
			{
				Text = "This is TEXT!",
				HorizontalTextAlignment = TextAlignment.Center,
				Font = Font.SystemFontOfSize(initialSize),
			};

			await ValidateUnrelatedPropertyUnaffected(
				label,
				GetNativeHorizontalTextAlignment,
				nameof(ILabel.Font),
				() => label.Font = Font.SystemFontOfSize(newSize));
		}

		[Theory(DisplayName = "Font Family and Weight Initializes Correctly")]
		[InlineData(null, FontWeight.Regular, FontSlant.Default)]
		[InlineData(null, FontWeight.Regular, FontSlant.Italic)]
		[InlineData(null, FontWeight.Bold, FontSlant.Default)]
		[InlineData(null, FontWeight.Bold, FontSlant.Italic)]
		[InlineData("Lobster Two", FontWeight.Regular, FontSlant.Default)]
		[InlineData("Lobster Two", FontWeight.Regular, FontSlant.Italic)]
		[InlineData("Lobster Two", FontWeight.Bold, FontSlant.Default)]
		[InlineData("Lobster Two", FontWeight.Bold, FontSlant.Italic)]
#if !__IOS__
		// iOS cannot force a font to be bold like all other OS
		[InlineData("Dokdo", FontWeight.Regular, FontSlant.Default)]
		[InlineData("Dokdo", FontWeight.Regular, FontSlant.Italic)]
		[InlineData("Dokdo", FontWeight.Bold, FontSlant.Default)]
		[InlineData("Dokdo", FontWeight.Bold, FontSlant.Italic)]
#endif
#if __ANDROID__
		// "monospace" is a special font name on Android
		[InlineData("monospace", FontWeight.Regular, FontSlant.Default)]
		[InlineData("monospace", FontWeight.Regular, FontSlant.Italic)]
		[InlineData("monospace", FontWeight.Bold, FontSlant.Default)]
		[InlineData("monospace", FontWeight.Bold, FontSlant.Italic)]
#endif
		public async Task FontFamilyAndAttributesInitializesCorrectly(string family, FontWeight weight, FontSlant slant)
		{
			var label = new LabelStub
			{
				Text = "Test",
				Font = Font.OfSize(family, 30, weight, slant)
			};

			var (isBold, isItalic) = await GetValueAsync(label, (handler) =>
			{
				var isBold = GetNativeIsBold(handler);
				var isItalic = GetNativeIsItalic(handler);

				return (isBold, isItalic);
			});

			Assert.Equal(weight == FontWeight.Bold, isBold);
			Assert.Equal(slant == FontSlant.Italic, isItalic);
		}

		[Theory(DisplayName = "Updating Text Does Not Affect HorizontalTextAlignment")]
		[InlineData("Short", "Longer Text")]
		[InlineData("Long thext here", "Short")]
		public async Task TextDoesNotAffectHorizontalTextAlignment(string initialText, string newText)
		{
			var label = new LabelStub
			{
				Text = initialText,
				HorizontalTextAlignment = TextAlignment.Center,
			};

			await ValidateUnrelatedPropertyUnaffected(
				label,
				GetNativeHorizontalTextAlignment,
				nameof(ILabel.Text),
				() => label.Text = newText);
		}

		[Theory(DisplayName = "Updating LineHeight Does Not Affect HorizontalTextAlignment")]
		[InlineData(1, 2)]
		[InlineData(2, 1)]
		public async Task LineHeightDoesNotAffectHorizontalTextAlignment(double initialSize, double newSize)
		{
			var label = new LabelStub
			{
				Text = "This is TEXT!",
				HorizontalTextAlignment = TextAlignment.Center,
				LineHeight = initialSize,
			};

			await ValidateUnrelatedPropertyUnaffected(
				label,
				GetNativeHorizontalTextAlignment,
				nameof(ILabel.LineHeight),
				() => label.LineHeight = newSize);
		}

		[Theory(DisplayName = "Updating TextDecorations Does Not Affect HorizontalTextAlignment")]
		[InlineData(TextDecorations.None, TextDecorations.Underline)]
		[InlineData(TextDecorations.Underline, TextDecorations.Strikethrough)]
		[InlineData(TextDecorations.Underline, TextDecorations.None)]
		public async Task TextDecorationsDoesNotAffectHorizontalTextAlignment(TextDecorations initialDecorations, TextDecorations newDecorations)
		{
			var label = new LabelStub
			{
				Text = "This is TEXT!",
				HorizontalTextAlignment = TextAlignment.Center,
				TextDecorations = initialDecorations,
			};

			await ValidateUnrelatedPropertyUnaffected(
				label,
				GetNativeHorizontalTextAlignment,
				nameof(ILabel.TextDecorations),
				() => label.TextDecorations = newDecorations);
		}

		[Fact(DisplayName = "LineBreakMode Initializes Correctly")]
		public async Task LineBreakModeInitializesCorrectly()
		{
			var xplatLineBreakMode = LineBreakMode.TailTruncation;

			var label = new LabelStub()
			{
				LineBreakMode = xplatLineBreakMode
			};

			var expectedValue = xplatLineBreakMode.ToNative();

			var values = await GetValueAsync(label, (handler) =>
			{
				return new
				{
					ViewValue = label.LineBreakMode,
					NativeViewValue = GetNativeLineBreakMode(handler)
				};
			});

			Assert.Equal(xplatLineBreakMode, values.ViewValue);
			Assert.Equal(expectedValue, values.NativeViewValue);
		}

		[Fact(DisplayName = "LineBreakMode does not affect to MaxLines")]
		public async Task LineBreakModeDoesNotAffectMaxLines()
		{
			var label = new LabelStub()
			{
				Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit",
				MaxLines = 3,
				LineBreakMode = LineBreakMode.WordWrap,
			};

			var handler = await CreateHandlerAsync(label);
			var nativeLabel = GetNativeLabel(handler);

			await InvokeOnMainThreadAsync(() =>
			{
				Assert.Equal(3, GetNativeMaxLines(handler));
				Assert.Equal(LineBreakMode.WordWrap.ToNative(), GetNativeLineBreakMode(handler));

				label.LineBreakMode = LineBreakMode.CharacterWrap;
				nativeLabel.UpdateLineBreakMode(label);

				Assert.Equal(3, GetNativeMaxLines(handler));
				Assert.Equal(LineBreakMode.CharacterWrap.ToNative(), GetNativeLineBreakMode(handler));
			});
		}

		[Fact(DisplayName = "Single LineBreakMode changes MaxLines")]
		public async Task SingleLineBreakModeChangesMaxLines()
		{
			var label = new LabelStub()
			{
				Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit",
				MaxLines = 3,
				LineBreakMode = LineBreakMode.WordWrap,
			};

			var handler = await CreateHandlerAsync(label);
			var nativeLabel = GetNativeLabel(handler);

			await InvokeOnMainThreadAsync(() =>
			{
				Assert.Equal(3, GetNativeMaxLines(handler));
				Assert.Equal(LineBreakMode.WordWrap.ToNative(), GetNativeLineBreakMode(handler));

				label.LineBreakMode = LineBreakMode.HeadTruncation;
				nativeLabel.UpdateLineBreakMode(label);

				Assert.Equal(1, GetNativeMaxLines(handler));
				Assert.Equal(LineBreakMode.HeadTruncation.ToNative(), GetNativeLineBreakMode(handler));
			});
		}

		[Theory(DisplayName = "Unsetting single LineBreakMode resets MaxLines")]
		[InlineData(LineBreakMode.HeadTruncation)]
		[InlineData(LineBreakMode.NoWrap)]
		public async Task UnsettingSingleLineBreakModeResetsMaxLines(LineBreakMode newMode)
		{
			var label = new LabelStub()
			{
				Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit",
				MaxLines = 3,
				LineBreakMode = LineBreakMode.WordWrap,
			};

			var handler = await CreateHandlerAsync(label);
			var nativeLabel = GetNativeLabel(handler);

			await InvokeOnMainThreadAsync(() =>
			{
				Assert.Equal(3, GetNativeMaxLines(handler));
				Assert.Equal(LineBreakMode.WordWrap.ToNative(), GetNativeLineBreakMode(handler));

				label.LineBreakMode = newMode;
				nativeLabel.UpdateLineBreakMode(label);

				Assert.Equal(1, GetNativeMaxLines(handler));
				Assert.Equal(newMode.ToNative(), GetNativeLineBreakMode(handler));

				label.LineBreakMode = LineBreakMode.WordWrap;
				nativeLabel.UpdateLineBreakMode(label);

				Assert.Equal(3, GetNativeMaxLines(handler));
				Assert.Equal(LineBreakMode.WordWrap.ToNative(), GetNativeLineBreakMode(handler));
			});
		}

		[Fact(DisplayName = "MaxLines Initializes Correctly")]
		public async Task MaxLinesInitializesCorrectly()
		{
			var label = new LabelStub()
			{
				Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit",
				MaxLines = 2
			};

			await ValidatePropertyInitValue(label, () => label.MaxLines, GetNativeMaxLines, label.MaxLines);
		}

		[Fact]
		[Category(TestCategory.TextFormatting)]
		public async Task LineHeightAppliedWhenTextAdded()
		{
			double xplatLineHeight = 2;
			var expectedLineHeight = xplatLineHeight;

			var label = new LabelStub() { LineHeight = xplatLineHeight }; // No text set

			var handler = await CreateHandlerAsync(label);

			label.Text = "Now we have text";
			await InvokeOnMainThreadAsync(() => handler.UpdateValue(nameof(label.Text)));

			var actualLineHeight = await InvokeOnMainThreadAsync(() => GetNativeLineHeight(handler));

			Assert.Equal(expectedLineHeight, actualLineHeight);
		}

		[Fact]
		[Category(TestCategory.TextFormatting)]
		public async Task CharacterSpacingAppliedWhenTextAdded()
		{
			double xplatCharacterSpacing = 1.5;
			var expectedCharacterSpacing = xplatCharacterSpacing;

			var label = new LabelStub() { CharacterSpacing = xplatCharacterSpacing }; // No text set

			var handler = await CreateHandlerAsync(label);

			label.Text = "Now we have text";
			await InvokeOnMainThreadAsync(() => handler.UpdateValue(nameof(label.Text)));

			var actualCharacterSpacing = await InvokeOnMainThreadAsync(() => GetNativeCharacterSpacing(handler));

			Assert.Equal(expectedCharacterSpacing, actualCharacterSpacing);
		}

		[Fact]
		[Category(TestCategory.TextFormatting)]
		public async Task LineHeightSurvivesCharacterSpacing()
		{
			double xplatCharacterSpacing = 1.5;
			var expectedCharacterSpacing = xplatCharacterSpacing;
			double xplatLineHeight = 2;
			var expectedLineHeight = xplatLineHeight;

			var label = new LabelStub() { Text = "test", LineHeight = xplatLineHeight };

			var handler = await CreateHandlerAsync(label);

			label.CharacterSpacing = xplatCharacterSpacing;
			await InvokeOnMainThreadAsync(() => handler.UpdateValue(nameof(label.CharacterSpacing)));

			var actualLineHeight = await InvokeOnMainThreadAsync(() => GetNativeLineHeight(handler));
			var actualCharacterSpacing = await InvokeOnMainThreadAsync(() => GetNativeCharacterSpacing(handler));

			Assert.Equal(expectedLineHeight, actualLineHeight);
			Assert.Equal(expectedCharacterSpacing, actualCharacterSpacing);
		}

		[Theory(DisplayName = "Negative MaxLines value with wrap is correct")]
#if __IOS__
		[InlineData(0)]
#elif __ANDROID__
		[InlineData(int.MaxValue)]
#endif
		public async Task NegativeMaxValueWithWrapIsCorrect(int expectedLines)
		{
			var label = new LabelStub()
			{
				Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit",
				MaxLines = -1,
				LineBreakMode = LineBreakMode.WordWrap
			};

			var nativeValue = await GetValueAsync(label, GetNativeMaxLines);

			Assert.Equal(expectedLines, nativeValue);
		}

		[Fact(DisplayName = "LineHeight Initializes Correctly")]
		public async Task LineHeightInitializesCorrectly()
		{
			var xplatLineHeight = 1.5d;

			var labelHandler = new LabelStub()
			{
				Text = "test",
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
	}
}