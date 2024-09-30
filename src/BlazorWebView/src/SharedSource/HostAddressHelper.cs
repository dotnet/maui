// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Components.WebView;

internal static class HostAddressHelper
{
	private const string AppHostAddressAlways0000Switch = "BlazorWebView.AppHostAddressAlways0000";

	private static bool IsAppHostAddressAlways0000Enabled =>
		AppContext.TryGetSwitch(AppHostAddressAlways0000Switch, out var enabled) && enabled;

	public static string GetAppHostAddress()
		=> IsAppHostAddressAlways0000Enabled
			? "0.0.0.0"
			: "0.0.0.1";
}
