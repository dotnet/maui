#nullable enable
using Microsoft.Maui.Graphics;
namespace Microsoft.Maui
{
	public interface IFrameworkElementHandler // TODO ezhart IElementHandler? IHandler?
	{
		void SetMauiContext(IMauiContext mauiContext);
		void SetVirtualView(IFrameworkElement view); // TODO ezhart should match rename for VirtualView below (e.g., SetVirtualInstance)
		void UpdateValue(string property);
		void DisconnectHandler();
		object? NativeView { get; } // TODO ezhart The 'View' suffix doesn't fit anymore. NativeInstance? NativeObject?
		IFrameworkElement? VirtualView { get; } // TODO ezhart The 'View' suffix doesn't fit anymore. VirtualInstance? VirualObject?
												// I don't relish the idea of VirtualElement/NativeElement because once you change levels (to IViewHandler/IWidgetHandler)
												// the Element suffix makes less sense.
		IMauiContext? MauiContext { get; }
		bool HasContainer { get; set; }
		void NativeArrange(Rectangle frame);
		Size GetDesiredSize(double widthConstraint, double heightConstraint);
	}
}
