using System;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class BorderHandlerTests
	{
		ContentView GetNativeBorder(BorderHandler borderHandler) =>
			borderHandler.PlatformView;

		Task ValidateHasColor(IBorderView border, Color color, Action action = null)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeBorder = GetNativeBorder(CreateHandler(border));
				action?.Invoke();
				nativeBorder.AssertContainsColorAsync(color);
			});
		}
	}
}
