using Android.Content;
using AndroidX.AppCompat.Widget;
using AndroidX.DrawerLayout.Widget;

namespace Microsoft.Maui.Controls.Platform
{
	public interface IShellContext
	{
		Context AndroidContext { get; }
		DrawerLayout CurrentDrawerLayout { get; }
		Shell Shell { get; }

		IShellObservableFragment CreateFragmentForPage(Page page);

		IShellFlyoutContentView CreateShellFlyoutContentView();

		IShellItemView CreateShellItemView(ShellItem shellItem);

		IShellSectionView CreateShellSectionView(ShellSection shellSection);

		IShellToolbarTracker CreateTrackerForToolbar(Toolbar toolbar);

		IShellToolbarAppearanceTracker CreateToolbarAppearanceTracker();

		IShellTabLayoutAppearanceTracker CreateTabLayoutAppearanceTracker(ShellSection shellSection);

		IShellBottomNavViewAppearanceTracker CreateBottomNavViewAppearanceTracker(ShellItem shellItem);
	}
}