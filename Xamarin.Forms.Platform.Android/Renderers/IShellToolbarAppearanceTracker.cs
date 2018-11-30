using Android.Support.Design.Widget;
using System;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Xamarin.Forms.Platform.Android
{
	public interface IShellToolbarAppearanceTracker : IDisposable
	{
		void SetAppearance(Toolbar toolbar, IShellToolbarTracker toolbarTracker, ShellAppearance appearance);
		void ResetAppearance(Toolbar toolbar, IShellToolbarTracker toolbarTracker);
	}
}