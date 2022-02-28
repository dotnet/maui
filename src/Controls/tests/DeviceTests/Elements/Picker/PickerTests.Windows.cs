#nullable enable
using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.DeviceTests
{
	public partial class PickerTests
	{
		MauiComboBox GetPlatformControl(PickerHandler pickerHandler) =>
			pickerHandler.PlatformView;

		Task<string> GetPlatformText(PickerHandler pickerHandler)
		{
			return InvokeOnMainThreadAsync(() => GetPlatformControl(pickerHandler).Text);
		}
	}
}
