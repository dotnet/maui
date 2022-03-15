using Android.Graphics.Drawables;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Graphics;
using AToolbar = AndroidX.AppCompat.Widget.Toolbar;
using Microsoft.Maui.Controls.Handlers.Compatibility;

namespace Microsoft.Maui.Controls.Platform.Compatibility
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

		public virtual void SetAppearance(AToolbar toolbar, IShellToolbarTracker toolbarTracker, ShellAppearance appearance)
		{
			var foreground = appearance.ForegroundColor;
			var background = appearance.BackgroundColor;
			var titleColor = appearance.TitleColor;

			SetColors(toolbar, toolbarTracker, foreground, background, titleColor);
		}

		public virtual void ResetAppearance(AToolbar toolbar, IShellToolbarTracker toolbarTracker)
		{
			SetColors(toolbar, toolbarTracker, ShellRenderer.DefaultForegroundColor, ShellRenderer.DefaultBackgroundColor, ShellRenderer.DefaultTitleColor);
		}

		protected virtual void SetColors(AToolbar toolbar, IShellToolbarTracker toolbarTracker, Color foreground, Color background, Color title)
		{
			var titleArgb = title.ToPlatform(ShellRenderer.DefaultTitleColor).ToArgb();

			if (_titleTextColor != titleArgb)
			{
				toolbar.SetTitleTextColor(titleArgb);
				_titleTextColor = titleArgb;
			}

			var newColor = background.ToPlatform(ShellRenderer.DefaultBackgroundColor);
			if (!(toolbar.Background is ColorDrawable cd) || cd.Color != newColor)
			{
				using (var colorDrawable = new ColorDrawable(background.ToPlatform(ShellRenderer.DefaultBackgroundColor)))
					toolbar.SetBackground(colorDrawable);
			}

			var newTintColor = foreground ?? ShellRenderer.DefaultForegroundColor;

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