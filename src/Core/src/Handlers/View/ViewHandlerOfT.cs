#nullable enable
#if __IOS__ || MACCATALYST
using NativeView = UIKit.UIView;
#elif MONOANDROID
using NativeView = Android.Views.View;
#elif WINDOWS
using NativeView = Microsoft.UI.Xaml.FrameworkElement;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
using NativeView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public abstract partial class ViewHandler<TVirtualView, TNativeView> : FrameworkElementHandler<TVirtualView, TNativeView>
		where TVirtualView : class, IView
#if !NETSTANDARD || IOS || ANDROID || WINDOWS
		where TNativeView : NativeView
#else
		where TNativeView : class
#endif
	{
		public static PropertyMapper<IView> ViewMapper = new(FrameworkElementMapper)
		{
			[nameof(IView.Visibility)] = MapVisibility,
			[nameof(IView.Width)] = MapWidth,
			[nameof(IView.Height)] = MapHeight,
		};

		internal ViewHandler(PropertyMapper mapper) : base(mapper)
		{ 
		}

		public static void MapVisibility(IFrameworkElementHandler handler, IView view)
		{
			((NativeView?)handler.NativeView)?.UpdateVisibility(view);
		}

		public static void MapWidth(IFrameworkElementHandler handler, IView view)
		{
			((NativeView?)handler.NativeView)?.UpdateWidth(view);
		}

		public static void MapHeight(IFrameworkElementHandler handler, IView view)
		{
			((NativeView?)handler.NativeView)?.UpdateHeight(view);
		}
	}
}