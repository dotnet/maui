// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Diagnostics;

namespace Microsoft.Maui
{
	internal static class DebuggerHelper
	{
#if DEBUG
		static DebuggerHelper() => _mockDebuggerIsAttached = true;
#endif

		public static bool _mockDebuggerIsAttached;
		public static bool DebuggerIsAttached => _mockDebuggerIsAttached || Debugger.IsAttached;
	}
}
