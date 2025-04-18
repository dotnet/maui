using System.Diagnostics;

namespace Microsoft.Maui.Controls;

internal interface ILayoutPassTypeStats
{
	long StandaloneCount { get; }
	long Count { get; }
	long TotalTime { get; }
}

internal static class LayoutPassTypeStatsExtensions
{
	public static double GetAverageTimeNs(this ILayoutPassTypeStats stats)
	{
		if (stats.Count == 0)
		{
			return 0;
		}

		return (double)stats.TotalTime / stats.Count / Stopwatch.Frequency * 1000000.0;
	}
}