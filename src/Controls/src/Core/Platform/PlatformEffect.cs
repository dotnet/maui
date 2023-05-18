#nullable disable
using System;
#if __IOS__ || MACCATALYST
using PlatformView = UIKit.UIView;
#elif MONOANDROID
using PlatformView = Android.Views.View;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
#elif TIZEN
using PlatformView = Tizen.NUI.BaseComponents.View;
#elif (NETSTANDARD || !PLATFORM)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Controls.Platform
{
	public abstract class PlatformEffect : PlatformEffect<PlatformView, PlatformView>
	{
		internal override void SendAttached()
		{
			_ = Element ?? throw new InvalidOperationException("Element cannot be null here");
			Control = (PlatformView)Element.Handler.PlatformView;

			if (Element.Handler is IViewHandler vh)
				Container = (PlatformView)(vh.ContainerView ?? vh.PlatformView);
			else
				Container = Control;

			base.SendAttached();
		}
	}
}