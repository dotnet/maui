using UIKit;

namespace Xamarin.Platform
{
	public interface INativeViewHandler : IViewHandler
	{
		UIView? View { get; }
	}
}