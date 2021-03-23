using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Button)]
	public partial class ButtonHandlerTests : HandlerTestBase<ButtonHandler, ButtonStub>
	{
		[Fact(DisplayName = "Text Initializes Correctly")]
		public async Task TextInitializesCorrectly()
		{
			var button = new ButtonStub()
			{
				Text = "Test"
			};

			await ValidatePropertyInitValue(button, () => button.Text, GetNativeText, button.Text);
		}

		[Fact(DisplayName = "Text Color Initializes Correctly")]
		public async Task TextColorInitializesCorrectly()
		{
			var button = new ButtonStub()
			{
				Text = "Test",
				TextColor = Color.Orange
			};

			await ValidatePropertyInitValue(button, () => button.TextColor, GetNativeTextColor, button.TextColor);
		}

		[Fact(DisplayName = "Click event fires Correctly")]
		public async Task ClickEventFires()
		{
			var clicked = false;

			var button = new ButtonStub();
			button.Clicked += delegate
			{
				clicked = true;
			};

			await PerformClick(button);

			Assert.True(clicked);
		}

		[Theory(DisplayName = "Font Size Initializes Correctly")]
		[InlineData(1)]
		[InlineData(10)]
		[InlineData(20)]
		[InlineData(100)]
		public async Task FontSizeInitializesCorrectly(int fontSize)
		{
			var button = new ButtonStub()
			{
				Text = "Test",
				Font = Font.OfSize("Arial", fontSize)
			};

			await ValidatePropertyInitValue(button, () => button.Font.FontSize, GetNativeUnscaledFontSize, button.Font.FontSize);
		}

		[Theory(DisplayName = "Font Attributes Initialize Correctly")]
		[InlineData(FontAttributes.None, false, false)]
		[InlineData(FontAttributes.Bold, true, false)]
		[InlineData(FontAttributes.Italic, false, true)]
		[InlineData(FontAttributes.Bold | FontAttributes.Italic, true, true)]
		public async Task FontAttributesInitializeCorrectly(FontAttributes attributes, bool isBold, bool isItalic)
		{
			var button = new ButtonStub()
			{
				Text = "Test",
				Font = Font.OfSize("Arial", 10).WithAttributes(attributes)
			};

			await ValidatePropertyInitValue(button, () => button.Font.FontAttributes.HasFlag(FontAttributes.Bold), GetNativeIsBold, isBold);
			await ValidatePropertyInitValue(button, () => button.Font.FontAttributes.HasFlag(FontAttributes.Italic), GetNativeIsItalic, isItalic);
		}
	}
}