using System.Threading.Tasks;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Collection(HandlerTestBase.RunInNewWindowCollection)]
	public partial class EditorTests
	{
		AppCompatEditText GetPlatformControl(EditorHandler handler) =>
			handler.PlatformView;

		Task<string> GetPlatformText(EditorHandler handler)
		{
			return InvokeOnMainThreadAsync(() => GetPlatformControl(handler).Text);
		}
	}
}
