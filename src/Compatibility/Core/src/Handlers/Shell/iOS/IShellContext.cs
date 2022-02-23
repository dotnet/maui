using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Platform.Compatibility
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