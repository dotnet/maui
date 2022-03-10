#if __IOS__ || MACCATALYST
using PlatformView = UIKit.UIView;
#elif MONOANDROID
using PlatformView = AndroidX.RecyclerView.Widget.RecyclerView;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.ListViewBase;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial interface IItemsViewHandler : IViewHandler 
	{
		new ItemsView VirtualView { get; }
		new PlatformView PlatformView { get; }
	}
}
