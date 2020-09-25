using Xamarin.Forms;

namespace Xamarin.Platform
{
	public interface IViewHandler
	{
		void SetView(IView view);
		void UpdateValue(string property);
		void TearDown();
		object? NativeView { get; }
		bool HasContainer { get; set; }
		SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint);
		void SetFrame(Rectangle frame);
	}
}