using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Label)]
	public partial class LabelHandlerTests : HandlerTestBase<LabelHandler>
	{
		public LabelHandlerTests(HandlerTestFixture fixture) : base(fixture)
		{
		}

		[Fact(DisplayName = "Background Color Initializes Correctly")]
		public async Task BackgroundColorInitializesCorrectly()
		{
			var label = new LabelStub()
			{
				BackgroundColor = Color.Blue,
				Text = "Test"
			};

			await ValidateNativeBackgroundColor(label, Color.Blue);
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
				TextColor = Color.Red
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
		[InlineData(FontAttributes.None, false, false)]
		[InlineData(FontAttributes.Bold, true, false)]
		[InlineData(FontAttributes.Italic, false, true)]
		[InlineData(FontAttributes.Bold | FontAttributes.Italic, true, true)]
		public async Task AttributesInitializeCorrectly(FontAttributes attributes, bool isBold, bool isItalic)
		{
			var label = new LabelStub()
			{
				Text = "Test",
				Font = Font.OfSize("Arial", 10).WithAttributes(attributes)
			};

			await ValidatePropertyInitValue(label, () => label.Font.FontAttributes.HasFlag(FontAttributes.Bold), GetNativeIsBold, isBold);
			await ValidatePropertyInitValue(label, () => label.Font.FontAttributes.HasFlag(FontAttributes.Italic), GetNativeIsItalic, isItalic);
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

		[Fact(DisplayName = "Updating Font Does Not Affect CharacterSpacing")]
		public async Task FontDoesNotAffectCharacterSpacing()
		{
			var label = new LabelStub
			{
				Text = "This is TEXT!",
				CharacterSpacing = 5,
				Font = Font.SystemFontOfSize(20)
			};

			await ValidateUnrelatedPropertyUnaffected(
				label,
				GetNativeCharacterSpacing,
				nameof(ILabel.Font),
				() => label.Font = Font.SystemFontOfSize(15));
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
	}
}