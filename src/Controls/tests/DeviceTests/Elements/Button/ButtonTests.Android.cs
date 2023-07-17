#nullable enable
using System.Threading.Tasks;
using Android.Text;
using AndroidX.AppCompat.Widget;
using Google.Android.Material.Button;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ButtonTests
	{
		[Fact(DisplayName = "IconSize Initializes Correctly")]
		public async Task IconSizeInitializesCorrectly()
		{
			var button = new Button()
			{
				ImageSource = new FileImageSource { File = "red.png" },
				Text = "Test"
			};

			var handler = await CreateHandlerAsync<ButtonHandler>(button);

			await InvokeOnMainThreadAsync(async () =>
			{
				await handler.PlatformView.AttachAndRun(async () =>
				{
					bool iconLoaded = await AssertionExtensions.Wait(() => ButtonIconLoaded(handler));
					Assert.True(iconLoaded);

					var platformButtonSize = GetPlatformButtonSize(handler);
					Assert.True(platformButtonSize.Height > 0);

					var platformButtonIconSize = GetPlatformButtonIconSize(handler);
					Assert.True(platformButtonIconSize.Height > 0);

					Assert.True(platformButtonSize.Height > platformButtonIconSize.Height);
					Assert.True(platformButtonSize.Width > platformButtonIconSize.Width);
				});
			});
		}

		AppCompatButton GetPlatformButton(ButtonHandler buttonHandler) =>
			buttonHandler.PlatformView;

		Task<string?> GetPlatformText(ButtonHandler buttonHandler)
		{
			return InvokeOnMainThreadAsync(() => GetPlatformButton(buttonHandler).Text);
		}

		TextUtils.TruncateAt? GetPlatformLineBreakMode(ButtonHandler buttonHandler) =>
			GetPlatformButton(buttonHandler).Ellipsize;

		bool ButtonIconLoaded(ButtonHandler buttonHandler) =>
			buttonHandler.PlatformView.Icon != null;

		Size GetPlatformButtonSize(ButtonHandler buttonHandler)
		{
			var platformButton = GetPlatformButton(buttonHandler);

			return new Size(platformButton.Width, platformButton.Height);
		}

		Size GetPlatformButtonIconSize(ButtonHandler buttonHandler)
		{
			var platformButton = GetPlatformButton(buttonHandler);

			if(platformButton is MaterialButton materialButton)
			{
				return new Size(materialButton.IconSize);
			}

			return Size.Zero;
		}
	}
}