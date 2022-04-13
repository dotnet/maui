#nullable enable
using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ButtonTests
	{
		Button GetPlatformButton(ButtonHandler buttonHandler) =>
			buttonHandler.PlatformView;

		Task<string?> GetPlatformText(ButtonHandler buttonHandler)
		{
			return InvokeOnMainThreadAsync(() => GetPlatformButton(buttonHandler).GetContent<TextBlock>()?.Text);
		}

		TextTrimming GetPlatformLineBreakMode(ButtonHandler buttonHandler) =>
			(GetPlatformButton(buttonHandler).Content as TextBlock)!.TextTrimming;
	}
}
