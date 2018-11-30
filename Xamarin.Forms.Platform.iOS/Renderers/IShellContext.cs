using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public interface IShellContext
	{
		bool AllowFlyoutGesture { get; }

		IShellItemRenderer CurrentShellItemRenderer { get; }

		Shell Shell { get; }

		IShellPageRendererTracker CreatePageRendererTracker();

		IShellFlyoutContentRenderer CreateShellFlyoutContentRenderer();

		IShellSectionRenderer CreateShellSectionRenderer(ShellSection shellSection);

		IShellNavBarAppearanceTracker CreateNavBarAppearanceTracker();

		IShellTabBarAppearanceTracker CreateTabBarAppearanceTracker();

		IShellSearchResultsRenderer CreateShellSearchResultsRenderer();
	}
}