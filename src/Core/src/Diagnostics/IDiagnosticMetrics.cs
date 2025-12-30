using System.Diagnostics.Metrics;

namespace Microsoft.Maui.Diagnostics;

/// <summary>
/// Defines a contract for creating diagnostic metrics using a specified <see cref="System.Diagnostics.Metrics.Meter"/>.
/// </summary>
internal interface IDiagnosticMetrics
{
	/// <summary>
	/// Called when the <see cref="System.Diagnostics.Metrics.Meter"/> is created to initialize the metrics.
	/// </summary>
	/// <param name="meter">The <see cref="System.Diagnostics.Metrics.Meter"/> instance to use for creating metrics.</param>
	void Create(Meter meter);
}
