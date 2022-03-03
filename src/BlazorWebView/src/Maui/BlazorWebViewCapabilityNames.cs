// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if WEBVIEW2_MAUI
namespace Microsoft.AspNetCore.Components.WebView.Maui
#elif WEBVIEW2_WPF
namespace Microsoft.AspNetCore.Components.WebView.Wpf
#else
namespace Microsoft.AspNetCore.Components.WebView.WindowsForms
#endif
{
	/// <summary>
	/// The names of the capabilities in <see cref="BlazorWebViewCapabilities"/>.
	/// </summary>
	public static class BlazorWebViewCapabilityNames
	{
		/// <summary>
		/// This capability represents the ability for the BlazorWebView to run in "development" mode, which enables
		/// instrospecting the underlying webview context via the developer tools for the platform.	
		/// </summary>
		/// <remarks>This mode is enabled by default when supported when the app is running under the debugger.</remarks>
		public static readonly string DevelopmentMode = nameof(DevelopmentMode);
	}
}
