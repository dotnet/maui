using Android.Content;
using AndroidX.AppCompat.Widget;
using AndroidX.DrawerLayout.Widget;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	public interface IShellContext : Microsoft.Maui.Controls.Platform.IShellContext
	{
		IShellFlyoutContentRenderer CreateShellFlyoutContentRenderer();

		IShellItemRenderer CreateShellItemRenderer(ShellItem shellItem);

		IShellSectionRenderer CreateShellSectionRenderer(ShellSection shellSection);
	}
}