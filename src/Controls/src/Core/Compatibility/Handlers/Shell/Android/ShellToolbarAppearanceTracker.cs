#nullable disable
using System;
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
		Color _originalNativeTitleColor = ShellRenderer.DefaultTitleColor;
		Color _originalNativeBackgroundColor = ShellRenderer.DefaultBackgroundColor;
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
			if (RuntimeFeature.IsMaterial3Enabled && _originalAppearanceCaptured)
				RestoreNativeColors(toolbar, toolbarTracker);
			else
				SetColors(toolbar, toolbarTracker, ShellRenderer.DefaultForegroundColor, new SolidColorBrush(ShellRenderer.DefaultBackgroundColor), ShellRenderer.DefaultTitleColor);
		}

		protected virtual void SetColors(AToolbar toolbar, IShellToolbarTracker toolbarTracker, Color foreground, Brush background, Color title)
		{
			if (_disposed)
				return;

			Toolbar shellToolbar = _shellContext?.Shell?.Toolbar;

			if (shellToolbar is null)
				return;

			if (RuntimeFeature.IsMaterial3Enabled && _originalAppearanceCaptured)
			{
				shellToolbar.BarTextColor = title ?? _originalNativeTitleColor;
				shellToolbar.BarBackground = background ?? new SolidColorBrush(_originalNativeBackgroundColor);
				shellToolbar.IconColor = foreground ?? _originalNativeTitleColor;
				toolbarTracker.TintColor = foreground;
			}
			else
			{
				shellToolbar.BarTextColor = title ?? ShellRenderer.DefaultTitleColor;
				shellToolbar.BarBackground = background ?? new SolidColorBrush(ShellRenderer.DefaultBackgroundColor);
				shellToolbar.IconColor = foreground ?? ShellRenderer.DefaultForegroundColor;
			}

			AndroidSystemChrome.UpdateTopChrome(
				toolbar,
				RuntimeFeature.IsMaterial3Enabled && _originalAppearanceCaptured && background is null
					? null
					: shellToolbar.BarBackground);
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
			AndroidSystemChrome.UpdateTopChrome(toolbar, null);
		}

		internal void CaptureNativeColors(AToolbar toolbar)
		{
			if (_originalAppearanceCaptured)
				return;

			var context = toolbar?.Context;
			if (context is not null)
				_originalNativeTitleColor = Color.FromInt(context.GetThemeAttrColor(Resource.Attribute.colorOnSurface));

			if (toolbar?.Background is MaterialShapeDrawable materialShapeDrawable && materialShapeDrawable.FillColor is ColorStateList fillColor)
				_originalNativeBackgroundColor = Color.FromInt(fillColor.DefaultColor);
			else if (toolbar?.Background is ColorDrawable colorDrawable)
				_originalNativeBackgroundColor = Color.FromInt(colorDrawable.Color);

			_originalAppearanceCaptured = true;
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
