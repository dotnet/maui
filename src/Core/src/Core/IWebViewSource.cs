// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui
{
	/// <summary>
	/// Provide the data for a WebView.
	/// </summary>
	public interface IWebViewSource
	{
		/// <summary>
		/// Load the HTML content from the source.
		/// </summary>
		/// <param name="webViewDelegate">WebViewDelegate parameter.</param>
		void Load(IWebViewDelegate webViewDelegate);
	}
}