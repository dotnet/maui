#nullable enable
using Microsoft.Maui.Graphics;
namespace Microsoft.Maui
{
	public interface IViewHandler
	{
		void SetMauiContext(IMauiContext mauiContext);
		void SetVirtualView(IView view);
		void UpdateValue(string property);
		void DisconnectHandler();
		object? NativeView { get; }
		IView? VirtualView { get; }
		IMauiContext? MauiContext { get; }
		bool HasContainer { get; set; }
		object? ContainerView { get; }
		Size GetDesiredSize(double widthConstraint, double heightConstraint);
		void NativeArrange(Rectangle frame);
	}
}
