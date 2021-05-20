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
		object? NativeView { get; }
		IFrameworkElement? VirtualView { get; }
		IMauiContext? MauiContext { get; }

		// TODO ezhart This doesn't make sense for anything above IView, we might need another interface layer to add to ViewHandler
		bool HasContainer { get; set; }

		void NativeArrange(Rectangle frame);
		Size GetDesiredSize(double widthConstraint, double heightConstraint);
	}
}
