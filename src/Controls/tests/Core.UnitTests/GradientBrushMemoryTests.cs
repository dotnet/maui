using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class GradientBrushMemoryTests : BaseTestFixture
	{
		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference CreateBrushWithSharedGradientStops(GradientStopCollection sharedStops)
		{
			var brush = new LinearGradientBrush { GradientStops = sharedStops };
			return new WeakReference(brush);
		}

		[Fact]
		public async Task GradientBrushDoesNotLeakWhenSharingGradientStops()
		{
			// A long-lived/shared GradientStopCollection, exactly as the issue describes.
			var sharedStops = new GradientStopCollection
			{
				new GradientStop()
			};
			var weakBrush = CreateBrushWithSharedGradientStops(sharedStops);

			Assert.False(await weakBrush.WaitForCollect(), "LinearGradientBrush should not be alive!");
			GC.KeepAlive(sharedStops);
		}

		[Fact]
		public async Task GradientStopChangesStillInvalidateAfterGc()
		{
			var stop = new GradientStop();
			var brush = new LinearGradientBrush
			{
				GradientStops = new GradientStopCollection { stop }
			};
			bool invalidated = false;
			brush.InvalidateGradientBrushRequested += (_, __) => invalidated = true;

			await TestHelpers.Collect();

			stop.Offset = 0.5f;

			Assert.True(invalidated);
			GC.KeepAlive(brush);
		}

		[Fact]
		public async Task SharedGradientStopsInvalidateEachLiveBrushAfterGc()
		{
			var stop = new GradientStop();
			var sharedStops = new GradientStopCollection { stop };
			var firstBrush = new LinearGradientBrush { GradientStops = sharedStops };
			var secondBrush = new LinearGradientBrush { GradientStops = sharedStops };
			int firstInvalidationCount = 0;
			int secondInvalidationCount = 0;
			firstBrush.InvalidateGradientBrushRequested += (_, __) => firstInvalidationCount++;
			secondBrush.InvalidateGradientBrushRequested += (_, __) => secondInvalidationCount++;

			await TestHelpers.Collect();

			stop.Offset = 0.5f;

			Assert.Equal(1, firstInvalidationCount);
			Assert.Equal(1, secondInvalidationCount);
			GC.KeepAlive(firstBrush);
			GC.KeepAlive(secondBrush);
		}

		[Fact]
		public async Task AliveBrushStillInvalidatesAfterSiblingBrushIsCollected()
		{
			var stop = new GradientStop();
			var sharedStops = new GradientStopCollection { stop };
			var weakCollectedBrush = CreateBrushWithSharedGradientStops(sharedStops);
			var aliveBrush = new LinearGradientBrush { GradientStops = sharedStops };
			int invalidationCount = 0;
			aliveBrush.InvalidateGradientBrushRequested += (_, __) => invalidationCount++;

			Assert.False(await weakCollectedBrush.WaitForCollect(), "Sibling LinearGradientBrush should not be alive!");

			stop.Offset = 0.5f;

			Assert.Equal(1, invalidationCount);
			GC.KeepAlive(aliveBrush);
			GC.KeepAlive(sharedStops);
		}

		[Fact]
		public void RemovingAndReplacingGradientStopsMovesSubscriptions()
		{
			var removedStop = new GradientStop { Offset = 0.1f };
			var retainedStop = new GradientStop { Offset = 0.2f };
			var oldStops = new GradientStopCollection { removedStop, retainedStop };
			var brush = new LinearGradientBrush { GradientStops = oldStops };
			int invalidationCount = 0;
			brush.InvalidateGradientBrushRequested += (_, __) => invalidationCount++;

			oldStops.Remove(removedStop);
			invalidationCount = 0;

			removedStop.Offset = 0.3f;
			Assert.Equal(0, invalidationCount);

			retainedStop.Offset = 0.4f;
			Assert.Equal(1, invalidationCount);

			var replacementStop = new GradientStop { Offset = 0.1f };
			brush.GradientStops = new GradientStopCollection { replacementStop };
			invalidationCount = 0;

			retainedStop.Offset = 0.5f;
			oldStops.Add(new GradientStop());

			Assert.Equal(0, invalidationCount);

			replacementStop.Offset = 0.6f;

			Assert.Equal(1, invalidationCount);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void ReplacingGradientStopsInvalidatesBrush(bool replaceWithNull)
		{
			var brush = new LinearGradientBrush
			{
				GradientStops = new GradientStopCollection { new GradientStop() }
			};
			int invalidationCount = 0;
			brush.InvalidateGradientBrushRequested += (_, __) => invalidationCount++;

			brush.GradientStops = replaceWithNull ? null : new GradientStopCollection();

			Assert.Equal(1, invalidationCount);
		}

		[Fact]
		public void NullGradientStopCollectionAllowsBindingContextChange()
		{
			var brush = new LinearGradientBrush { GradientStops = null };
			var bindingContext = new object();

			brush.BindingContext = bindingContext;

			Assert.Same(bindingContext, brush.BindingContext);
		}

		[Fact]
		public void NullGradientStopEntryAllowsBindingContextChange()
		{
			var brush = new LinearGradientBrush
			{
				GradientStops = new GradientStopCollection { null }
			};
			var bindingContext = new object();

			brush.BindingContext = bindingContext;

			Assert.Same(bindingContext, brush.BindingContext);
		}

		[Fact]
		public void SharedGradientStopsPreserveExistingMostRecentlyAssignedParentBehavior()
		{
			var stop = new GradientStop();
			var sharedStops = new GradientStopCollection { stop };
			var firstBindingContext = new object();
			var firstBrush = new LinearGradientBrush
			{
				BindingContext = firstBindingContext,
				GradientStops = sharedStops
			};

			Assert.Same(firstBrush, stop.Parent);
			Assert.Same(firstBindingContext, stop.BindingContext);

			var secondBindingContext = new object();
			var secondBrush = new LinearGradientBrush
			{
				BindingContext = secondBindingContext,
				GradientStops = sharedStops
			};

			Assert.Same(secondBrush, stop.Parent);
			Assert.Same(secondBindingContext, stop.BindingContext);
			GC.KeepAlive(firstBrush);
		}

		[Theory]
		[InlineData(SharedStopDetachment.ClearCollection)]
		[InlineData(SharedStopDetachment.RemoveStop)]
		[InlineData(SharedStopDetachment.ReplaceCollection)]
		public void DetachingSharedStopFromPreviousBrushPreservesCurrentParent(SharedStopDetachment detachment)
		{
			var stop = new GradientStop();
			var firstStops = new GradientStopCollection { stop };
			var secondStops = new GradientStopCollection { stop };
			var firstBrush = new LinearGradientBrush { GradientStops = firstStops };
			var secondBindingContext = new object();
			var secondBrush = new LinearGradientBrush
			{
				BindingContext = secondBindingContext,
				GradientStops = secondStops
			};

			switch (detachment)
			{
				case SharedStopDetachment.ClearCollection:
					firstStops.Clear();
					break;
				case SharedStopDetachment.RemoveStop:
					firstStops.Remove(stop);
					break;
				case SharedStopDetachment.ReplaceCollection:
					firstBrush.GradientStops = new GradientStopCollection();
					break;
			}

			Assert.Same(secondBrush, stop.Parent);
			Assert.Same(secondBindingContext, stop.BindingContext);
		}

		[Fact]
		public void DuplicateGradientStopsPreserveOccurrenceSubscriptions()
		{
			var sharedStop = new GradientStop();
			var stops = new GradientStopCollection { sharedStop, sharedStop };
			var brush = new LinearGradientBrush { GradientStops = stops };
			int invalidationCount = 0;
			brush.InvalidateGradientBrushRequested += (_, __) => invalidationCount++;

			sharedStop.Offset = 0.1f;
			Assert.Equal(2, invalidationCount);
			Assert.Same(brush, sharedStop.Parent);

			stops.Remove(sharedStop);
			invalidationCount = 0;

			sharedStop.Offset = 0.2f;
			Assert.Equal(1, invalidationCount);
			Assert.Same(brush, sharedStop.Parent);

			stops.Remove(sharedStop);
			invalidationCount = 0;

			sharedStop.Offset = 0.3f;
			Assert.Equal(0, invalidationCount);
			Assert.Null(sharedStop.Parent);
		}

		[Fact]
		public void ClearingGradientStopsAllowsReentrantReplacement()
		{
			var brush = new LinearGradientBrush
			{
				GradientStops = new GradientStopCollection { new GradientStop() }
			};
			bool replaced = false;
			brush.InvalidateGradientBrushRequested += (_, __) =>
			{
				if (!replaced && brush.GradientStops.Count == 0)
				{
					replaced = true;
					brush.GradientStops = new GradientStopCollection { new GradientStop() };
				}
			};

			brush.GradientStops.Clear();

			Assert.True(replaced);
			Assert.Single(brush.GradientStops);
		}

		[Fact]
		public async Task ClearingAndReusingGradientStopsKeepsNewStopsSubscribed()
		{
			var stops = new GradientStopCollection { new GradientStop() };
			var brush = new LinearGradientBrush
			{
				GradientStops = stops
			};
			bool invalidated = false;
			brush.InvalidateGradientBrushRequested += (_, __) => invalidated = true;

			stops.Clear();
			var newStop = new GradientStop();
			stops.Add(newStop);

			await TestHelpers.Collect();
			invalidated = false;

			newStop.Offset = 0.5f;

			Assert.True(invalidated);
			GC.KeepAlive(brush);
		}

		public enum SharedStopDetachment
		{
			ClearCollection,
			RemoveStop,
			ReplaceCollection,
		}
	}
}
