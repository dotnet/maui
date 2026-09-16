#nullable disable
using Android.Content.Res;
using Android.Graphics.Drawables;
using Google.Android.Material.Tabs;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using AColor = Android.Graphics.Color;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public class ShellTabLayoutAppearanceTracker : IShellTabLayoutAppearanceTracker
	{
		bool _disposed;
		bool _originalAppearanceCaptured;
		ColorStateList _originalTextColors;
		Drawable _originalBackground;
		int? _originalIndicatorColor;
		IShellContext _shellContext;

		public ShellTabLayoutAppearanceTracker(IShellContext shellContext)
		{
			_shellContext = shellContext;
		}

		public virtual void ResetAppearance(TabLayout tabLayout)
		{
			if (RuntimeFeature.IsMaterial3Enabled && _originalAppearanceCaptured)
			{
				RestoreNativeColors(tabLayout);
				AndroidSystemChrome.UpdateTopChrome(tabLayout, null);
			}
			else
			{
				SetColors(tabLayout, ShellRenderer.DefaultForegroundColor,
					ShellRenderer.DefaultBackgroundColor,
					ShellRenderer.DefaultTitleColor,
					ShellRenderer.DefaultUnselectedColor);
			}
		}

		public virtual void SetAppearance(TabLayout tabLayout, ShellAppearance appearance)
		{
			var foreground = appearance.ForegroundColor;
			var background = appearance.BackgroundColor;
			var titleColor = appearance.TitleColor;
			var unselectedColor = appearance.UnselectedColor;

			SetColors(tabLayout, foreground, background, titleColor, unselectedColor);
		}

		protected virtual void SetColors(TabLayout tabLayout, Color foreground, Color background, Color title, Color unselected)
		{
			if (RuntimeFeature.IsMaterial3Enabled && _originalAppearanceCaptured)
			{
				var titleArgb = title?.ToPlatform().ToArgb() ?? _originalTextColors.DefaultColor;
				var unselectedArgb = unselected?.ToPlatform().ToArgb() ?? _originalTextColors.DefaultColor;

				tabLayout.SetTabTextColors(unselectedArgb, titleArgb);

				if (background is null)
					tabLayout.SetBackground(_originalBackground);
				else
					tabLayout.SetBackground(new ColorDrawable(background.ToPlatform()));

				if (foreground is not null)
					tabLayout.SetSelectedTabIndicatorColor(foreground.ToPlatform());
				else if (_originalIndicatorColor is int originalColor)
					tabLayout.SetSelectedTabIndicatorColor(new AColor(originalColor));
			}
			else
			{
				var titleArgb = title.ToPlatform(ShellRenderer.DefaultTitleColor).ToArgb();
				var unselectedArgb = unselected.ToPlatform(ShellRenderer.DefaultUnselectedColor).ToArgb();

				tabLayout.SetTabTextColors(unselectedArgb, titleArgb);
				tabLayout.SetBackground(new ColorDrawable(background.ToPlatform(ShellRenderer.DefaultBackgroundColor)));
				tabLayout.SetSelectedTabIndicatorColor(foreground.ToPlatform(ShellRenderer.DefaultForegroundColor));
			}

			AndroidSystemChrome.UpdateTopChrome(
				tabLayout,
				background is null && RuntimeFeature.IsMaterial3Enabled && _originalAppearanceCaptured
					? null
					: new SolidColorBrush(background ?? ShellRenderer.DefaultBackgroundColor));
		}

		internal void CaptureNativeColors(TabLayout tabLayout)
		{
			if (_originalAppearanceCaptured)
				return;

			_originalTextColors = tabLayout.TabTextColors;
			_originalBackground = tabLayout.Background;
			_originalIndicatorColor = tabLayout.Context?.GetThemeAttrColor(Resource.Attribute.colorPrimary);
			_originalAppearanceCaptured = true;
		}

		void RestoreNativeColors(TabLayout tabLayout)
		{
			tabLayout.TabTextColors = _originalTextColors;
			tabLayout.SetBackground(_originalBackground);

			if (_originalIndicatorColor is int originalColor)
				tabLayout.SetSelectedTabIndicatorColor(new AColor(originalColor));
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
			_originalBackground = null;
			_originalTextColors = null;
			_originalIndicatorColor = null;
			_shellContext = null;
		}

		#endregion IDisposable
	}
}
