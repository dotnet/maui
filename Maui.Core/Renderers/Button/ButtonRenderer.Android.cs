using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;

#if __ANDROID_29__
using AndroidX.Core.View;
using AndroidX.AppCompat.Widget;
#else
using Android.Support.V4.View;
using Android.Support.V7.Widget;
#endif

using AColor = Android.Graphics.Color;
using AView = Android.Views.View;
//using AndroidX.AppCompat.Widget;
using AndroidX.Core.Content.Resources;
using static Android.Content.Res.Resources;
using Android;
using AndroidX.Core.Content;
using Android.Content.Res;

namespace System.Maui.Platform
{
	public partial class ButtonRenderer
	{
		protected override AppCompatButton CreateView()
		{
			var button = new AppCompatButton(Context);
			button.Click += Button_Click;
			return button;
		}

		//static ColorStateList ColorStateList;

		protected override void SetupDefaults()
		{
			var colors = TypedNativeView.TextColors;
			DefaultTextColor = new AColor(colors.DefaultColor).ToColor();
		}

		private void Button_Click(object sender, EventArgs e)
		{
			VirtualView.Clicked();
		}

		protected override void DisposeView(AppCompatButton nativeView)
		{
			nativeView.Click -= Button_Click;
			base.DisposeView(nativeView);
		}


		//public static void MapPropertyButtonFont(IViewRenderer renderer, IButton view)
		//{

		//}

		//public static void MapPropertyButtonInputTransparent(IViewRenderer renderer, IButton view)
		//{

		//}

		//public static void MapPropertyButtonCharacterSpacing(IViewRenderer renderer, IButton view)
		//{

		//}


	}
}
