#nullable enable
using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ButtonTests
	{
		Button GetNativeButton(ButtonHandler buttonHandler) =>
			buttonHandler.PlatformView;

		Task<string?> GetNativeText(ButtonHandler buttonHandler)
		{
			return InvokeOnMainThreadAsync(() => GetNativeButton(buttonHandler).GetContent<TextBlock>()?.Text);
		}
	}
}
