using System.Diagnostics.Metrics;

namespace Microsoft.Maui.Diagnostics;

internal interface IDiagnosticMetrics
{
	void Create(Meter meter);
}
