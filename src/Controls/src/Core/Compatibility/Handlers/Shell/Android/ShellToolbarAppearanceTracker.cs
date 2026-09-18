#nullable disable
using Android.Content.Res;
using Android.Graphics.Drawables;
using AndroidX.AppCompat.Widget;
using Google.Android.Material.Shape;
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
		// Defaults to the M2 fallback so BarTextColor/BarBackground/IconColor are never null even if
		// a custom renderer never calls CaptureNativeColors; CaptureNativeColors overwrites these with
		// the real native values once it runs.
		Color _originalNativeTitleColor = ShellRenderer.DefaultTitleColor;
		Color _originalNativeBackgroundColor = ShellRenderer.DefaultBackgroundColor;
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
			if (RuntimeFeature.IsMaterial3Enabled)
			{
				RestoreNativeColors(toolbar, toolbarTracker);
			}
			else
			{
				SetColors(toolbar, toolbarTracker,
					ShellRenderer.DefaultForegroundColor,
					ShellRenderer.DefaultBackgroundColor,
					ShellRenderer.DefaultTitleColor);
			}
		}

		protected virtual void SetColors(AToolbar toolbar, IShellToolbarTracker toolbarTracker, Color foreground, Color background, Color title)
		{
			if (_disposed)
				return;

			Toolbar shellToolbar = _shellContext?.Shell?.Toolbar;

			if (shellToolbar is null)
				return;

			if (RuntimeFeature.IsMaterial3Enabled)
			{
				shellToolbar.BarTextColor = title ?? _originalNativeTitleColor;
				shellToolbar.BarBackground = new SolidColorBrush(background ?? _originalNativeBackgroundColor);
				shellToolbar.IconColor = foreground ?? _originalNativeTitleColor;
				toolbarTracker.TintColor = foreground;
			}
			else
			{
				shellToolbar.BarTextColor = title ?? ShellRenderer.DefaultTitleColor;
				shellToolbar.BarBackground = new SolidColorBrush(background ?? ShellRenderer.DefaultBackgroundColor);
				shellToolbar.IconColor = foreground ?? ShellRenderer.DefaultForegroundColor;
			}
		}

		void RestoreNativeColors(AToolbar toolbar, IShellToolbarTracker toolbarTracker)
		{
			if (_disposed)
				return;

			Toolbar shellToolbar = _shellContext?.Shell?.Toolbar;

			if (shellToolbar is null)
				return;

			shellToolbar.BarTextColor = _originalNativeTitleColor;
			shellToolbar.BarBackground = new SolidColorBrush(_originalNativeBackgroundColor);
			shellToolbar.IconColor = _originalNativeTitleColor;
			toolbarTracker.TintColor = _originalNativeTitleColor;

		}

		// Shell.Toolbar.BarTextColor is a cross-platform Color, so unlike the TabLayout/BottomNavigationView
		// trackers (which can poke native ColorStateLists directly), a null title color here still has to
		// flow through ToolbarExtensions.UpdateBarTextColor. That method's own "no color set" fallback
		// queries an AppCompat Toolbar styleable that isn't Material3-aware and resolves to the wrong
		// (near-invisible) color under M3. Capturing the real M3 theme color once and using it as our
		// default keeps BarTextColor from ever being null, so that broken shared fallback path is never hit.
		internal void CaptureNativeColors(AToolbar toolbar)
		{
			if (_originalAppearanceCaptured)
				return;

			var context = toolbar?.Context;
			if (context is not null)
			{
				_originalNativeTitleColor = Color.FromInt(context.GetThemeAttrColor(Resource.Attribute.colorOnSurface));
			}
			if (toolbar?.Background is MaterialShapeDrawable materialShapeDrawable && materialShapeDrawable.FillColor is ColorStateList fillColor)
			{
				_originalNativeBackgroundColor = Color.FromInt(fillColor.DefaultColor);
			}
			else if (toolbar?.Background is ColorDrawable colorDrawable)
			{
				// A freshly-inflated Toolbar commonly has a plain ColorDrawable background rather than
				// a MaterialShapeDrawable - without this branch the capture above silently "succeeds"
				// with a null color, and the once-only guard means it's never retried.
				_originalNativeBackgroundColor = Color.FromInt(colorDrawable.Color);
			}

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