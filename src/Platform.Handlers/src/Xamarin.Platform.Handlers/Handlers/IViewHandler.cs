using Xamarin.Forms;

namespace Xamarin.Platform
{
	public interface IViewHandler
	{
		void SetVirtualView(IView view);
		void UpdateValue(string property);
		void DisconnectHandler();
		object? NativeView { get; }
		bool HasContainer { get; set; }
		SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint);
		void SetFrame(Rectangle frame);
	}
}