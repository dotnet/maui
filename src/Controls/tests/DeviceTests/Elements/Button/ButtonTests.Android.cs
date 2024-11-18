#nullable enable
using System;
using System.Threading.Tasks;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Widget;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
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

		Android.Text.TextUtils.TruncateAt? GetPlatformLineBreakMode(ButtonHandler buttonHandler) =>
			GetPlatformButton(buttonHandler).Ellipsize;


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
				var drawables = TextViewCompat.GetCompoundDrawablesRelative(platformButton);
				Assert.NotNull(drawables[matchingDrawableIndex]);
			});
		}

	}
}
