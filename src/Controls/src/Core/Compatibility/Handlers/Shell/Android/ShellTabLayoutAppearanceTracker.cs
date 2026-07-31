#nullable disable
using Android.Content.Res;
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
		ColorStateList _defaultTextColors;
		Drawable _defaultBackground;
		Drawable.ConstantState _defaultBackgroundState;
		Drawable _defaultSelectedIndicator;
		Drawable.ConstantState _defaultSelectedIndicatorState;
		bool _capturedDefaults;

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
			CaptureDefaults(tabLayout);

			var titleColor = title ?? ShellRenderer.DefaultTitleColor;
			var titleArgb = titleColor?.ToPlatform().ToArgb()
				?? _defaultTextColors?.GetColorForState(new[] { R.Attribute.StateSelected }, new global::Android.Graphics.Color(_defaultTextColors.DefaultColor))
				?? global::Android.Graphics.Color.Transparent.ToArgb();
			var unselectedArgb = unselected.ToPlatform(ShellRenderer.DefaultUnselectedColor).ToArgb();

			tabLayout.SetTabTextColors(unselectedArgb, titleArgb);

			var backgroundColor = background ?? ShellRenderer.DefaultBackgroundColor;
			tabLayout.SetBackground(backgroundColor is null
				? CreateDrawable(tabLayout, _defaultBackgroundState, _defaultBackground)
				: new ColorDrawable(backgroundColor.ToPlatform()));

			var indicatorColor = foreground ?? ShellRenderer.DefaultForegroundColor;
			if (indicatorColor is null)
				tabLayout.SetSelectedTabIndicator(CreateDrawable(tabLayout, _defaultSelectedIndicatorState, _defaultSelectedIndicator));
			else
				tabLayout.SetSelectedTabIndicatorColor(indicatorColor.ToPlatform());
		}

		void CaptureDefaults(TabLayout tabLayout)
		{
			if (_capturedDefaults)
				return;

			_capturedDefaults = true;
			_defaultTextColors = tabLayout.TabTextColors;
			_defaultBackground = tabLayout.Background;
			_defaultBackgroundState = _defaultBackground?.GetConstantState();
			_defaultSelectedIndicator = tabLayout.TabSelectedIndicator;
			_defaultSelectedIndicatorState = _defaultSelectedIndicator?.GetConstantState();
		}

		static Drawable CreateDrawable(TabLayout tabLayout, Drawable.ConstantState state, Drawable fallback) =>
			state?.NewDrawable(tabLayout.Resources)?.Mutate() ?? fallback?.Mutate();

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