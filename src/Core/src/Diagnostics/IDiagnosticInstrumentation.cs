using System;
using System.Diagnostics;

namespace Microsoft.Maui.Diagnostics;

/// <summary>
/// Defines a contract for recording diagnostic events for instrumentation purposes.
/// </summary>
/// <remarks>
/// Implementations of this interface enable the collection and tracking of diagnostic information,
/// typically for logging, monitoring, or telemetry scenarios.
/// 
/// Any activities should be started automatically in the constructor of the implementing class. All
/// started activities should be stopped and disposed of when the implementing class is disposed.
/// </remarks>
internal interface IDiagnosticInstrumentation : IDisposable
{
	/// <summary>
	/// Called when the instrumentation is stopped.
	/// </summary>
	/// <remarks>
	/// This method is called when the instrumentation is stopped, allowing for any final
	/// metrics to be recorded or cleanup to be performed.
	/// </remarks>
	/// <param name="diagnostics">The <see cref="IDiagnosticsManager"/> instance.</param>
	/// <param name="tagList">The tags associated with the instrumentation.</param>
	void Stopped(IDiagnosticsManager diagnostics, in TagList tagList);
}
