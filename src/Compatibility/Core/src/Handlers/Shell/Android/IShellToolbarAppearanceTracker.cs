using System;
using AndroidX.AppCompat.Widget;
using AToolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public interface IShellToolbarAppearanceTracker : IDisposable
	{
		void SetAppearance(AToolbar toolbar, IShellToolbarTracker toolbarTracker, ShellAppearance appearance);
		void ResetAppearance(AToolbar toolbar, IShellToolbarTracker toolbarTracker);
	}
}