#nullable enable
#if __IOS__ || MACCATALYST
using PlatformView = Microsoft.Maui.Platform.MauiSearchBar;
using QueryEditor = UIKit.UITextField;
#elif MONOANDROID
using PlatformView = AndroidX.AppCompat.Widget.SearchView;
using QueryEditor = Android.Widget.EditText;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.AutoSuggestBox;
using QueryEditor = Microsoft.UI.Xaml.Controls.AutoSuggestBox;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
using PlatformView = System.Object;
using QueryEditor = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial interface ISearchBarHandler : IViewHandler
	{
		new ISearchBar VirtualView { get; }
		new PlatformView PlatformView { get; }
		QueryEditor? QueryEditor { get; }
	}
}