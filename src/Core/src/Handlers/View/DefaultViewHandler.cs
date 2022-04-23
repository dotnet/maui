using System;

#if IOS || MACCATALYST
using PlatformView = UIKit.UIView;
using ConcreteView = UIKit.UIView;
#elif MONOANDROID
using PlatformView = Android.Views.View;
using ConcreteView = Android.Views.View;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
using ConcreteView = Microsoft.UI.Xaml.Controls.Border;
#elif TIZEN
using PlatformView = ElmSharp.EvasObject;
using ConcreteView = Tizen.UIExtensions.ElmSharp.Canvas;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
using ConcreteView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public class DefaultViewHandler : ViewHandler<IView, PlatformView>
	{
		protected override PlatformView CreatePlatformView() =>
#if ANDROID
			new ConcreteView(Context);
#elif TIZEN
			new ConcreteView(NativeParent ?? throw new InvalidOperationException($"{nameof(NativeParent)} cannot be null"));
#else
			new ConcreteView();
#endif
	}
}