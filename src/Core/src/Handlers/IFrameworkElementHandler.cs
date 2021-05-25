#nullable enable
using Microsoft.Maui.Graphics;
namespace Microsoft.Maui
{
	public interface IFrameworkElementHandler
	{
		void SetMauiContext(IMauiContext mauiContext);
		void SetVirtualView(IFrameworkElement view);
		void UpdateValue(string property);
		void DisconnectHandler();
		object? NativeView { get; } // TODO ezhart NativeInstance? NativeObject?
		IFrameworkElement? VirtualView { get; }
		IMauiContext? MauiContext { get; }
		bool HasContainer { get; set; }
		void NativeArrange(Rectangle frame);
		Size GetDesiredSize(double widthConstraint, double heightConstraint);
	}
}
