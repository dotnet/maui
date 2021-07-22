using Android.Content;
using Microsoft.Maui.Graphics;
using AView = Android.Views.View;
using AColor = Android.Graphics.Color;
using AColorRes = Android.Resource.Color;
using AndroidX.Core.Content;

namespace Microsoft.Maui.Controls.Platform
{
	internal class ShellPageContainer : PageContainer
	{
		public ShellPageContainer(Context context, INativeViewHandler child, bool inFragment = false) : base(context, child, inFragment)
		{
			if (child.VirtualView.Background == null)
			{
				var color = NativeVersion.IsAtLeast(23) ?
								Context.Resources.GetColor(AColorRes.BackgroundLight, Context.Theme) :
								new AColor(ContextCompat.GetColor(Context, AColorRes.BackgroundLight));

				child.NativeView.SetBackgroundColor(color);
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