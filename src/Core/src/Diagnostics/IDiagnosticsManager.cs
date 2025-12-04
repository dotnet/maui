using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Microsoft.Maui.Diagnostics;

internal interface IDiagnosticsManager
{
	ActivitySource ActivitySource { get; }

	Meter? Meter { get; }

	void GetTags(object source, out TagList tagList);

	T? GetMetrics<T>()
		where T : IDiagnosticMetrics;
}
