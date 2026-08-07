#nullable disable
using Android.Graphics.Drawables;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using AToolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{

	public class ShellToolbarAppearanceTracker : IShellToolbarAppearanceTracker
	{
		bool _disposed;
		bool _originalAppearanceCaptured;
		Color _originalNativeTitleColor;
		IShellContext _shellContext;

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
			// SetColors already resolves the right M2/M3 default per-color internally when
			// passed null, so there's no need to branch on RuntimeFeature.IsMaterial3Enabled here.
			SetColors(toolbar, toolbarTracker, null, null, null);
		}

		protected virtual void SetColors(AToolbar toolbar, IShellToolbarTracker toolbarTracker, Color foreground, Color background, Color title)
		{
			if (_disposed)
				return;

			Toolbar shellToolbar = _shellContext?.Shell?.Toolbar;

			if (shellToolbar is null)
				return;

			var defaultBackground = RuntimeFeature.IsMaterial3Enabled ? null : ShellRenderer.DefaultBackgroundColor;
			var defaultForeground = RuntimeFeature.IsMaterial3Enabled ? null : ShellRenderer.DefaultForegroundColor;
			var defaultTitle = RuntimeFeature.IsMaterial3Enabled ? _originalNativeTitleColor : ShellRenderer.DefaultTitleColor;
			var barBackground = background ?? defaultBackground;
			shellToolbar.BarTextColor = title ?? defaultTitle;
			shellToolbar.BarBackground = barBackground is null ? null : new SolidColorBrush(barBackground);
			shellToolbar.IconColor = foreground ?? defaultForeground;

			// Only sync the toolbar's menu-item/hamburger tint on Material3. On Material2 this
			// must stay untouched: IShellToolbarTracker.TintColor has its own White fallback for
			// the default (no custom appearance) M2 case, and syncing it here would overwrite
			// that and unexpectedly darken overflow/hamburger icons.
			if (RuntimeFeature.IsMaterial3Enabled)
				toolbarTracker.TintColor = foreground;
		}

		// Shell.Toolbar.BarTextColor is a cross-platform Color, so unlike the TabLayout/BottomNavigationView
		// trackers (which can poke native ColorStateLists directly), a null title color here still has to
		// flow through ToolbarExtensions.UpdateBarTextColor. That method's own "no color set" fallback
		// queries an AppCompat Toolbar styleable that isn't Material3-aware and resolves to the wrong
		// (near-invisible) color under M3. Capturing the real M3 theme color once and using it as our
		// default keeps BarTextColor from ever being null, so that broken shared fallback path is never hit.
		internal void CaptureNativeTitleColor(AToolbar toolbar)
		{
			if (_originalAppearanceCaptured)
				return;

			var context = toolbar?.Context;
			if (context is not null)
				_originalNativeTitleColor = Color.FromInt(context.GetThemeAttrColor(Resource.Attribute.colorOnSurface));

			_originalAppearanceCaptured = true;
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