using Android.Content;
using Android.Views;
using AWebView = Android.Webkit.WebView;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	/// <summary>
	/// A Blazor Web View implemented using <see cref="AWebView"/>.
	/// </summary>
	internal class BlazorAndroidWebView : AWebView
	{
		internal bool BackNavigationHandled { get; set; }

		/// <summary>
		/// Initializes a new instance of <see cref="BlazorAndroidWebView"/>
		/// </summary>
		/// <param name="context">The <see cref="Context"/>.</param>
		public BlazorAndroidWebView(Context context) : base(context)
		{
		}

		public override bool OnKeyDown(Keycode keyCode, KeyEvent? e)
		{
			if (keyCode == Keycode.Back && CanGoBack() && e?.RepeatCount == 0)
			{
				GoBack();
				BackNavigationHandled = true;
				return true;
			}
			BackNavigationHandled = false;
			return false;
		}
	}
}
