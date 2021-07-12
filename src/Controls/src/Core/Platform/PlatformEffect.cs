using System;
#if __IOS__ || MACCATALYST
using NativeView = UIKit.UIView;
#elif MONOANDROID
using NativeView = Android.Views.View;
#elif WINDOWS
using NativeView = Microsoft.UI.Xaml.FrameworkElement;
#elif NETSTANDARD
using NativeView = System.Object;
#endif

namespace Microsoft.Maui.Controls.Platform
{
	public abstract class PlatformEffect : PlatformEffect<NativeView, NativeView>
	{
		internal override void SendAttached()
		{
			_ = Element ?? throw new InvalidOperationException("Element cannot be null here");
			Control = (NativeView)Element.Handler.NativeView;

			if (Element.Handler is IViewHandler vh)
				Container = (NativeView)(vh.ContainerView ?? vh.NativeView);
			else
				Container = Control;

			base.SendAttached();
		}
	}
}