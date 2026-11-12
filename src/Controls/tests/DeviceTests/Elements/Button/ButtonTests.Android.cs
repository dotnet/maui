#nullable enable
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Android.Content.Res;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Widget;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;
using GravityFlags = Android.Views.GravityFlags;
using MeasureSpecMode = Android.Views.MeasureSpecMode;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ButtonTests
	{
		[Fact]
		public async Task NativeAlignmentIsNotRewrittenWhenUnset()
		{
			var button = new Button { Text = "Initial title" };
			var handler = await CreateHandlerAsync<ButtonHandler>(button);
			await InvokeOnMainThreadAsync(() =>
			{
				var platform = handler.PlatformView;
				var gravity = GravityFlags.End | GravityFlags.Bottom;
				platform.Gravity = gravity;
				platform.TextAlignment = global::Android.Views.TextAlignment.ViewEnd;
				button.Text = "Updated title";
				Assert.Equal(gravity, platform.Gravity);
				Assert.Equal(global::Android.Views.TextAlignment.ViewEnd, platform.TextAlignment);
			});
		}

		[Theory]
		[InlineData(TextAlignment.Start)]
		[InlineData(TextAlignment.Center)]
		[InlineData(TextAlignment.End)]
		public async Task NativeTextAlignmentCanBeChangedAndCleared(TextAlignment alignment)
		{
			var button = new Button { Text = "First line\nSecond line" };
			var handler = await CreateHandlerAsync<ButtonHandler>(button);
			await InvokeOnMainThreadAsync(() =>
			{
				var platform = handler.PlatformView;
				var originalGravity = platform.Gravity;
				var originalTextAlignment = platform.TextAlignment;
				const GravityFlags horizontalMask = GravityFlags.HorizontalGravityMask | GravityFlags.RelativeLayoutDirection;
				var horizontal = alignment switch
				{
					TextAlignment.Start => GravityFlags.Start,
					TextAlignment.End => GravityFlags.End,
					_ => GravityFlags.CenterHorizontal
				};
				var vertical = alignment switch
				{
					TextAlignment.Start => GravityFlags.Top,
					TextAlignment.End => GravityFlags.Bottom,
					_ => GravityFlags.CenterVertical
				};

				button.HorizontalTextAlignment = alignment;
				Assert.Equal(horizontal, platform.Gravity & horizontalMask);
				Assert.Equal(originalGravity & GravityFlags.VerticalGravityMask, platform.Gravity & GravityFlags.VerticalGravityMask);
				Assert.Equal(alignment switch
				{
					TextAlignment.Start => global::Android.Views.TextAlignment.ViewStart,
					TextAlignment.End => global::Android.Views.TextAlignment.ViewEnd,
					_ => global::Android.Views.TextAlignment.Center
				}, platform.TextAlignment);

				button.VerticalTextAlignment = alignment;
				Assert.Equal(vertical, platform.Gravity & GravityFlags.VerticalGravityMask);
				button.ClearValue(Button.HorizontalTextAlignmentProperty);
				Assert.Equal(originalGravity & horizontalMask, platform.Gravity & horizontalMask);
				Assert.Equal(originalTextAlignment, platform.TextAlignment);
				Assert.Equal(vertical, platform.Gravity & GravityFlags.VerticalGravityMask);

				button.Text = string.Empty;
				Assert.Equal(originalGravity, platform.Gravity);
				button.Text = "Restored title";
				Assert.Equal(vertical, platform.Gravity & GravityFlags.VerticalGravityMask);
				button.ClearValue(Button.VerticalTextAlignmentProperty);
				Assert.Equal(originalGravity, platform.Gravity);
			});
		}

		[Theory]
		[InlineData(Button.ButtonContentLayout.ImagePosition.Left, false)]
		[InlineData(Button.ButtonContentLayout.ImagePosition.Top, false)]
		[InlineData(Button.ButtonContentLayout.ImagePosition.Right, false)]
		[InlineData(Button.ButtonContentLayout.ImagePosition.Bottom, false)]
		[InlineData(Button.ButtonContentLayout.ImagePosition.Left, true)]
		[InlineData(Button.ButtonContentLayout.ImagePosition.Top, true)]
		[InlineData(Button.ButtonContentLayout.ImagePosition.Right, true)]
		[InlineData(Button.ButtonContentLayout.ImagePosition.Bottom, true)]
		public async Task ImageAndTextGroupRendersAtRequestedAlignment(Button.ButtonContentLayout.ImagePosition position, bool rtl)
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Button, ButtonHandler>();
				});
			});
			var button = new Button
			{
				Text = "A\nB",
				ImageSource = "red.png",
				TextColor = Colors.Blue,
				BackgroundColor = Colors.White,
				LineBreakMode = LineBreakMode.WordWrap,
				FontSize = 20,
				WidthRequest = 300,
				HeightRequest = 260,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Padding = new Thickness(24, 12, 8, 20),
				FlowDirection = rtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight,
				ContentLayout = new Button.ButtonContentLayout(position, 8)
			};

			await CreateHandlerAndAddToWindow(button, async () =>
			{
				var platform = Assert.IsType<ButtonHandler>(button.Handler).PlatformView;
				await AssertHelpers.AssertEventually(() => platform.Icon is not null && platform.Layout is not null && platform.Width > 0 && platform.Height > 0);
				foreach (var alignment in new[] { TextAlignment.Start, TextAlignment.Center, TextAlignment.End })
				{
					button.HorizontalTextAlignment = button.VerticalTextAlignment = alignment;
					var width = platform.Width;
					var height = platform.Height;
					platform.Measure(
						global::Android.Views.View.MeasureSpec.MakeMeasureSpec(width, MeasureSpecMode.Exactly),
						global::Android.Views.View.MeasureSpec.MakeMeasureSpec(height, MeasureSpecMode.Exactly));
					platform.Layout(0, 0, width, height);

					using var bitmap = global::Android.Graphics.Bitmap.CreateBitmap(width, height, global::Android.Graphics.Bitmap.Config.Argb8888!)
						?? throw new InvalidOperationException("Could not capture the Button.");
					using var canvas = new global::Android.Graphics.Canvas(bitmap);
					platform.Draw(canvas);
					var pixels = new int[width * height];
					bitmap.GetPixels(pixels, 0, width, 0, 0, width, height);
					var image = FindDrawnContent(pixels, width, height, 0xFF0000);
					var text = FindDrawnContent(pixels, width, height, 0x0000FF);
					var left = Math.Min(image.Left, text.Left);
					var top = Math.Min(image.Top, text.Top);
					var groupWidth = Math.Max(image.Right, text.Right) - left;
					var groupHeight = Math.Max(image.Bottom, text.Bottom) - top;
					var horizontal = rtl && alignment != TextAlignment.Center
						? alignment == TextAlignment.Start ? TextAlignment.End : TextAlignment.Start : alignment;
					var expectedLeft = platform.PaddingLeft + (width - platform.PaddingLeft - platform.PaddingRight - groupWidth) *
						(horizontal == TextAlignment.Start ? 0 : horizontal == TextAlignment.End ? 1 : 0.5);
					var expectedTop = platform.PaddingTop + (height - platform.PaddingTop - platform.PaddingBottom - groupHeight) *
						(alignment == TextAlignment.Start ? 0 : alignment == TextAlignment.End ? 1 : 0.5);
					// Glyph ink excludes the font's leading and descent, unlike native text layout bounds.
					Assert.InRange(Math.Abs(left - expectedLeft), 0, platform.TextSize);
					Assert.InRange(Math.Abs(top - expectedTop), 0, platform.TextSize);
					var gap = position switch
					{
						Button.ButtonContentLayout.ImagePosition.Top => text.Top - image.Bottom,
						Button.ButtonContentLayout.ImagePosition.Bottom => image.Top - text.Bottom,
						_ => (position == Button.ButtonContentLayout.ImagePosition.Left) != rtl
							? text.Left - image.Right : image.Left - text.Right
					};
					Assert.InRange(gap, platform.IconPadding, platform.IconPadding + platform.TextSize);
				}
			});
		}

		static Rect FindDrawnContent(int[] pixels, int width, int height, int rgb)
		{
			var left = width;
			var top = height;
			var right = -1;
			var bottom = -1;
			for (var y = 0; y < height; y++)
				for (var x = 0; x < width; x++)
					if ((pixels[y * width + x] & 0xFFFFFF) == rgb)
					{
						left = Math.Min(left, x);
						top = Math.Min(top, y);
						right = Math.Max(right, x);
						bottom = Math.Max(bottom, y);
					}
			Assert.True(right >= left && bottom >= top, $"Expected rendered color 0x{rgb:X6}.");
			return new Rect(left, top, right - left + 1, bottom - top + 1);
		}

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

		[Theory(DisplayName = "Button Icon has Correct Position"), Category(TestCategory.Layout)]
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
