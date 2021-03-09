using UIKit;

namespace Microsoft.Maui
{
	public interface INativeViewHandler : IViewHandler
	{
		UIView? View { get; }
	}
}