using UIKit;

namespace Microsoft.Maui
{
	public interface INativeViewHandler : IViewHandler
	{
		new UIView? View { get; }
	}
}