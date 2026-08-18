using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	internal interface ISafeAreaLayout
	{
		Size CrossPlatformArrange(Rect bounds, Thickness safeArea, bool delegateTopInset);
		void DisconnectSafeArea();
	}
}
