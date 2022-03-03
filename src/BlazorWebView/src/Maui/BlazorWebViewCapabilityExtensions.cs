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
	/// Extension methods for well-known capabilities.
	/// </summary>
	public static class BlazorWebViewCapabilityExtensions
	{
		/// <summary>
		/// Determines whether or not <see cref="BlazorWebViewCapabilityNames.DevelopmentMode"/> is supported.
		/// </summary>
		/// <param name="capabilities">The <see cref="BlazorWebViewCapabilities"/>.</param>
		/// <returns><c>true</c> if <see cref="BlazorWebViewCapabilityNames.DevelopmentMode"/> is supported; <c>false</c> otherwise.</returns>
		public static bool IsDevelopmentModeSupported(this BlazorWebViewCapabilities capabilities) =>
			capabilities.IsSupported(BlazorWebViewCapabilityNames.DevelopmentMode);

		/// <summary>
		/// Sets the <see cref="BlazorWebViewCapabilityNames.DevelopmentMode"/> capability to the given <paramref name="value"/>.
		/// </summary>
		/// <param name="capabilities">The <see cref="BlazorWebViewCapabilities"/>.</param>
		/// <param name="value">The capability value.</param>
		public static void SetDevelopmentMode(this BlazorWebViewCapabilities capabilities, bool value) =>
			capabilities.SetCapability(BlazorWebViewCapabilityNames.DevelopmentMode, value);

		/// <summary>
		/// Gets the value of the <see cref="BlazorWebViewCapabilityNames.DevelopmentMode"/> capability.
		/// </summary>
		/// <param name="capabilities">The <see cref="BlazorWebViewCapabilities"/>.</param>
		public static bool GetDevelopmentMode(this BlazorWebViewCapabilities capabilities) =>
			(bool)capabilities.GetCapability(BlazorWebViewCapabilityNames.DevelopmentMode);
	}
}
