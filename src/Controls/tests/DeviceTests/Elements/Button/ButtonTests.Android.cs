#nullable enable
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Android.Content.Res;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Widget;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ButtonTests
	{
		AppCompatButton GetPlatformButton(ButtonHandler buttonHandler) =>
			(AppCompatButton)buttonHandler.PlatformView;

		Task<string?> GetPlatformText(ButtonHandler buttonHandler)
		{
			return InvokeOnMainThreadAsync(() => GetPlatformButton(buttonHandler).Text);
		}

		global::Android.Text.TextUtils.TruncateAt? GetPlatformLineBreakMode(ButtonHandler buttonHandler) =>
			GetPlatformButton(buttonHandler).Ellipsize;

		Task<float> GetPlatformOpacity(ButtonHandler buttonHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetPlatformButton(buttonHandler);
				return nativeView.Alpha;
			});
		}

		Task<bool> GetPlatformIsVisible(ButtonHandler buttonHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetPlatformButton(buttonHandler);
				return nativeView.Visibility == global::Android.Views.ViewStates.Visible;
			});
		}

		[Theory(DisplayName = "Button Icon has Correct Position")]
		[InlineData(Button.ButtonContentLayout.ImagePosition.Left)]
		[InlineData(Button.ButtonContentLayout.ImagePosition.Top)]
		[InlineData(Button.ButtonContentLayout.ImagePosition.Right)]
		[InlineData(Button.ButtonContentLayout.ImagePosition.Bottom)]
		public async Task NestedButtonHasExpectedIconPosition(Button.ButtonContentLayout.ImagePosition imagePosition)
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Page, PageHandler>();
					handlers.AddHandler<Window, WindowHandlerStub>();
					handlers.AddHandler<Layout, LayoutHandler>();
					handlers.AddHandler<Button, ButtonHandler>();
				});
			});

			var button = new Button()
			{
				Text = "Hello",
				ImageSource = "red.png",
				ContentLayout = new Button.ButtonContentLayout(imagePosition, 8)
			};

			var page = new ContentPage()
			{
				Content = new HorizontalStackLayout()
				{
					button
				}
			};

			await CreateHandlerAndAddToWindow(page, () =>
			{
				var handler = CreateHandler<ButtonHandler>(button);

				var platformButton = (AppCompatButton)handler.PlatformView;

				int matchingDrawableIndex = imagePosition switch
				{
					Button.ButtonContentLayout.ImagePosition.Left => 0,
					Button.ButtonContentLayout.ImagePosition.Top => 1,
					Button.ButtonContentLayout.ImagePosition.Right => 2,
					Button.ButtonContentLayout.ImagePosition.Bottom => 3,
					_ => throw new InvalidOperationException(),
				};

				// Assert that the image is in the expected position
#pragma warning disable CS0618 // Type or member is obsolete
				var drawables = TextViewCompat.GetCompoundDrawablesRelative(platformButton);
#pragma warning restore CS0618 // Type or member is obsolete
				Assert.NotNull(drawables?[matchingDrawableIndex]);
			});
		}

		[Fact]
		[Description("The Opacity property of a Button should match with native Opacity")]
		public async Task VerifyButtonOpacityProperty()
		{
			var button = new Button
			{
				Opacity = 0.35f
			};
			var expectedValue = button.Opacity;

			var handler = await CreateHandlerAsync<ButtonHandler>(button);
			await InvokeOnMainThreadAsync(async () =>
			{
				var nativeOpacityValue = await GetPlatformOpacity(handler);
				Assert.Equal(expectedValue, nativeOpacityValue);
			});
		}

		[Fact]
		[Description("The ScaleX property of a Button should match with native ScaleX")]
		public async Task ScaleXConsistent()
		{
			var button = new Button() { ScaleX = 0.45f };
			var expected = button.ScaleX;
			var handler = await CreateHandlerAsync<ButtonHandler>(button);
			var platformButton = GetPlatformButton(handler);
			var platformScaleX = await InvokeOnMainThreadAsync(() => platformButton.ScaleX);
			Assert.Equal(expected, platformScaleX);
		}

		[Fact]
		[Description("The ScaleY property of a Button should match with native ScaleY")]
		public async Task ScaleYConsistent()
		{
			var button = new Button() { ScaleY = 1.23f };
			var expected = button.ScaleY;
			var handler = await CreateHandlerAsync<ButtonHandler>(button);
			var platformButton = GetPlatformButton(handler);
			var platformScaleY = await InvokeOnMainThreadAsync(() => platformButton.ScaleY);
			Assert.Equal(expected, platformScaleY);
		}

		[Fact]
		[Description("The Scale property of a Button should match with native Scale")]
		public async Task ScaleConsistent()
		{
			var button = new Button() { Scale = 2.0f };
			var expected = button.Scale;
			var handler = await CreateHandlerAsync<ButtonHandler>(button);
			var platformButton = GetPlatformButton(handler);
			var platformScaleX = await InvokeOnMainThreadAsync(() => platformButton.ScaleX);
			var platformScaleY = await InvokeOnMainThreadAsync(() => platformButton.ScaleY);
			Assert.Equal(expected, platformScaleX);
			Assert.Equal(expected, platformScaleY);
		}

		[Fact]
		[Description("The RotationX property of a Button should match with native RotationX")]
		public async Task RotationXConsistent()
		{
			var button = new Button() { RotationX = 33.0 };
			var expected = button.RotationX;
			var handler = await CreateHandlerAsync<ButtonHandler>(button);
			var platformButton = GetPlatformButton(handler);
			var platformRotationX = await InvokeOnMainThreadAsync(() => platformButton.RotationX);
			Assert.Equal(expected, platformRotationX);
		}

		[Fact]
		[Description("The RotationY property of a Button should match with native RotationY")]
		public async Task RotationYConsistent()
		{
			var button = new Button() { RotationY = 87.0 };
			var expected = button.RotationY;
			var handler = await CreateHandlerAsync<ButtonHandler>(button);
			var platformButton = GetPlatformButton(handler);
			var platformRotationY = await InvokeOnMainThreadAsync(() => platformButton.RotationY);
			Assert.Equal(expected, platformRotationY);
		}

		[Fact]
		[Description("The Rotation property of a Button should match with native Rotation")]
		public async Task RotationConsistent()
		{
			var button = new Button() { Rotation = 23.0 };
			var expected = button.Rotation;
			var handler = await CreateHandlerAsync<ButtonHandler>(button);
			var platformButton = GetPlatformButton(handler);
			var platformRotation = await InvokeOnMainThreadAsync(() => platformButton.Rotation);
			Assert.Equal(expected, platformRotation);
		}

		[Fact]
		[Description("The RippleColor property of a Button Platform Specific should match with native RippleDrawable Color")]
		public async Task ButtonRippleColorPlatformSpecific()
		{
			var button = new Button();
			var expected = Graphics.Colors.Red;
			Controls.PlatformConfiguration.AndroidSpecific.Button.SetRippleColor(button, expected);
			var handler = await CreateHandlerAsync<ButtonHandler>(button);
			var platformButton = GetPlatformButton(handler);

			if (platformButton.Background is global::Android.Graphics.Drawables.RippleDrawable rippleDrawable)
			{
				var constantState = rippleDrawable.GetConstantState();
				if (constantState is not null)
				{
					var colorField = constantState.Class.GetDeclaredField("mColor");
					colorField.Accessible = true;
					if (colorField.Get(constantState) is ColorStateList colorStateList)
					{
						var color = colorStateList?.DefaultColor;
						Assert.Equal(expected.ToPlatform(), color);
					}
				}
			}
		}

		//src/Compatibility/Core/tests/Android/TranslationTests.cs
		[Fact]
		[Description("The Translation property of a Button should match with native Translation")]
		public async Task ButtonTranslationConsistent()
		{
			var button = new Button()
			{
				Text = "Button Test",
				TranslationX = 50,
				TranslationY = -20
			};

			var handler = await CreateHandlerAsync<ButtonHandler>(button);
			var nativeView = GetPlatformButton(handler);
			await InvokeOnMainThreadAsync(() =>
			{
				AssertTranslationMatches(nativeView, button.TranslationX, button.TranslationY);
			});
		}
	}
}
