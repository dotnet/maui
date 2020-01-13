using Android.Content;
#if __ANDROID_29__
using AndroidX.Core.Widget;
using AndroidX.DrawerLayout.Widget;
using AndroidX.AppCompat.Widget;
#else
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
#endif

namespace Xamarin.Forms.Platform.Android
{
	public interface IShellContext
	{
		Context AndroidContext { get; }
		DrawerLayout CurrentDrawerLayout { get; }
		Shell Shell { get; }

		IShellObservableFragment CreateFragmentForPage(Page page);

		IShellFlyoutContentRenderer CreateShellFlyoutContentRenderer();

		IShellItemRenderer CreateShellItemRenderer(ShellItem shellItem);

		IShellSectionRenderer CreateShellSectionRenderer(ShellSection shellSection);

		IShellToolbarTracker CreateTrackerForToolbar(Toolbar toolbar);

		IShellToolbarAppearanceTracker CreateToolbarAppearanceTracker();

		IShellTabLayoutAppearanceTracker CreateTabLayoutAppearanceTracker(ShellSection shellSection);

		IShellBottomNavViewAppearanceTracker CreateBottomNavViewAppearanceTracker(ShellItem shellItem);
	}
}