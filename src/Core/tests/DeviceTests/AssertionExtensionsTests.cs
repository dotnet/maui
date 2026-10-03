using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Memory)]
	public class AssertionExtensionsTests
	{
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task EarlierReferenceCollectedDuringLaterWait(bool useWaitForGC)
		{
			var root = new StrongRoot();
			var references = CreateReferences(root, releaseDuringLaterWait: true);

			if (useWaitForGC)
				await AssertionExtensions.WaitForGC(references);
			else
				Assert.True(await AssertionExtensions.WaitForCollect(references));

			Assert.True(root.EarlierReferenceWasAlive);
			Assert.All(references, reference => Assert.False(reference.IsAlive));
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task RetainedReferenceStillFails(bool retainedReferenceFirst)
		{
			var root = new StrongRoot();
			var references = CreateReferences(root, releaseDuringLaterWait: false);
			if (!retainedReferenceFirst)
				Array.Reverse(references);

			try
			{
				Assert.False(await AssertionExtensions.WaitForCollect(references));
				var error = await Assert.ThrowsAsync<TrueException>(() => AssertionExtensions.WaitForGC(references));
				Assert.Contains("Reference to System.Object", error.Message, StringComparison.Ordinal);
			}
			finally
			{
				GC.KeepAlive(root.Target);
			}
		}

		[Fact]
		public async Task AlreadyCollectedReferencesSucceed()
		{
			var references = new[] { new WeakReference(null), new WeakReference(null) };
			Assert.True(await AssertionExtensions.WaitForCollect(references));
			await AssertionExtensions.WaitForGC(references);
		}

		[Fact]
		public async Task EmptyCollectionIsCollected()
		{
			Assert.True(await AssertionExtensions.WaitForCollect(Array.Empty<WeakReference>()));
		}

		[Fact]
		public async Task WaitForGCRejectsEmptyCollection()
		{
			await Assert.ThrowsAsync<NotEmptyException>(() => AssertionExtensions.WaitForGC(Array.Empty<WeakReference>()));
		}

		[Fact]
		public async Task NullReferenceIsRejected()
		{
			var references = new WeakReference[] { null };
			await Assert.ThrowsAsync<NotNullException>(() => AssertionExtensions.WaitForCollect(references));
			await Assert.ThrowsAsync<NotNullException>(() => AssertionExtensions.WaitForGC(references));
		}

		[Fact]
		public async Task NullCollectionIsRejected()
		{
			await Assert.ThrowsAsync<NullReferenceException>(() => AssertionExtensions.WaitForCollect((WeakReference[])null));
			await Assert.ThrowsAsync<ArgumentNullException>(() => AssertionExtensions.WaitForGC((WeakReference[])null));
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference[] CreateReferences(StrongRoot root, bool releaseDuringLaterWait)
		{
			root.Target = new object();
			var first = new WeakReference(root.Target);
			var second = releaseDuringLaterWait
				? new ReleaseOnCheckReference(new object(), root, first)
				: new WeakReference(new object());
			return new[] { first, second };
		}

		sealed class StrongRoot
		{
			public object Target;
			public bool EarlierReferenceWasAlive;
		}

		sealed class ReleaseOnCheckReference : WeakReference
		{
			readonly StrongRoot _root;
			readonly WeakReference _earlierReference;

			public ReleaseOnCheckReference(object target, StrongRoot root, WeakReference earlierReference)
				: base(target)
			{
				_root = root;
				_earlierReference = earlierReference;
			}

			public override bool IsAlive
			{
				get
				{
					if (_root.Target is not null)
					{
						// Release the real target only after its own bounded collection wait.
						_root.EarlierReferenceWasAlive = _earlierReference.IsAlive;
						_root.Target = null;
						GC.Collect();
						GC.WaitForPendingFinalizers();
						GC.Collect();
					}

					return base.IsAlive;
				}
			}
		}
	}
}
