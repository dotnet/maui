using System.Runtime.InteropServices;
using Maui.HybridApp;

namespace Maui.Controls.Sample.HybridWebApp.Playground;

sealed class SampleDiagnosticsProvider(SampleStartupIdentity startup)
{
	[HybridAction("sample.diagnostics")]
	internal DiagnosticsResult Read() => new(RuntimeInformation.ProcessArchitecture.ToString(),
		Environment.ProcessId, startup.StartupId, startup.StartedAt.ToString("O"));
}

sealed record SampleStartupIdentity(string StartupId, DateTimeOffset StartedAt);
sealed record DiagnosticsResult(string Architecture, int Pid, string StartupId, string StartedAt);
