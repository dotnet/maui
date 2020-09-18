using AppKit;

namespace Xamarin.Platform
{
	public interface INativeViewHandler : IViewHandler
	{
		NSView View { get; }
	}
}