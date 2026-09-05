#nullable disable
using Android.Content;
using Android.Views;
using AndroidX.Core.Content;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Graphics;
using AColor = Android.Graphics.Color;
using AColorRes = Android.Resource.Color;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	internal class ShellPageContainer : ViewGroup
	{
		static int? DarkBackground;
		static int? LightBackground;
		public IViewHandler Child { get; set; }

		public bool IsInFragment { get; set; }

		public ShellPageContainer(Context context, IPlatformViewHandler child, bool inFragment = false) : base(context)
		{
			Child = child;
			IsInFragment = inFragment;
			if (child.VirtualView.Background is null)
			{
				bool isDark = ShellRenderer.IsDarkTheme;

				int color = RuntimeFeature.IsMaterial3Enabled
				 ? GetMaterial3Background(context)
				 : GetResourceBackground(context, isDark);

				child.PlatformView.SetBackgroundColor(new AColor(color));
			}
			child.PlatformView.RemoveFromParent();
			AddView(child.PlatformView);
		}

		int GetMaterial3Background(Context context)
		{
			// Material 3 colorSurface automatically adapts to light/dark theme
			// The theme resolution happens in GetThemeAttrColor based on the active theme
			return ContextExtensions.GetThemeAttrColor(context, Resource.Attribute.colorSurface);
		}

		int GetResourceBackground(Context context, bool isDark)
		{
			int color;
			if (isDark)
			{
				color = DarkBackground ??= ContextCompat.GetColor(context, AColorRes.BackgroundDark);
			}
			else
			{
				color = LightBackground ??= ContextCompat.GetColor(context, AColorRes.BackgroundLight);
			}
			return color;
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			var width = r - l;
			var height = b - t;

			if (Child.PlatformView is AView aView)
				aView.Layout(0, 0, width, height);
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			if (Child.PlatformView is AView aView)
			{
				aView.Measure(widthMeasureSpec, heightMeasureSpec);
				SetMeasuredDimension(aView.MeasuredWidth, aView.MeasuredHeight);
			}
			else
				SetMeasuredDimension(0, 0);
		}
	}
}
