// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
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
