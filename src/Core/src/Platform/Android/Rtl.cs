// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Android.App;
using Android.Content.PM;

namespace Microsoft.Maui.Platform
{
	static class Rtl
	{
		/// <summary>
		/// True if /manifest/application@android:supportsRtl="true"
		/// </summary>
		public static readonly bool IsSupported =
			(Application.Context?.ApplicationInfo?.Flags & ApplicationInfoFlags.SupportsRtl) != 0;
	}
}
