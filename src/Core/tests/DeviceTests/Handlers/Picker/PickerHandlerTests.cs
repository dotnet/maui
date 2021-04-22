using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Picker)]
	public partial class PickerHandlerTests : HandlerTestBase<PickerHandler, PickerStub>
	{
		[Theory(DisplayName = "Font Size Initializes Correctly")]
		[InlineData(1)]
		[InlineData(10)]
		[InlineData(20)]
		[InlineData(100)]
		public async Task FontSizeInitializesCorrectly(int fontSize)
		{
			var items = new List<string>
			{
				"Item 1",
				"Item 2",
				"Item 3"
			};

			var picker = new PickerStub()
			{
				Title = "Select an Item",
				Font = Font.OfSize("Arial", fontSize)
			};

			picker.ItemsSource = items;
			picker.SelectedIndex = 0;

			await ValidatePropertyInitValue(picker, () => picker.Font.FontSize, GetNativeUnscaledFontSize, picker.Font.FontSize);
		}

		[Theory(DisplayName = "Font Attributes Initialize Correctly")]
		[InlineData(FontAttributes.None, false, false)]
		[InlineData(FontAttributes.Bold, true, false)]
		[InlineData(FontAttributes.Italic, false, true)]
		[InlineData(FontAttributes.Bold | FontAttributes.Italic, true, true)]
		public async Task FontAttributesInitializeCorrectly(FontAttributes attributes, bool isBold, bool isItalic)
		{
			var items = new List<string>
			{
				"Item 1",
				"Item 2",
				"Item 3"
			};

			var picker = new PickerStub()
			{
				Title = "Select an Item",
				Font = Font.OfSize("Arial", 10).WithAttributes(attributes)
			};

			picker.ItemsSource = items;
			picker.SelectedIndex = 0;

			await ValidatePropertyInitValue(picker, () => picker.Font.FontAttributes.HasFlag(FontAttributes.Bold), GetNativeIsBold, isBold);
			await ValidatePropertyInitValue(picker, () => picker.Font.FontAttributes.HasFlag(FontAttributes.Italic), GetNativeIsItalic, isItalic);
		}

		[Theory(DisplayName = "Updating Font Does Not Affect HorizontalTextAlignment")]
		[InlineData(10, 20)]
		[InlineData(20, 10)]
		public async Task FontDoesNotAffectHorizontalTextAlignment(double initialSize, double newSize)
		{
			var picker = new PickerStub
			{
				Title = "This is TEXT!",
				HorizontalTextAlignment = TextAlignment.Center,
				Font = Font.SystemFontOfSize(initialSize),
			};

			await ValidateUnrelatedPropertyUnaffected(
				picker,
				GetNativeHorizontalTextAlignment,
				nameof(IPicker.Font),
				() => picker.Font = Font.SystemFontOfSize(newSize));
		}

		[Theory(DisplayName = "Updating Text Does Not Affect HorizontalTextAlignment")]
		[InlineData("Short", "Long text here")]
		[InlineData("Long text here", "Short")]
		public async Task TextDoesNotAffectHorizontalTextAlignment(string initialText, string newText)
		{
			var picker = new PickerStub
			{
				Title = initialText,
				HorizontalTextAlignment = TextAlignment.Center,
			};

			await ValidateUnrelatedPropertyUnaffected(
				picker,
				GetNativeHorizontalTextAlignment,
				nameof(IPicker.Title),
				() => picker.Title = newText);
		}
	}
}