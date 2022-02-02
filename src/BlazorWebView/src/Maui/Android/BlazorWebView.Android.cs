using Android.Content;
using Android.Views;
using Android.Webkit;
using AWebView = Android.Webkit.WebView;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	public class BlazorAndroidWebView : AWebView
	{
		public BlazorAndroidWebView(Context context) : base(context)
		{
		}

		public override bool OnKeyDown(Keycode keyCode, KeyEvent? e)
		{
			if (keyCode == Keycode.Back && CanGoBack() && e?.RepeatCount == 0)
			{
				GoBack();
				return true;
			}
			return false;
		}
	}
}
