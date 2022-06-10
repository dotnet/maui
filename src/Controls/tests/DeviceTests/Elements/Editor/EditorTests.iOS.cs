using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Collection(HandlerTestBase.RunInNewWindowCollection)]
	public partial class EditorTests
	{
		MauiTextView GetPlatformControl(EditorHandler handler) =>
			handler.PlatformView;

		Task<string> GetPlatformText(EditorHandler handler)
		{
			return InvokeOnMainThreadAsync(() => GetPlatformControl(handler).Text);
		}
	}
}
