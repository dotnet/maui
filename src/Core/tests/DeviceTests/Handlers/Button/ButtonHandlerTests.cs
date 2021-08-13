using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
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
				TextColor = Colors.Orange
			};

			await ValidatePropertyInitValue(button, () => button.TextColor, GetNativeTextColor, button.TextColor);
		}

		[Fact(DisplayName = "Null Text Color Doesn't Crash")]
		public async Task NullTextColorDoesntCrash()
		{
			var button = new ButtonStub()
			{
				Text = "Test",
				TextColor = null
			};

			await CreateHandlerAsync(button);
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
	}
}