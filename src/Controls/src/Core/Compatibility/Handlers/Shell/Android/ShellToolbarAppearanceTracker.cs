#nullable disable
using Android.Content.Res;
using Android.Graphics.Drawables;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.ApplicationModel;
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
			if (_disposed)
				return;

			Toolbar shellToolbar = _shellContext?.Shell?.Toolbar;

			if (shellToolbar is null)
				return;

			var barBackground = background ?? ShellRenderer.DefaultBackgroundColor;
			var barTextColor = title ?? ShellRenderer.DefaultTitleColor;
			var iconColor = foreground ?? ShellRenderer.DefaultForegroundColor;

			shellToolbar.BarTextColor = barTextColor;
			shellToolbar.BarBackground = barBackground is null ? null : new SolidColorBrush(barBackground);
			shellToolbar.IconColor = iconColor;

			if (RuntimeFeature.IsMaterial3Enabled && (barBackground is null || barTextColor is null || iconColor is null))
				RefreshMaterial3Defaults(toolbar, barBackground is null, barTextColor is null, iconColor is null);
		}

		static void RefreshMaterial3Defaults(AToolbar toolbar, bool updateBackground, bool updateTitle, bool updateIcons)
		{
			if (toolbar.Context is null)
				return;

			var requestedTheme = Application.Current?.RequestedTheme ?? AppTheme.Unspecified;
			if (requestedTheme is not AppTheme.Light and not AppTheme.Dark)
				return;

			using var configuration = new Configuration(toolbar.Context.Resources.Configuration);
			configuration.UiMode = (configuration.UiMode & ~UiMode.NightMask) |
				(requestedTheme == AppTheme.Dark ? UiMode.NightYes : UiMode.NightNo);

			using var configurationContext = toolbar.Context.CreateConfigurationContext(configuration);
			using var themedContext = new MauiMaterialContextThemeWrapper(configurationContext);
			var surfaceColor = themedContext.GetThemeAttrColor(Resource.Attribute.colorSurface);
			var onSurfaceColor = themedContext.GetThemeAttrColor(Resource.Attribute.colorOnSurface);

			if (updateBackground)
				toolbar.SetBackgroundColor(new global::Android.Graphics.Color(surfaceColor));

			if (updateTitle)
				toolbar.SetTitleTextColor(onSurfaceColor);

			if (updateIcons)
			{
				toolbar.NavigationIcon?.SetTint(onSurfaceColor);
				toolbar.OverflowIcon?.SetTint(onSurfaceColor);
			}
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