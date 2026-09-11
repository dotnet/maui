using System;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class GradientBrushTests : BaseTestFixture
	{
		public static TheoryData<Type> GradientBrushTypes =>
			new()
			{
				typeof(LinearGradientBrush),
				typeof(RadialGradientBrush),
			};

		[Theory]
		[MemberData(nameof(GradientBrushTypes))]
		public async Task SharedGradientStopsDoNotRetainBrush(Type brushType)
		{
			var sharedGradientStops = new GradientStopCollection
			{
				new GradientStop(Colors.Red, 0),
				new GradientStop(Colors.Blue, 1),
			};

			var brushReference = CreateBrushReference(brushType, sharedGradientStops);

			Assert.False(await brushReference.WaitForCollect());
			GC.KeepAlive(sharedGradientStops);
		}

		[Theory]
		[MemberData(nameof(GradientBrushTypes))]
		public async Task SharedGradientStopCollectionWithMultipleBrushesDoesNotRetainAnyBrush(Type brushType)
		{
			var sharedGradientStops = new GradientStopCollection
			{
				new GradientStop(Colors.Red, 0),
				new GradientStop(Colors.Blue, 1),
			};

			var brushReferences = CreateBrushReferences(brushType, sharedGradientStops, 5);

			foreach (var reference in brushReferences)
			{
				Assert.False(await reference.WaitForCollect());
			}

			GC.KeepAlive(sharedGradientStops);
		}

		[Theory]
		[MemberData(nameof(GradientBrushTypes))]
		public async Task SharedGradientStopDoesNotRetainBrush(Type brushType)
		{
			var sharedStop = new GradientStop(Colors.Red, 0);

			var brushReference = CreateBrushReferenceWithSharedStop(brushType, sharedStop);

			Assert.False(await brushReference.WaitForCollect());
			GC.KeepAlive(sharedStop);
		}

		[Theory]
		[MemberData(nameof(GradientBrushTypes))]
		public async Task BrushUsingResourceStoredGradientStopsIsCollected(Type brushType)
		{
			var resources = new ResourceDictionary
			{
				["SharedGradientStops"] = new GradientStopCollection
				{
					new GradientStop(Colors.Red, 0),
					new GradientStop(Colors.Blue, 1),
				}
			};

			var brushReference = CreateBrushReference(brushType, (GradientStopCollection)resources["SharedGradientStops"]);

			Assert.False(await brushReference.WaitForCollect());
			GC.KeepAlive(resources);
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
		static WeakReference CreateBrushReference(Type brushType, GradientStopCollection gradientStops)
		{
			var brush = CreateGradientBrush(brushType);
			brush.GradientStops = gradientStops;
			return new WeakReference(brush);
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
		static WeakReference[] CreateBrushReferences(Type brushType, GradientStopCollection gradientStops, int count)
		{
			var references = new WeakReference[count];

			for (var index = 0; index < count; index++)
			{
				references[index] = CreateBrushReference(brushType, gradientStops);
			}

			return references;
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
		static WeakReference CreateBrushReferenceWithSharedStop(Type brushType, GradientStop sharedStop)
		{
			var brush = CreateGradientBrush(brushType);
			brush.GradientStops = new GradientStopCollection
			{
				sharedStop,
				new GradientStop(Colors.Blue, 1),
			};
			return new WeakReference(brush);
		}

		static GradientBrush CreateGradientBrush(Type brushType) =>
			(GradientBrush)Activator.CreateInstance(brushType);
	}
}
