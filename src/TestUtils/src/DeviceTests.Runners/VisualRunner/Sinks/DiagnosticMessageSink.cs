// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable
using System;
using Xunit;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner
{
	class DiagnosticMessageSink : DiagnosticEventSink
	{
		public DiagnosticMessageSink(Action<string> logger, string assemblyDisplayName, bool showDiagnostics)
		{
			if (showDiagnostics && logger != null)
			{
				DiagnosticMessageEvent += args => logger($"{assemblyDisplayName}: {args.Message.Message}");
			}
		}
	}
}