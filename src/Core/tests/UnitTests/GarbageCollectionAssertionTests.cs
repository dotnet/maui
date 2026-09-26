using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;
using GCAssertions = Microsoft.Maui.DeviceTests.AssertionExtensions;

namespace Microsoft.Maui.UnitTests
{
	public class GarbageCollectionAssertionTests
	{
		[Fact]
		public async Task WaitForCollectRechecksEarlierReferences()
		{
			Assert.True(await GCAssertions.WaitForCollect(CreateLateCollectedReferences()));
		}

		[Fact]
		public async Task WaitForGCAcceptsReferencesCollectedDuringLaterChecks()
		{
			await GCAssertions.WaitForGC(CreateLateCollectedReferences());
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(2)]
		public async Task WaitForGCRejectsRetainedReferenceAtEveryPosition(int retainedIndex)
		{
			var target = new object();
			var references = new[] { new WeakReference(null), new WeakReference(null), new WeakReference(null) };
			references[retainedIndex] = new WeakReference(target);

			try
			{
				var error = await Assert.ThrowsAsync<TrueException>(() => GCAssertions.WaitForGC(references));
				Assert.Contains("System.Object", error.Message, StringComparison.Ordinal);
			}
			finally
			{
				GC.KeepAlive(target);
			}
		}

		[Fact]
		public async Task WaitForGCAcceptsAlreadyCollectedReferences()
		{
			await GCAssertions.WaitForGC(new WeakReference(null), new WeakReference(null));
		}

		[Fact]
		public async Task WaitForGCRejectsEmptyReferences()
		{
			await Assert.ThrowsAsync<NotEmptyException>(() => GCAssertions.WaitForGC());
		}

		[Fact]
		public async Task WaitForGCRejectsNullReference()
		{
			await Assert.ThrowsAsync<NotNullException>(() => GCAssertions.WaitForGC(new WeakReference[] { null }));
		}

		static WeakReference[] CreateLateCollectedReferences()
		{
			bool firstIsAlive = true;
			bool laterIsAlive = true;
			return new WeakReference[]
			{
				new ObservedWeakReference(() => firstIsAlive),
				new ObservedWeakReference(() =>
				{
					if (laterIsAlive)
					{
						laterIsAlive = false;
						return true;
					}

					// Model the first target being released by a later reference's collection.
					firstIsAlive = false;
					return false;
				})
			};
		}

		sealed class ObservedWeakReference : WeakReference
		{
			readonly Func<bool> _isAlive;

			public ObservedWeakReference(Func<bool> isAlive) : base(null)
			{
				_isAlive = isAlive;
			}

			public override bool IsAlive => _isAlive();
		}
	}
}
