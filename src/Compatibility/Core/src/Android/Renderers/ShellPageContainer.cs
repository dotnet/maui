using Android.Content;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	internal class ShellPageContainer : PageContainer
	{
		public ShellPageContainer(Context context, IVisualElementRenderer child, bool inFragment = false) : base(context, child, inFragment)
		{
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