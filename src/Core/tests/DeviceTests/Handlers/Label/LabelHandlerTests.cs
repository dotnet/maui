using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Label)]
	public partial class LabelHandlerTests : CoreHandlerTestBase<LabelHandler, LabelStub>
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

		[Fact(DisplayName = "Null Text Color Doesn't Crash")]
		public async Task NullTextColorDoesntCrash()
		{
			var label = new LabelStub()
			{
				Text = "Test",
				TextColor = null
			};

			await CreateHandlerAsync(label);
		}

#if !WINDOWS

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
#endif

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

#if !WINDOWS

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
					PlatformViewValue = GetNativeLineHeight(handler)
				};
			});

			float expectedValue = 1.5f;

			Assert.Equal(xplatLineHeight, values.ViewValue);
			Assert.Equal(expectedValue, values.PlatformViewValue);
		}
#endif

		[Category(TestCategory.Label)]
		public class LabelTextStyleTests : TextStyleHandlerTests<LabelHandler, LabelStub>
		{
		}
	}
}