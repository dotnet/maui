using Microsoft.Maui.Graphics;
using Microsoft.UI.Windowing;

using PlatformWindow = Microsoft.UI.Xaml.Window;

namespace Microsoft.Maui.Embedding;

internal partial class EmbeddedWindowHandler
{
	protected override void ConnectHandler(PlatformWindow platformView)
	{
		base.ConnectHandler(platformView);
	}

	protected override void DisconnectHandler(PlatformWindow platformView)
	{
		base.DisconnectHandler(platformView);
	}
}
