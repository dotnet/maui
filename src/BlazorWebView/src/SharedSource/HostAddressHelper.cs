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
	{
		if (IsAppHostAddressAlways0000Enabled)
		{
			return "0.0.0.0";
		}
		else
		{
#if IOS || MACCATALYST
			// The 0.0.0.0 address was used until iOS/MacCatalyst 18, where it stopped working.
			// We'll retain the previous behavior for older versions of those systems, while defaulting
			// to new behavior on the new system.

			// Note that pre-release versions of iOS/MacCatalyst have the expected Major/Minor values,
			// but the Build, MajorRevision, MinorRevision, and Revision values are all -1, so we need
			// to pass in int.MinValue for those values.
			if (!OperatingSystem.IsIOSVersionAtLeast(major: 18, minor: int.MinValue, build: int.MinValue) &&
				!OperatingSystem.IsMacCatalystVersionAtLeast(major: 18, minor: int.MinValue, build: int.MinValue))
			{
				return "0.0.0.0";
			}
#endif
			return "localhost";
		}
	}
}
