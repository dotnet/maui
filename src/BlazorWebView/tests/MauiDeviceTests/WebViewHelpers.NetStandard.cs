// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests
{
#if !WINDOWS && !ANDROID && !IOS && !MACCATALYST
	public static class WebViewHelpers
	{
		public static Task WaitForWebViewReady(object platformWebView)
		{
			return Task.CompletedTask;
		}

		public static Task WaitForControlDiv(object platformWebView, string controlValueToWaitFor)
		{
			return Task.CompletedTask;
		}

		public static Task<string> ExecuteScriptAsync(object platformWebView, string script)
		{
			return Task.FromResult<string>(null);
		}
	}
#endif
}
