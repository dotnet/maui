
using Xamarin.Forms;

namespace System.Maui {
	public interface IMauiRenderer
	{
		//void SetView (IFrameworkElement view);
		//void UpdateValue (string property);
		//void Remove (IFrameworkElement view);
		object NativeView { get; }
		//bool HasContainer { get; set; } 
		//SizeRequest GetDesiredSize (double widthConstraint, double heightConstraint);
		//void SetFrame (Rectangle frame);
	}
}