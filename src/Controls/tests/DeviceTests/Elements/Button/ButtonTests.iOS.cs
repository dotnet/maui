#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ButtonTests
	{
		UIButton GetPlatformButton(ButtonHandler buttonHandler) =>
			(UIButton)buttonHandler.PlatformView;

		Task<string> GetPlatformText(ButtonHandler buttonHandler)
		{
			return InvokeOnMainThreadAsync(() => GetPlatformButton(buttonHandler).CurrentTitle);
		}

		UILineBreakMode GetPlatformLineBreakMode(ButtonHandler buttonHandler) =>
			GetPlatformButton(buttonHandler).TitleLabel.LineBreakMode;

		[Fact("Clicked works after GC")]
		public async Task ClickedWorksAfterGC()
		{
			bool fired = false;
			var button = new Button();
			button.Clicked += (sender, e) => fired = true;
			var handler = await CreateHandlerAsync<ButtonHandler>(button);

			await Task.Yield();
			GC.Collect();
			GC.WaitForPendingFinalizers();

			await InvokeOnMainThreadAsync(() => handler.PlatformView.SendActionForControlEvents(UIControlEvent.TouchUpInside));
			Assert.True(fired, "Button.Clicked did not fire!");
		}
	}
}
