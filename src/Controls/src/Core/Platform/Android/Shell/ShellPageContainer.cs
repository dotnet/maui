using Android.Content;
using AndroidX.Core.Content;
using Microsoft.Maui.Graphics;
using AColor = Android.Graphics.Color;
using AColorRes = Android.Resource.Color;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform
{
	internal class ShellPageContainer : PageContainer
	{
		static int? DarkBackground;
		static int? LightBackground;

		public ShellPageContainer(Context context, INativeViewHandler child, bool inFragment = false) : base(context, child, inFragment)
		{
			if (child.VirtualView.Background == null)
			{
				int color;
				if (ShellView.IsDarkTheme)
					color = DarkBackground ??= ContextCompat.GetColor(context, AColorRes.BackgroundDark);
				else
					color = LightBackground ??= ContextCompat.GetColor(context, AColorRes.BackgroundLight);

				child.NativeView.SetBackgroundColor(new AColor(color));
			}
		}




		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			var width = r - l;
			var height = b - t;

			if (changed && Child.NativeView is AView aView)
				aView.Layout(0, 0, width, height);
		}
	}
}