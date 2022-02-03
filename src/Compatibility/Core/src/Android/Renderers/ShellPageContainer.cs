using Android.Content;
using AndroidX.Core.Content;
using Microsoft.Maui.Graphics;
using AColor = Android.Graphics.Color;
using AColorRes = Android.Resource.Color;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	internal class ShellPageContainer : PageContainer
	{
		public ShellPageContainer(Context context, IVisualElementRenderer child, bool inFragment = false) : base(context, child, inFragment)
		{
			if (child.Element.Handler is IPlatformViewHandler nvh &&
				nvh.VirtualView.Background == null)
			{
				var color = NativeVersion.IsAtLeast(23) ?
								Context.Resources.GetColor(AColorRes.BackgroundLight, Context.Theme) :
								new AColor(ContextCompat.GetColor(Context, AColorRes.BackgroundLight));

				nvh.PlatformView.SetBackgroundColor(color);
			}
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			var width = Context.FromPixels(r - l);
			var height = Context.FromPixels(b - t);
			Child.Element.Layout(new Rectangle(0, 0, width, height));
			base.OnLayout(changed, l, t, r, b);
		}
	}
}