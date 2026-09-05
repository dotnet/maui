using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	internal interface ISafeAreaLayout
	{
		Size CrossPlatformArrange(Rect bounds, Thickness safeArea, bool delegateTopInset);
		void DisconnectSafeArea();
	}

#if IOS
	internal interface ISafeAreaScrollView
	{
		void ApplyDelegatedTopInset(double topInset);
		void ResetDelegatedTopInset();
	}

	internal interface ISafeAreaScrollViewContainer
	{
		void ApplyDelegatedFrame(Rect safeFrame, Rect delegatedFrame);
		void ResetDelegatedFrame();
	}
#endif
}
