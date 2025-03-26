using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Button)]
	public partial class ButtonTests : ControlsHandlerTestBase
	{
		[Theory]
		[ClassData(typeof(TextTransformCases))]
		public async Task InitialTextTransformApplied(string text, TextTransform transform, string expected)
		{
			var control = new Button() { Text = text, TextTransform = transform };
			var platformText = await GetPlatformText(await CreateHandlerAsync<ButtonHandler>(control));
			Assert.Equal(expected, platformText);
		}

		[Theory]
		[ClassData(typeof(TextTransformCases))]
		public async Task TextTransformUpdated(string text, TextTransform transform, string expected)
		{
			var control = new Button() { Text = text };
			var handler = await CreateHandlerAsync<ButtonHandler>(control);
			await InvokeOnMainThreadAsync(() => control.TextTransform = transform);
			var platformText = await GetPlatformText(handler);
			Assert.Equal(expected, platformText);
		}

		[Theory(DisplayName = "LineBreakMode Initializes Correctly")]
		[InlineData(LineBreakMode.MiddleTruncation)]
		[InlineData(LineBreakMode.HeadTruncation)]
		[InlineData(LineBreakMode.TailTruncation)]
		[InlineData(LineBreakMode.WordWrap)]
		[InlineData(LineBreakMode.CharacterWrap)]
		[InlineData(LineBreakMode.NoWrap)]
		public async Task LineBreakModeInitializesCorrectly(LineBreakMode lineBreakMode)
		{
			var xplatLineBreakMode = lineBreakMode;

			var button = new Button()
			{
				LineBreakMode = xplatLineBreakMode
			};

			var expectedValue = xplatLineBreakMode.ToPlatform();

			var handler = await CreateHandlerAsync<ButtonHandler>(button);

			await InvokeOnMainThreadAsync(() =>
			{
				Assert.Equal(expectedValue, GetPlatformLineBreakMode(handler));
			});
		}

		[Fact]
		[Description("The BackgroundColor of a Button should match with native background color")]
		public async Task ButtonBackgroundColorConsistent()
		{
			var expected = Colors.AliceBlue;
			var button = new Button()
			{
				BackgroundColor = expected,
				HeightRequest = 100,
				WidthRequest = 200
			};

			await ValidateHasColor(button, expected, typeof(ButtonHandler));
		}

		[Fact]
		[Description("The IsVisible property of a Button should match with native IsVisible")]
		public async Task VerifyButtonIsVisibleProperty()
		{
			var button = new Button();
			button.IsVisible = false;
			var expectedValue = button.IsVisible;

			var handler = await CreateHandlerAsync<ButtonHandler>(button);
			await InvokeOnMainThreadAsync(async () =>
			{
				var isVisible = await GetPlatformIsVisible(handler);
				Assert.Equal(expectedValue, isVisible);
			});
		}
	}
}