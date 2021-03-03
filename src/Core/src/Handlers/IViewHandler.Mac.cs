using AppKit;

namespace Microsoft.Maui
{
	public interface INativeViewHandler : IViewHandler
	{
		NSView? View { get; }
	}
}