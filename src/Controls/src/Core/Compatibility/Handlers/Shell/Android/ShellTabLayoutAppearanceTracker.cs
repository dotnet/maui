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
		Drawable _originalIndicatorDrawable;
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
				// Derive the selected-state color from the captured native ColorStateList on demand,
				// using its own DefaultColor as the GetColorForState fallback (matches the pattern used
				// by ShellBottomNavViewAppearanceTracker.MakeColorStateList). CaptureNativeColors always
				// runs before SetColors on Material3, so _originalTextColors is guaranteed non-null here.
				var materialTitleArgb = title?.ToPlatform().ToArgb() ?? _originalTextColors.DefaultColor;
				var materialUnselectedArgb = unselected?.ToPlatform().ToArgb() ?? _originalTextColors.DefaultColor;

				tabLayout.SetTabTextColors(materialUnselectedArgb, materialTitleArgb);

				if (background is null)
				{
					tabLayout.SetBackground(_originalBackground);
				}
				else
				{
					tabLayout.SetBackground(new ColorDrawable(background.ToPlatform()));
				}

				if (foreground is not null)
				{
					tabLayout.SetSelectedTabIndicatorColor(foreground.ToPlatform());
				}
				else
				{
					tabLayout.SetSelectedTabIndicator(_originalIndicatorDrawable);
				}
			}
			else
			{
				var titleArgb = title.ToPlatform(ShellRenderer.DefaultTitleColor).ToArgb();
				var unselectedArgb = unselected.ToPlatform(ShellRenderer.DefaultUnselectedColor).ToArgb();
				tabLayout.SetTabTextColors(unselectedArgb, titleArgb);
				tabLayout.SetBackground(new ColorDrawable(background.ToPlatform(ShellRenderer.DefaultBackgroundColor)));
				tabLayout.SetSelectedTabIndicatorColor(foreground.ToPlatform(ShellRenderer.DefaultForegroundColor));
			}
		}

		internal void CaptureNativeColors(TabLayout tabLayout)
		{
			if (_originalAppearanceCaptured)
				return;

			_originalTextColors = tabLayout.TabTextColors;
			_originalBackground = tabLayout.Background;
			_originalIndicatorDrawable = tabLayout.TabSelectedIndicator;

			_originalAppearanceCaptured = true;
		}

		void RestoreNativeColors(TabLayout tabLayout)
		{
			if (!_originalAppearanceCaptured)
				return;

			tabLayout.SetTabTextColors(_originalTextColors.DefaultColor, _originalTextColors.DefaultColor);
			tabLayout.SetBackground(_originalBackground);
			tabLayout.SetSelectedTabIndicator(_originalIndicatorDrawable);
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
			_originalIndicatorDrawable = null;
			_shellContext = null;
		}

		#endregion IDisposable
	}
}