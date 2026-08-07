#nullable disable
using System;
using Android.Graphics.Drawables;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Graphics;
using AToolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{

	public class ShellToolbarAppearanceTracker : IShellToolbarAppearanceTracker
	{
		bool _disposed;
		IShellContext _shellContext;

		public ShellToolbarAppearanceTracker(IShellContext shellContext)
		{
			_shellContext = shellContext;
		}

		public virtual void SetAppearance(AToolbar toolbar, IShellToolbarTracker toolbarTracker, ShellAppearance appearance)
		{
			if (appearance is null)
			{
				return;
			}

			var foreground = appearance.ForegroundColor;
			var background = !Brush.IsNullOrEmpty(appearance.Background)
				? appearance.Background
				: appearance.BackgroundColor is not null
					? new SolidColorBrush(appearance.BackgroundColor)
					: null;
			var titleColor = appearance.TitleColor;

			SetColors(toolbar, toolbarTracker, foreground, background, titleColor);
		}

		public virtual void ResetAppearance(AToolbar toolbar, IShellToolbarTracker toolbarTracker)
		{
			var context = _shellContext.AndroidContext;
			SetColors(
				toolbar,
				toolbarTracker,
				ShellRenderer.GetForegroundColor(context),
				new SolidColorBrush(ShellRenderer.GetBackgroundColor(context)),
				ShellRenderer.GetTitleColor(context));
		}

		protected virtual void SetColors(AToolbar toolbar, IShellToolbarTracker toolbarTracker, Color foreground, Brush background, Color title)
		{
			if (_disposed)
				return;

			Toolbar shellToolbar = _shellContext?.Shell?.Toolbar;

			if (shellToolbar is null)
				return;

			var context = _shellContext.AndroidContext;
			shellToolbar.BarTextColor = title ?? ShellRenderer.GetTitleColor(context);
			shellToolbar.BarBackground = background ?? new SolidColorBrush(ShellRenderer.GetBackgroundColor(context));
			shellToolbar.IconColor = foreground ?? ShellRenderer.GetForegroundColor(context);
		}

		[Obsolete("Use SetColors(AToolbar, IShellToolbarTracker, Color, Brush, Color) instead.")]
		protected virtual void SetColors(AToolbar toolbar, IShellToolbarTracker toolbarTracker, Color foreground, Color background, Color title)
		{
			SetColors(toolbar, toolbarTracker, foreground, background is not null ? new SolidColorBrush(background) : null, title);
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