using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Xamarin.Forms.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class Performance
	{
		static readonly Dictionary<string, Stats> Statistics = new Dictionary<string, Stats>();

		[Conditional("PERF")]
		public static void Clear()
		{
			Statistics.Clear();
		}

		public static void Count(string tag = null, [CallerFilePath] string path = null, [CallerMemberName] string member = null)
		{
			string id = path + ":" + member + (tag != null ? "-" + tag : string.Empty);

			Stats stats;
			if (!Statistics.TryGetValue(id, out stats))
				Statistics[id] = stats = new Stats();

			stats.CallCount++;
		}

		[Conditional("PERF")]
		public static void DumpStats()
		{
			Debug.WriteLine(GetStats());
		}

		public static string GetStats()
		{
			var b = new StringBuilder();
			b.AppendLine("ID                                                                                 | Call Count | Total Time | Avg Time");
			foreach (KeyValuePair<string, Stats> kvp in Statistics.OrderBy(kvp => kvp.Key))
			{
				string key = ShortenPath(kvp.Key);
				double total = TimeSpan.FromTicks(kvp.Value.TotalTime).TotalMilliseconds;
				double avg = total / kvp.Value.CallCount;
				b.AppendFormat("{0,-80} | {1,-10} | {2,-10}ms | {3,-8}ms", key, kvp.Value.CallCount, total, avg);
				b.AppendLine();
			}
			return b.ToString();
		}

		[Conditional("PERF")]
		public static void Start(string tag = null, [CallerFilePath] string path = null, [CallerMemberName] string member = null)
		{
			string id = path + ":" + member + (tag != null ? "-" + tag : string.Empty);

			Stats stats;
			if (!Statistics.TryGetValue(id, out stats))
				Statistics[id] = stats = new Stats();

			stats.CallCount++;
			stats.StartTimes.Push(Stopwatch.GetTimestamp());
		}

		[Conditional("PERF")]
		public static void Stop(string tag = null, [CallerFilePath] string path = null, [CallerMemberName] string member = null)
		{
			string id = path + ":" + member + (tag != null ? "-" + tag : string.Empty);
			long stop = Stopwatch.GetTimestamp();

			Stats stats = Statistics[id];
			long start = stats.StartTimes.Pop();
			if (!stats.StartTimes.Any())
				stats.TotalTime += stop - start;
		}

		static string ShortenPath(string path)
		{
			int index = path.IndexOf("Xamarin.Forms.");
			if (index > -1)
				path = path.Substring(index + 14);

			return path;
		}

		class Stats
		{
			public readonly Stack<long> StartTimes = new Stack<long>();
			public int CallCount;
			public long TotalTime;
		}
	}
}