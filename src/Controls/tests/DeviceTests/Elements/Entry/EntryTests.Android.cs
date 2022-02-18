using System.Threading.Tasks;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class EntryTests
	{
		AppCompatEditText GetPlatformControl(EntryHandler handler) =>
			handler.PlatformView;

		Task<string> GetPlatformText(EntryHandler handler)
		{
			return InvokeOnMainThreadAsync(() => GetPlatformControl(handler).Text);
		}
	}
}
