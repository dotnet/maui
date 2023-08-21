// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.Maui.ApplicationModel
{
	public static partial class MainThread
	{
		static void PlatformBeginInvokeOnMainThread(Action action) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		static bool PlatformIsMainThread =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
