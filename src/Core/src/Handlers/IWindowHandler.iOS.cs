using UIKit;

namespace Microsoft.Maui
{
	public interface INativeWindowHandler : IWindowHandler
	{
		void SetWindow(UIWindow window);
	}
}