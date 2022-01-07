using System.Threading.Tasks;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Views;
using Microsoft.Maui.Graphics;
using AView = Android.Views.View;

namespace Microsoft.Maui.Handlers
{
	public partial class SwipeHandler : ViewHandler<ISwipeView, MauiSwipeView>
	{
		protected override MauiSwipeView CreateNativeView()
		{
			return new MauiSwipeView(Context);
		}



		public override void SetVirtualView(IView view)
		{
			NativeView.Element = (ISwipeView)view;
			base.SetVirtualView(view);
		}
	}
}