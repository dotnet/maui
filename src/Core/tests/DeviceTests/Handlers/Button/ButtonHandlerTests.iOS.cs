using Microsoft.Maui.Handlers;
using System.Threading.Tasks;
using UIKit;
using Microsoft.Maui;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ButtonHandlerTests
	{
		UIButton GetNativeButton(ButtonHandler buttonHandler) =>
			(UIButton)buttonHandler.View;

		string GetNativeText(ButtonHandler buttonHandler) =>
			GetNativeButton(buttonHandler).CurrentTitle;

		Color GetNativeTextColor(ButtonHandler buttonHandler) =>
			GetNativeButton(buttonHandler).CurrentTitleColor.ToColor();

		Task PerformClick(IButton button)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				GetNativeButton(CreateHandler(button)).SendActionForControlEvents(UIControlEvent.TouchUpInside);
			});
		}
	}
}