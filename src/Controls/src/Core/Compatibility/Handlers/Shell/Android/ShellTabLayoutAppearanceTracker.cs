#nullable disable
using Android.Graphics.Drawables;
using Google.Android.Material.Tabs;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Graphics;
using R = Android.Resource;

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
			if (RuntimeFeature.IsMaterial3Enabled)
			{
				if (ShellRenderer.DefaultTitleColor is not null && ShellRenderer.DefaultUnselectedColor is not null)
				{
					var materialTitleArgb = title.ToPlatform(ShellRenderer.DefaultTitleColor).ToArgb();
					var materialUnselectedArgb = unselected.ToPlatform(ShellRenderer.DefaultUnselectedColor).ToArgb();
					tabLayout.SetTabTextColors(materialUnselectedArgb, materialTitleArgb);
				}
				else if (tabLayout.TabTextColors is { } nativeTextColors)
				{
					var materialTitleArgb = title?.ToPlatform().ToArgb()
						?? nativeTextColors.GetColorForState(new[] { R.Attribute.StateSelected }, new global::Android.Graphics.Color(nativeTextColors.DefaultColor));
					var materialUnselectedArgb = unselected?.ToPlatform().ToArgb() ?? nativeTextColors.DefaultColor;
					tabLayout.SetTabTextColors(materialUnselectedArgb, materialTitleArgb);
				}

				if (background is not null || ShellRenderer.DefaultBackgroundColor is not null)
					tabLayout.SetBackground(new ColorDrawable(background.ToPlatform(ShellRenderer.DefaultBackgroundColor)));

				if (foreground is not null || ShellRenderer.DefaultForegroundColor is not null)
					tabLayout.SetSelectedTabIndicatorColor(foreground.ToPlatform(ShellRenderer.DefaultForegroundColor));

				return;
			}

			var titleArgb = title.ToPlatform(ShellRenderer.DefaultTitleColor).ToArgb();
			var unselectedArgb = unselected.ToPlatform(ShellRenderer.DefaultUnselectedColor).ToArgb();

			tabLayout.SetTabTextColors(unselectedArgb, titleArgb);
			tabLayout.SetBackground(new ColorDrawable(background.ToPlatform(ShellRenderer.DefaultBackgroundColor)));
			tabLayout.SetSelectedTabIndicatorColor(foreground.ToPlatform(ShellRenderer.DefaultForegroundColor));
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