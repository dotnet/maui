using System;
using System.Threading.Tasks;
using Android.Text;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Widget;
using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;
using AColor = Android.Graphics.Color;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ButtonHandlerTests
	{
		[Fact(DisplayName = "IsVisible updates Correctly")]
		public async Task IsVisibleUpdatesCorrectly()
		{
			var expected = Colors.Red;

			var layout = new LayoutStub();

			var hiddenButton = new ButtonStub
			{
				Text = "Text",
				TextColor = expected,
				Visibility = Visibility.Collapsed,
			};

			var button = new ButtonStub
			{
				Text = "Change IsVisible"
			};

			layout.Add(hiddenButton);
			layout.Add(button);

			var clicked = false;

			button.Clicked += delegate
			{
				hiddenButton.Visibility = Visibility.Visible;
				clicked = true;
			};

			await PerformClick(button);

			Assert.True(clicked);

			var result = await GetValueAsync(hiddenButton, GetVisibility);
			Assert.Equal(hiddenButton.Visibility, result);

			await ValidateHasColor(hiddenButton, expected);
		}

		[Fact(DisplayName = "CharacterSpacing Initializes Correctly")]
		public async Task CharacterSpacingInitializesCorrectly()
		{
			var xplatCharacterSpacing = 4;

			var button = new ButtonStub()
			{
				CharacterSpacing = xplatCharacterSpacing,
				Text = "Test"
			};

			float expectedValue = button.CharacterSpacing.ToEm();

			var values = await GetValueAsync(button, (handler) =>
			{
				return new
				{
					ViewValue = button.CharacterSpacing,
					PlatformViewValue = GetNativeCharacterSpacing(handler)
				};
			});

			Assert.Equal(xplatCharacterSpacing, values.ViewValue);
			Assert.Equal(expectedValue, values.PlatformViewValue, EmCoefficientPrecision);
		}

		[Theory]
		[InlineData("red.png", "#FF0000")]
		[InlineData("green.png", "#00FF00")]
		public async Task ImageSourceUpdatesCorrectly(string filename, string colorHex)
		{
			var image = new ButtonStub
			{
				ImageSource = new FileImageSourceStub("black.png"),
			};

			// Update the Button Icon
			image.ImageSource = new FileImageSourceStub(filename);

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler(image);

				bool imageLoaded = await Wait(() => ImageSourceLoaded(handler));

				Assert.True(imageLoaded);
				var expectedColor = Color.FromArgb(colorHex);
				await handler.PlatformView.AssertContainsColor(expectedColor, MauiContext);
			});
		}

		AppCompatButton GetNativeButton(ButtonHandler buttonHandler) =>
			buttonHandler.PlatformView;

		string GetNativeText(ButtonHandler buttonHandler) =>
			GetNativeButton(buttonHandler).Text;

		Color GetNativeTextColor(ButtonHandler buttonHandler)
		{
			int currentTextColorInt = GetNativeButton(buttonHandler).CurrentTextColor;
			AColor currentTextColor = new AColor(currentTextColorInt);

			return currentTextColor.ToColor();
		}

		Thickness GetNativePadding(ButtonHandler buttonHandler)
		{
			var appCompatButton = GetNativeButton(buttonHandler);

			return new Thickness(
				appCompatButton.PaddingLeft,
				appCompatButton.PaddingTop,
				appCompatButton.PaddingRight,
				appCompatButton.PaddingBottom);
		}

		Task PerformClick(IButton button)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				GetNativeButton(CreateHandler(button)).PerformClick();
			});
		}

		double GetNativeCharacterSpacing(ButtonHandler buttonHandler)
		{
			var button = GetNativeButton(buttonHandler);

			if (button != null)
			{
				return button.LetterSpacing;
			}

			return -1;
		}

		bool ImageSourceLoaded(ButtonHandler buttonHandler)
		{
			var image = buttonHandler.PlatformView.Icon ??
						TextViewCompat.GetCompoundDrawablesRelative(buttonHandler.PlatformView)[3];

			return image != null;
		}

		TextUtils.TruncateAt GetNativeLineBreakMode(ButtonHandler buttonHandler) =>
			GetNativeButton(buttonHandler).Ellipsize;
	}
}