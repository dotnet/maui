using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Shapes;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class PathMemoryTests
	{
		// A shared / long-lived Geometry assigned to Path.Data must not keep the Path alive.
		// Path subscribes to Geometry.PropertyChanged (and PathGeometry.InvalidatePathGeometryRequested)
		// and previously did so with a plain (strong) delegate, rooting every Path for the
		// lifetime of the shared geometry.
		[Theory]
		[InlineData(typeof(EllipseGeometry))]
		[InlineData(typeof(RectangleGeometry))]
		[InlineData(typeof(PathGeometry))]
		public async Task PathDataDoesNotLeakWhenGeometryOutlivesIt(Type geometryType)
		{
			var sharedGeometry = (Geometry)Activator.CreateInstance(geometryType);

			var reference = new WeakReference(new Path { Data = sharedGeometry });

			Assert.False(await reference.WaitForCollect(), "Path should not be alive!");

			// Keep the shared geometry rooted until after measuring so the leak is observable.
			GC.KeepAlive(sharedGeometry);
		}

		// A shared / long-lived Transform assigned to Path.RenderTransform must not keep the Path alive.
		[Fact]
		public async Task PathRenderTransformDoesNotLeakWhenTransformOutlivesIt()
		{
			var sharedTransform = new RotateTransform { Angle = 45 };

			var reference = new WeakReference(new Path { RenderTransform = sharedTransform });

			Assert.False(await reference.WaitForCollect(), "Path should not be alive!");

			GC.KeepAlive(sharedTransform);
		}
	}
}
