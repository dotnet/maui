using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Button)]
	public partial class ButtonHandlerTests : CoreHandlerTestBase<ButtonHandler, ButtonStub>
	{
		const int Precision = 4;

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

		[Theory(DisplayName = "Padding Initializes Correctly")]
		[InlineData(0, 0, 0, 0)]
		[InlineData(1, 1, 1, 1)]
		[InlineData(10, 10, 10, 10)]
		[InlineData(5, 10, 15, 20)]
		public async Task PaddingInitializesCorrectly(double left, double top, double right, double bottom)
		{
			var user = new Thickness(left, top, right, bottom);

			var button = new ButtonStub
			{
				Text = "Test",
				Padding = user
			};

			var (expected, native) = await GetValueAsync(button, handler =>
			{
				var native = GetNativePadding(handler);
				var scaled = user;

#if __ANDROID__
				scaled = handler.PlatformView.Context!.ToPixels(scaled);
#endif

				return (scaled, native);
			});

			Assert.Equal(expected.Left, native.Left, Precision);
			Assert.Equal(expected.Top, native.Top, Precision);
			Assert.Equal(expected.Right, native.Right, Precision);
			Assert.Equal(expected.Bottom, native.Bottom, Precision);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("#FF0000")]
		[InlineData("#00FF00")]
		[InlineData("#0000FF")]
		[InlineData("#000000")]
		public async Task BorderColorInitializesCorrectly(string colorHex)
		{
			var expectedColor = Color.FromArgb(colorHex);

			var button = new ButtonStub
			{
				Text = "Test",
				StrokeColor = expectedColor,
				StrokeThickness = 3
			};

			var handler = await CreateHandlerAsync(button);

			await InvokeOnMainThreadAsync(async () =>
			{
				await handler.PlatformView.AssertContainsColor(expectedColor, MauiContext);
			});
		}

		[Theory]
		[InlineData(null)]
		[InlineData("#FF0000")]
		[InlineData("#00FF00")]
		[InlineData("#0000FF")]
		[InlineData("#000000")]
		public async Task BorderColorUpdatesCorrectly(string colorHex)
		{
			var expectedColor = Color.FromArgb(colorHex);

			var button = new ButtonStub
			{
				Text = "Test",
				StrokeThickness = 3
			};

			await AttachAndRun(button, async (handler) =>
			{
				button.StrokeColor = expectedColor;
				handler.UpdateValue(nameof(IButton.StrokeColor));

				await handler.PlatformView.AssertContainsColor(expectedColor, MauiContext);
			});
		}

		[Category(TestCategory.Button)]
		public class ButtonTextStyleTests : TextStyleHandlerTests<ButtonHandler, ButtonStub>
		{
		}

#if WINDOWS
		// TODO: buttons are not focusable on Android without FocusableInTouchMode=true and iOS is having issues
		//       https://github.com/dotnet/maui/issues/6482
		[Category(TestCategory.Button)]
		public class ButtonFocusTests : FocusHandlerTests<ButtonHandler, ButtonStub, VerticalStackLayoutStub>
		{
			public ButtonFocusTests()
			{
			}
		}
#endif
	}
}