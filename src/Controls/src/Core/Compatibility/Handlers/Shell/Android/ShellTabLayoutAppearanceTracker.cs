#nullable disable
using Android.Content.Res;
using Android.Graphics.Drawables;
using Google.Android.Material.Tabs;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using R = Android.Resource;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public class ShellTabLayoutAppearanceTracker : IShellTabLayoutAppearanceTracker
	{
		bool _disposed;
		bool _originalAppearanceCaptured;
		ColorStateList _originalTextColors;
		Drawable _originalBackground;
		int? _originalIndicatorColor;
		int? _originalSelectedTextColorArgb;
		int _originalNativeTextColor;
		IShellContext _shellContext;

		public ShellTabLayoutAppearanceTracker(IShellContext shellContext)
		{
			_shellContext = shellContext;
		}

		public virtual void ResetAppearance(TabLayout tabLayout)
		{
			if (RuntimeFeature.IsMaterial3Enabled)
			{
				RestoreNativeColors(tabLayout);
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
			if (RuntimeFeature.IsMaterial3Enabled)
			{
				var materialTitleArgb = title?.ToPlatform().ToArgb() ?? _originalSelectedTextColorArgb ?? _originalNativeTextColor;
				var materialUnselectedArgb = unselected?.ToPlatform().ToArgb() ?? _originalTextColors?.DefaultColor ?? _originalNativeTextColor;

				tabLayout.SetTabTextColors(materialUnselectedArgb, materialTitleArgb);

				if (background is null)
					tabLayout.SetBackground(_originalBackground);
				else
					tabLayout.SetBackground(new ColorDrawable(background.ToPlatform()));

				if (foreground is not null)
					tabLayout.SetSelectedTabIndicatorColor(foreground.ToPlatform());
				else if (_originalIndicatorColor is int indicatorColor)
					tabLayout.SetSelectedTabIndicatorColor(indicatorColor);

				return;
			}

			var titleArgb = title.ToPlatform(ShellRenderer.DefaultTitleColor).ToArgb();
			var unselectedArgb = unselected.ToPlatform(ShellRenderer.DefaultUnselectedColor).ToArgb();

			tabLayout.SetTabTextColors(unselectedArgb, titleArgb);
			tabLayout.SetBackground(new ColorDrawable(background.ToPlatform(ShellRenderer.DefaultBackgroundColor)));
			tabLayout.SetSelectedTabIndicatorColor(foreground.ToPlatform(ShellRenderer.DefaultForegroundColor));
		}

		internal void CaptureNativeColors(TabLayout tabLayout)
		{
			if (_originalAppearanceCaptured)
				return;

			_originalTextColors = tabLayout.TabTextColors;
			_originalBackground = tabLayout.Background;
			_originalNativeTextColor = tabLayout.Context.GetThemeAttrColor(Resource.Attribute.colorOnSurface);

			// Pre-resolve the selected-state text color once. Its fallback (DefaultColor) is
			// purely derived from the captured native ColorStateList, so it's safe to compute
			// here instead of re-deriving it on every SetColors call.
			_originalSelectedTextColorArgb = _originalTextColors?.GetColorForState(
				new[] { R.Attribute.StateSelected }, new global::Android.Graphics.Color(_originalTextColors.DefaultColor));

			using var styledAttributes = tabLayout.Context.Theme.ObtainStyledAttributes(
				null,
				Resource.Styleable.TabLayout,
				Resource.Attribute.tabStyle,
				0);

			if (styledAttributes.HasValue(Resource.Styleable.TabLayout_tabIndicatorColor))
				_originalIndicatorColor = styledAttributes.GetColor(Resource.Styleable.TabLayout_tabIndicatorColor, 0);
			else
				_originalIndicatorColor = tabLayout.Context.GetThemeAttrColor(Resource.Attribute.colorPrimary);

			_originalAppearanceCaptured = true;
		}

		void RestoreNativeColors(TabLayout tabLayout)
		{
			if (!_originalAppearanceCaptured)
				return;

			tabLayout.TabTextColors = _originalTextColors;
			tabLayout.SetBackground(_originalBackground);

			if (_originalIndicatorColor is int indicatorColor)
				tabLayout.SetSelectedTabIndicatorColor(indicatorColor);
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
			_originalSelectedTextColorArgb = null;
			_shellContext = null;
		}

		#endregion IDisposable
	}
}