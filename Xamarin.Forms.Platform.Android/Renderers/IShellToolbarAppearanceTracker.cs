using System;
using AndroidX.AppCompat.Widget;

namespace Xamarin.Forms.Platform.Android
{
	public interface IShellToolbarAppearanceTracker : IDisposable
	{
		void SetAppearance(Toolbar toolbar, IShellToolbarTracker toolbarTracker, ShellAppearance appearance);
		void ResetAppearance(Toolbar toolbar, IShellToolbarTracker toolbarTracker);
	}
}