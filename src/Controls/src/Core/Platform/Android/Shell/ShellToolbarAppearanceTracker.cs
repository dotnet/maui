using Android.Graphics.Drawables;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Platform
{

	public class ShellToolbarAppearanceTracker : IShellToolbarAppearanceTracker
	{
		bool _disposed;
		IShellContext _shellContext;
		int _titleTextColor = -1;

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
			SetColors(toolbar, toolbarTracker, ShellView.DefaultForegroundColor, ShellView.DefaultBackgroundColor, ShellView.DefaultTitleColor);
		}

		protected virtual void SetColors(Toolbar toolbar, IShellToolbarTracker toolbarTracker, Color foreground, Color background, Color title)
		{
			var titleArgb = title.ToNative(ShellView.DefaultTitleColor).ToArgb();

			if (_titleTextColor != titleArgb)
			{
				toolbar.SetTitleTextColor(titleArgb);
				_titleTextColor = titleArgb;
			}

			var newColor = background.ToNative(ShellView.DefaultBackgroundColor);
			if (!(toolbar.Background is ColorDrawable cd) || cd.Color != newColor)
			{
				using (var colorDrawable = new ColorDrawable(background.ToNative(ShellView.DefaultBackgroundColor)))
					toolbar.SetBackground(colorDrawable);
			}

			var newTintColor = foreground ?? ShellView.DefaultForegroundColor;

			if (toolbarTracker.TintColor != newTintColor)
				toolbarTracker.TintColor = newTintColor;
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