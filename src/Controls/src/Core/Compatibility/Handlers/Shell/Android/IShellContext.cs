using Android.Content;
using AndroidX.AppCompat.Widget;
using AndroidX.DrawerLayout.Widget;
using AToolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace Microsoft.Maui.Controls.Platform.Compatibility
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

		IShellToolbarTracker CreateTrackerForToolbar(AToolbar toolbar);

		IShellToolbarAppearanceTracker CreateToolbarAppearanceTracker();

		IShellTabLayoutAppearanceTracker CreateTabLayoutAppearanceTracker(ShellSection shellSection);

		IShellBottomNavViewAppearanceTracker CreateBottomNavViewAppearanceTracker(ShellItem shellItem);
	}
}