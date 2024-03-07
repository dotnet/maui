using PlatformView = Microsoft.Maui.Platform.NotImplementedView;

namespace Microsoft.Maui.Handlers;

public partial class TabbedViewHandler
{
	protected override PlatformView CreatePlatformView()
	{
		return new(nameof(ITabbedView));
	}
}