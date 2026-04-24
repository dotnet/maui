#nullable disable
using System;
using Android.Graphics.Drawables;
using Google.Android.Material.Tabs;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public class ShellTabLayoutAppearanceTracker : IShellTabLayoutAppearanceTracker
	{
		bool _disposed;
		IShellContext _shellContext;

		public ShellTabLayoutAppearanceTracker(IShellContext shellContext)
		{
			_shellContext = shellContext;
		}

		public virtual void ResetAppearance(TabLayout tabLayout)
		{
			SetColors(tabLayout, ShellRenderer.DefaultForegroundColor,
				ShellRenderer.DefaultBackgroundColor,
				ShellRenderer.DefaultTitleColor,
				ShellRenderer.DefaultUnselectedColor);
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
			var selectedTitleDefault = GetDefaultSelectedTopTabColor(ShellRenderer.DefaultTitleColor);
			var selectedIndicatorDefault = GetDefaultSelectedTopTabColor(ShellRenderer.DefaultForegroundColor);
			var selectedTitleColor = ResolveSelectedColor(title, ShellRenderer.DefaultTitleColor, selectedTitleDefault);
			var selectedIndicatorColor = ResolveSelectedColor(foreground, ShellRenderer.DefaultForegroundColor, selectedIndicatorDefault);

			var titleArgb = selectedTitleColor.ToPlatform(selectedTitleDefault).ToArgb();
			var unselectedArgb = unselected.ToPlatform(ShellRenderer.DefaultUnselectedColor).ToArgb();

			tabLayout.SetTabTextColors(unselectedArgb, titleArgb);
			tabLayout.SetBackground(new ColorDrawable(background.ToPlatform(ShellRenderer.DefaultBackgroundColor)));
			tabLayout.SetSelectedTabIndicatorColor(selectedIndicatorColor.ToPlatform(selectedIndicatorDefault));
		}

		static Color ResolveSelectedColor(Color value, Color legacyDefault, Color resolvedDefault)
		{
			if (!RuntimeFeature.IsMaterial3Enabled)
			{
				return value;
			}

			if (value is null || IsSameColor(value, legacyDefault))
			{
				return resolvedDefault;
			}

			return value;
		}

		static bool IsSameColor(Color first, Color second)
		{
			if (first is null || second is null)
			{
				return false;
			}

			const float tolerance = 0.001f;

			return Math.Abs(first.Red - second.Red) < tolerance
				&& Math.Abs(first.Green - second.Green) < tolerance
				&& Math.Abs(first.Blue - second.Blue) < tolerance
				&& Math.Abs(first.Alpha - second.Alpha) < tolerance;
		}

		static Color GetDefaultSelectedTopTabColor(Color material2Default)
		{
			if (RuntimeFeature.IsMaterial3Enabled)
			{
				return ShellRenderer.IsDarkTheme ? Color.FromArgb("#D0BCFF") : Color.FromArgb("#6750A4");
			}

			return material2Default;
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
			_shellContext = null;
		}

		#endregion IDisposable
	}
}