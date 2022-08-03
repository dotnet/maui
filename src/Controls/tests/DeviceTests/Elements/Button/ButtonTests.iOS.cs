#nullable enable
using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using UIKit;

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
	}
}
