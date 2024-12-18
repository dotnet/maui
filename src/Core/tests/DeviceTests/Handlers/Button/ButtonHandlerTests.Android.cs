using System.Threading.Tasks;
using Android.Graphics.Drawables;
using Android.Text;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Widget;
using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;
using AColor = Android.Graphics.Color;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ButtonHandlerTests
	{
		[Fact(DisplayName = "Button has Ripple Effect")]
		public async Task ButtonRippleEffect()
		{
			var layout = new LayoutStub();

			var button = new ButtonStub
			{
				Text = "Text",
				Background = new LinearGradientPaintStub(Colors.Red, Colors.Orange),
			};

			layout.Add(button);

			var clicked = false;

			button.Clicked += delegate
			{
				clicked = true;
			};

			await PerformClick(button);

			Assert.True(clicked);

			await AttachAndRun(button, async (handler) =>
			{
				await Task.Delay(100);

				var hasRipple = GetNativeHasRippleDrawable(handler);
				Assert.True(hasRipple);
			});
		}

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

		[Theory(DisplayName = "CornerRadius Initializes Correctly"
#if __ANDROID_23__
			, Skip = "Failing on Android 23"
#endif
		)]
		[InlineData(0, 0)]
		[InlineData(5, 5)]
		public async Task CornerRadiusInitializesCorrectly(int viewRadius, int platformViewRadius)
		{
			var button = new ButtonStub
			{
				Background = new LinearGradientPaintStub(Colors.Red, Colors.Orange),
				ImageSource = new FileImageSourceStub("black.png"),
				CornerRadius = viewRadius
			};

			var values = await GetValueAsync(button, (handler) =>
			{
				return new
				{
					ViewValue = button.CornerRadius,
					PlatformViewValue = GetNativeCornerRadius(handler)
				};
			});

			Assert.Equal(viewRadius, values.ViewValue);
			Assert.Equal(platformViewRadius, values.PlatformViewValue);
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

				await AssertEventually(() => ImageSourceLoaded(handler));

				var expectedColor = Color.FromArgb(colorHex);
				await handler.PlatformView.AssertContainsColor(expectedColor, MauiContext);
			});
		}

		AppCompatButton GetNativeButton(ButtonHandler buttonHandler) =>
			buttonHandler.PlatformView;

		string GetNativeText(ButtonHandler buttonHandler) =>
			GetNativeButton(buttonHandler).Text;

		int GetNativeCornerRadius(ButtonHandler buttonHandler)
		{
			var appCompatButton = GetNativeButton(buttonHandler);

			if (appCompatButton.Background is RippleDrawable rippleDrawable)
			{
				const int BackgroundDrawableId = 999;
				var background = rippleDrawable.FindDrawableByLayerId(BackgroundDrawableId) as GradientDrawable;

				if (background != null)
				{
					return (int)MauiContext.Context.FromPixels(background.CornerRadius);
				}
			}

			return -1;
		}

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

		bool GetNativeHasRippleDrawable(ButtonHandler buttonHandler)
		{
			var button = buttonHandler.PlatformView;

			if (button is null)
				return false;

			var rippleDrawable = button.Background as RippleDrawable;

			return rippleDrawable is not null;
		}
	}
}