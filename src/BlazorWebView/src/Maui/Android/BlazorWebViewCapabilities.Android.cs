using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	/// <summary>
	/// Represents the capabilities supported by the underlying webview component in the underlying platform.
	/// </summary>
	public partial class BlazorWebViewCapabilities
	{
#if ANDROID
		/// <summary>
		/// Initializes a new instance of <see cref="BlazorWebViewCapabilities"/>.
		/// </summary>
		public BlazorWebViewCapabilities()
		{
			DefineCommonCapabilities();
		}
#endif
	}
}
