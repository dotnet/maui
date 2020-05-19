using Android.Graphics.Drawables;
#if __ANDROID_29__
using Google.Android.Material.Tabs;
#else
using Android.Support.Design.Widget;
#endif

namespace Xamarin.Forms.Platform.Android
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
			var titleArgb = title.ToAndroid(ShellRenderer.DefaultTitleColor).ToArgb();
			var unselectedArgb = unselected.ToAndroid(ShellRenderer.DefaultUnselectedColor).ToArgb();

			tabLayout.SetTabTextColors(unselectedArgb, titleArgb);
			tabLayout.SetBackground(new ColorDrawable(background.ToAndroid(ShellRenderer.DefaultBackgroundColor)));
			tabLayout.SetSelectedTabIndicatorColor(foreground.ToAndroid(ShellRenderer.DefaultForegroundColor));
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