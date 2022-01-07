#if __IOS__ || MACCATALYST
using NativeView = UIKit.UIView;
#elif MONOANDROID
using NativeView = Google.Android.Material.Swipe.MaterialSwipe;
#elif WINDOWS
using NativeView = Microsoft.UI.Xaml.FrameworkElement;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
using NativeView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class SwipeHandler : ISwipeViewHandler
	{
		public static IPropertyMapper<ISwipeView, ISwipeViewHandler> Mapper = new PropertyMapper<ISwipeView, ISwipeViewHandler>(ViewHandler.ViewMapper)
		{
		};

		public SwipeHandler() : base(Mapper)
		{

		}

		public SwipeHandler(IPropertyMapper? mapper = null) : base(mapper ?? Mapper)
		{
		}

		ISwipeView ISwipeViewHandler.TypedVirtualView => VirtualView;

		NativeView ISwipeViewHandler.TypedNativeView => NativeView;
	}
}