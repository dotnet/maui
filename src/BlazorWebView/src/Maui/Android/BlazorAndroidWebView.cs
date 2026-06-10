using Android.Content;
using AWebView = Android.Webkit.WebView;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	/// <summary>
	/// A Blazor Web View implemented using <see cref="AWebView"/>.
	/// </summary>
	internal class BlazorAndroidWebView : AWebView
	{
		/// <summary>
		/// Initializes a new instance of <see cref="BlazorAndroidWebView"/>
		/// </summary>
		/// <param name="context">The <see cref="Context"/>.</param>
		public BlazorAndroidWebView(Context context) : base(context)
		{
		}
	}
}
