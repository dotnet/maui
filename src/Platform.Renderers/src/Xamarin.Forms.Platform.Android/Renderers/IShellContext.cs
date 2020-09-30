using Android.Content;
using AndroidX.AppCompat.Widget;
using AndroidX.DrawerLayout.Widget;

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