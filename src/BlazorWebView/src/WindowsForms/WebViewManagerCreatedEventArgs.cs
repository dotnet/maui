// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Components.WebView.WebView2;

namespace Microsoft.AspNetCore.Components.WebView.WindowsForms
{
	public class WebViewManagerCreatedEventArgs
	{
		public WebViewManagerCreatedEventArgs(WebView2WebViewManager webViewManager)
		{
			WebViewManager = webViewManager;
		}

		public WebView2WebViewManager WebViewManager { get; }
	}
}
