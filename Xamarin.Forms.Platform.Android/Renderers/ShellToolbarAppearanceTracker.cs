using Android.Graphics.Drawables;
#if __ANDROID_29__
using AndroidX.AppCompat.Widget;
#else
using Android.Support.V7.Widget;
#endif

namespace Xamarin.Forms.Platform.Android
{

	public class ShellToolbarAppearanceTracker : IShellToolbarAppearanceTracker
	{
		bool _disposed;
		IShellContext _shellContext;

		public ShellToolbarAppearanceTracker(IShellContext shellContext)
		{
			_shellContext = shellContext;
		}

		public virtual void SetAppearance(Toolbar toolbar, IShellToolbarTracker toolbarTracker, ShellAppearance appearance)
		{
			var foreground = appearance.ForegroundColor;
			var background = appearance.BackgroundColor;
			var titleColor = appearance.TitleColor;

			SetColors(toolbar, toolbarTracker, foreground, background, titleColor);
		}

		public virtual void ResetAppearance(Toolbar toolbar, IShellToolbarTracker toolbarTracker)
		{
			SetColors(toolbar, toolbarTracker, ShellRenderer.DefaultForegroundColor, ShellRenderer.DefaultBackgroundColor, ShellRenderer.DefaultTitleColor);
		}

		protected virtual void SetColors(Toolbar toolbar, IShellToolbarTracker toolbarTracker, Color foreground, Color background, Color title)
		{
			var titleArgb = title.ToAndroid(ShellRenderer.DefaultTitleColor).ToArgb();

			toolbar.SetTitleTextColor(titleArgb);
			using (var colorDrawable = new ColorDrawable(background.ToAndroid(ShellRenderer.DefaultBackgroundColor)))
				toolbar.SetBackground(colorDrawable);
			toolbarTracker.TintColor = foreground.IsDefault ? ShellRenderer.DefaultForegroundColor : foreground;
		}

		#region IDisposable

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				_shellContext = null;
			}
		}

		#endregion IDisposable
	}
}