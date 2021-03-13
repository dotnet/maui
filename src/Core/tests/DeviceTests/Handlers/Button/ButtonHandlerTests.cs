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
		public ButtonHandlerTests(HandlerTestFixture fixture) : base(fixture)
		{
		}

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
	}
}