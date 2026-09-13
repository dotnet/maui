using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Animations;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class AnimationExtensionsThreadSafetyTests
	{
		[Fact]
		public void Add_ConcurrentCalls_ProducesUniqueIds()
		{
			// Exercises the REAL AnimationExtensions.Add() from multiple threads.
			// If the fix is reverted (s_currentTweener++ instead of Interlocked.Increment),
			// this test will fail with duplicate IDs under contention.
			var manager = new NoOpAnimationManager();
			const int threadCount = 50;
			const int callsPerThread = 200;
			var ids = new ConcurrentBag<int>();

			var tasks = new Task[threadCount];
			for (int i = 0; i < threadCount; i++)
			{
				tasks[i] = Task.Run(() =>
				{
					for (int j = 0; j < callsPerThread; j++)
					{
						int id = manager.Add(_ => { });
						ids.Add(id);
					}
				});
			}

			Task.WaitAll(tasks);

			int expectedCount = threadCount * callsPerThread;
			var uniqueIds = new HashSet<int>(ids);

			Assert.Equal(expectedCount, ids.Count);
			Assert.Equal(expectedCount, uniqueIds.Count);
		}

		[Fact]
		public void Insert_ConcurrentCalls_ProducesUniqueIds()
		{
			// Same as above but exercises AnimationExtensions.Insert().
			var manager = new NoOpAnimationManager();
			const int threadCount = 50;
			const int callsPerThread = 200;
			var ids = new ConcurrentBag<int>();

			var tasks = new Task[threadCount];
			for (int i = 0; i < threadCount; i++)
			{
				tasks[i] = Task.Run(() =>
				{
					for (int j = 0; j < callsPerThread; j++)
					{
						int id = manager.Insert(_ => true);
						ids.Add(id);
					}
				});
			}

			Task.WaitAll(tasks);

			int expectedCount = threadCount * callsPerThread;
			var uniqueIds = new HashSet<int>(ids);

			Assert.Equal(expectedCount, ids.Count);
			Assert.Equal(expectedCount, uniqueIds.Count);
		}

		/// <summary>
		/// Minimal no-op IAnimationManager so AnimationExtensions.Add/Insert can run
		/// without requiring a platform ticker (which would block or crash off-thread).
		/// </summary>
		sealed class NoOpAnimationManager : IAnimationManager
		{
			public ITicker Ticker => null!;
			public double SpeedModifier { get; set; } = 1;
			public bool AutoStartTicker { get; set; }
			public void Add(Microsoft.Maui.Animations.Animation animation) { }
			public void Remove(Microsoft.Maui.Animations.Animation animation) { }
		}
	}
}
