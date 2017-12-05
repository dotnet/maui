using System.Runtime.CompilerServices;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	internal static class BatchableExtensions
	{
		static readonly ConditionalWeakTable<IBatchable, BatchCount> s_counters = new ConditionalWeakTable<IBatchable, BatchCount>();

		public static void BatchBegin(this IBatchable target)
		{
			BatchCount value = null;

			if (s_counters.TryGetValue(target, out value))
			{
				value.Count++;
			}
			else
			{
				s_counters.Add(target, new BatchCount());
			}
		}

		public static void BatchCommit(this IBatchable target)
		{
			BatchCount value = null;
			if (s_counters.TryGetValue(target, out value))
			{
				value.Count--;
				if (value.Count == 0)
				{
					target.OnBatchCommitted();
				}
				else if (value.Count < 0)
				{
					Log.Error("Called BatchCommit() without BatchBegin().");
					value.Count = 0;
				}
			}
			else
			{
				Log.Error("Called BatchCommit() without BatchBegin().");
			}
		}

		public static bool IsBatched(this IBatchable target)
		{
			BatchCount value = null;

			if (s_counters.TryGetValue(target, out value))
			{
				return value.Count != 0;
			}
			else
			{
				return false;
			}
		}

		class BatchCount
		{
			public int Count = 1;
		}
	}
}
