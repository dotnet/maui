using AppKit;

namespace System.Maui
{
	public interface INativeViewRenderer : IViewRenderer
	{
		NSView View { get; }
	}
}
