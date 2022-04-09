#nullable enable
using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ButtonTests
	{
		UIButton GetNativeButton(ButtonHandler buttonHandler) =>
			(UIButton)buttonHandler.PlatformView;

		Task<string> GetNativeText(ButtonHandler buttonHandler)
		{
			return InvokeOnMainThreadAsync(() => GetNativeButton(buttonHandler).CurrentTitle);
		}
	}
}
