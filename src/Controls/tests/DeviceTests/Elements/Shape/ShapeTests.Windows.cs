using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ShapeTests
	{
		Task PerformClick(IButton button)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var platformButton = GetNativeButton(CreateHandler<ButtonHandler>(button));
				var ap = new ButtonAutomationPeer(platformButton);
				var ip = ap.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
				ip?.Invoke();
			});

			UI.Xaml.Controls.Button GetNativeButton(ButtonHandler buttonHandler) =>
				buttonHandler.PlatformView;
		}
	}
}
